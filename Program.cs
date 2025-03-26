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
        private static SemaphoreSlim semaphore;
        static void Main(string[] args)
        {
            Config config = Config.Instance;

            SetInstances(config.maxInstances);
            SetParties(config);

            PrintSummary();
            config.PrintRemainingPlayers();
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
