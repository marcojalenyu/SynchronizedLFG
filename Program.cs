using System;
using System.IO;

namespace SynchronizedLFG
{
    class Program
    {
        private static List<Instance> instances = new List<Instance>();
        private static Queue<Party> partyQueue = new Queue<Party>();
        private static Random random = new Random();
        private static readonly object lockObject = new object();
        static void Main(string[] args)
        {
            Config config = Config.Instance;

            for (uint i = 0; i < config.maxInstances; i++)
            {
                instances.Add(new Instance(i));
            }

            uint partyId = 0;
            // To prevent starvation, there must be at least 1 tank, 1 healer, and 3 DPS in the queue
            while (config.numTanks >= 1 && config.numHealers >= 1 && config.numDPS >= 3)
            {
                Party newParty = new Party(partyId);
                partyQueue.Enqueue(newParty);
                partyId++;
                config.numTanks--;
                config.numHealers--;
                config.numDPS -= 3;
            }

            List<Thread> threads = new List<Thread>();
            Party? party = null; // Initialize party to null
            while (partyQueue.Count > 0)
            {
                Instance? instance = GetNextInactiveInstance();
                if (instance != null)
                {
                    // Console.WriteLine($"Parties left: {partyQueue.Count}");
                    party = partyQueue.Dequeue();

                    if (party != null)
                    {
                        uint time = (uint)random.Next((int)config.minTimeFinish, (int)config.maxTimeFinish);
                        Thread thread = new Thread(() => instance.Run(party, time));
                        threads.Add(thread);
                        thread.Start();
                    }
                }
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            Console.WriteLine("Summary:");
            foreach (Instance inst in instances)
            {
                Console.WriteLine($"Instance {inst.id} served {inst.totalPartiesServed} parties for a total of {inst.totalTimeServed} seconds.");
            }
        }

        private static Instance? GetNextInactiveInstance()
        {
            lock (lockObject)
            {
                foreach (Instance instance in instances)
                {
                    if (!instance.active)
                    {
                        return instance;
                    }
                }
            }
            return null;
        }
    }
}
