using System;
using System.IO;

namespace SynchronizedLFG
{
    class Program
    {
        private static List<Instance> instances = new List<Instance>();
        private static List<Thread> instanceThreads = new List<Thread>();
        private static Queue<Party> partyQueue = new Queue<Party>();
        private static Random random = new Random();
        private static SemaphoreSlim semaphore;
        private static object queueLock = new object();
        private static object printLock = new object();
        private static AutoResetEvent statusChangedEvent = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            Config config = Config.Instance;

            SetInstances(config.maxInstances);
            SetParties(config);

            semaphore = new SemaphoreSlim((int)config.maxInstances);

            Thread statusThread = new Thread(StatusWorker);
            statusThread.Start();

            foreach (var instance in instances)
            {
                Thread instanceWorker = new Thread(InstanceWorker);
                instanceThreads.Add(instanceWorker);
                instanceWorker.Start();
            }

            foreach (var instanceThread in instanceThreads)
            {
                instanceThread.Join();
            }

            statusChangedEvent.Set();
            statusThread.Join();

            PrintSummary();
            config.PrintRemainingPlayers();
        }

        private static void InstanceWorker()
        {
            while (true)
            {
                semaphore.Wait();

                Party party;
                lock (queueLock)
                {
                    if (partyQueue.Count == 0)
                    {
                        semaphore.Release();
                        break;
                    }
                    party = partyQueue.Dequeue();
                }

                Instance? instance = null;
                while (instance == null)
                {
                    lock (instances)
                    {
                        instance = GetAvailableInstance();
                        if (instance != null)
                        {
                            instance.Activate();
                        }
                    }

                    if (instance == null)
                    {
                        semaphore.Release();
                        semaphore.Wait();
                    }
                }

                if (instance != null)
                {
                    uint clearTime = (uint)random.Next((int)Config.Instance.minTimeFinish, (int)Config.Instance.maxTimeFinish);
                    statusChangedEvent.Set();
                    instance.Run(party, clearTime);
                    statusChangedEvent.Set();
                }                

                semaphore.Release();
            }
        }

        private static void StatusWorker()
        {
            while (true)
            {
                statusChangedEvent.WaitOne();
                PrintStatuses();

                if (partyQueue.Count == 0 && instances.All(i => !i.active))
                {
                    break;
                }
            }
        }

        // Create instances based on the maxInstances
        private static void SetInstances(uint maxInstances)
        {
            for (uint i = 0; i < maxInstances; i++)
            {
                instances.Add(new Instance(i));
            }
        }

        // Create parties based on the number of tanks, healers, and DPS
        private static void SetParties(Config config)
        {
            uint partyId = 0;
            uint t = config.numTanks;
            uint h = config.numHealers;
            uint d = config.numDPS;
            // Each party has 1 tank, 1 healer, and 3 DPS
            while (t >= 1 && h >= 1 && d >= 3)
            {
                Party newParty = new Party(partyId);
                partyQueue.Enqueue(newParty);
                partyId++;
                t--;
                h--;
                d -= 3;
            }
            config.UpdatePlayers(t, h, d);
        }

        private static Instance? GetAvailableInstance()
        {
            foreach (var instance in instances)
            {
                if (!instance.active)
                {
                    return instance;
                }
            }
            return null;
        }

        /**
         * Print current status of all available instances
         * - if there is a party in the instance, the status should say "active"
         * - if the instance is empty, the status should say "empty"
         */
        private static void PrintStatuses()
        {
            lock (printLock)
            {
                Console.WriteLine("\nStatus:");
                foreach (var instance in instances)
                {
                    string status = instance.active ? "active" : "empty";
                    Console.WriteLine($"Instance {instance.id}: {status}");
                }
            }
        }

        // Print the summary of each instance
        private static void PrintSummary()
        {
            Console.WriteLine("\nSummary:");
            foreach (var instance in instances)
            {
                Console.WriteLine($"Instance {instance.id}: {instance.totalPartiesServed} parties, {instance.totalTimeServed} seconds.");
            }
        }
    }
}
