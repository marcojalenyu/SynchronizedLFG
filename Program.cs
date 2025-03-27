using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

/**
 * This class runs the LFG system with synchronized threads 
 */
namespace SynchronizedLFG
{
    class Program
    {
        // For the queueing logic
        private static List<Instance> instances = new List<Instance>();
        private static Queue<Party> partyQueue = new Queue<Party>();
        private static Random random = new Random();
        private static uint lastUsedInstance = uint.MaxValue;
        // Locks and events to handle thread synchronization
        private static List<Thread> instanceThreads = new List<Thread>();
        private static object queueLock = new object();
        private static object printLock = new object();
        private static object instanceLock = new object();
        private static AutoResetEvent statusChangedEvent = new AutoResetEvent(false);

        /**
         * Main method
         */
        static void Main(string[] args)
        {
            Config config = Config.Instance;
            SetInstances(config.maxInstances);
            SetParties(config);

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

        /**
         * Worker thread for each instance:
         * It will wait for an instance to be available, then run the instance with a party
         * Locks are used to ensure thread safety when accessing the party queue
         */
        private static void InstanceWorker()
        {
            while (true)
            {
                Party party;
                // To ensure only one party is dequeued at a time
                lock (queueLock)
                {
                    if (partyQueue.Count == 0)
                    {
                        break;
                    }
                    party = partyQueue.Dequeue();
                }

                Instance? instance = null;
                while (instance == null)
                {
                    instance = GetAvailableInstance();
                    if (instance != null)
                    {
                        instance.Activate();
                    }
                }

                if (instance != null)
                {
                    uint clearTime = (uint)random.Next((int)Config.Instance.minTimeFinish, (int)Config.Instance.maxTimeFinish);
                    // To signal that the status has changed (and trigger status printing)
                    statusChangedEvent.Set();
                    instance.Run(party, clearTime);
                    statusChangedEvent.Set();
                }                
            }
        }

        /**
         * Print the status of all instances when the statusChangedEvent is set
         * (i.e. when a party is added to the queue or an instance is cleared)
         */
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

        /**
         * Create instances based on the max number of instances
         */
        private static void SetInstances(uint maxInstances)
        {
            for (uint i = 0; i < maxInstances; i++)
            {
                instances.Add(new Instance(i));
            }
        }

        /**
         * Create parties based on the number of tanks, healers, and DPS
         * Each party has 1 tank, 1 healer, and 3 DPS
         */
        private static void SetParties(Config config)
        {
            uint partyId = 0;
            uint t = config.numTanks;
            uint h = config.numHealers;
            uint d = config.numDPS;
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

        /**
         * Get the next available instance in a thread-safe and "fairer" manner
         * Note: (int) typecast is used because indices have to be integers
         */
        private static Instance? GetAvailableInstance()
        {
            // To ensure threads do not return the same instance
            lock (instanceLock)
            {
                uint instancesCount = (uint)instances.Count;
                uint startIndex = (lastUsedInstance == uint.MaxValue) ? 0 : (lastUsedInstance + 1) % instancesCount;
                for (uint i = 0; i < instances.Count; i++)
                {
                    // To ensure it will restart from the beginning if no available instance is found
                    uint index = (startIndex + i) % instancesCount;
                    if (!instances[(int)index].active)
                    {
                        lastUsedInstance = index;
                        return instances[(int)index];
                    }
                }
            }
            return null;
        }

        /**
         * Print current status of all available instances:
         * - if there is a party in the instance, the status should say "active"
         * - if the instance is empty, the status should say "empty"
         */
        private static void PrintStatuses()
        {
            // To ensure it only prints once even if multiple threads are calling it
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

        /**
         * Print the summary of all instances after all possible parties have been served
         */
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
