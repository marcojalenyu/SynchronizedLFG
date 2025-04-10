﻿using System;
using System.Collections.Generic;
using System.IO;

/**
 *  This class holds the specifications for LFG queueing based on config.txt:
 *  maxInstances    (n) - maximum number of concurrent instances
 *  numTanks        (t) - number of tank players in the queue
 *  numHealers      (h) - number of healer players in the queue
 *  numDPS          (d) - number of DPS players in the queue
 *  minTimeFinish   (t1) - minimum time before an instance is finished
 *  maxTimeFinish   (t2) - maximum time before an instance is finished
*/
class Config
{
    // Thread-safe singleton (Source: https://csharpindepth.com/articles/Singleton)
    private static Config? instance = null;
    private static readonly object padlock = new object();

    // Configurations
    public uint maxInstances { get; private set; }
    public uint numTanks { get; private set; }
    public uint numHealers { get; private set; }
    public uint numDPS { get; private set; }
    public uint minTimeFinish { get; private set; }
    public uint maxTimeFinish { get; private set; }

    // To track which needs to be set
    private readonly Dictionary<string, bool> keysSet = new Dictionary<string, bool>
    {
        { "n", false },
        { "t", false },
        { "h", false },
        { "d", false },
        { "t1", false },
        { "t2", false }
    };

    // Thread-safe singleton constructors
    private Config()
    {
        this.Initialize();
    }

    public static Config Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new Config();
                }
                return instance;
            }
        }
    }

    /**
     *  Extracts the values of config.txt
     */
    private void Initialize()
    {
        try
        {
            // To access config.txt
            string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.Split("bin")[0]);
            string configFilePath = Path.Combine(projectRoot, "config.txt");

            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException();
            }
            else
            {
                string[] lines = File.ReadAllLines(configFilePath);
                this.SetConfig(lines);
                this.SetInvalidToDefault();
                this.ValidateValueRange();
            }

        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Config file not found. Setting default values.");
            this.SetInvalidToDefault();
        }
        catch (Exception)
        {
            Console.WriteLine("Error occurred. Setting default values.");
            this.SetInvalidToDefault();
        }
    }

    /**
     * Set default values for the configuration:
     * maxInstances = 3
     * numTanks     = 10
     * numHealers   = 10
     * numDPS       = 10
     * minTimeFinish = 5
     * maxTimeFinish = 15
     */
    private void SetInvalidToDefault()
    {
        if (!keysSet["n"]) this.maxInstances = 3;
        if (!keysSet["t"]) this.numTanks = 10;
        if (!keysSet["h"]) this.numHealers = 10;
        if (!keysSet["d"]) this.numDPS = 10;
        if (!keysSet["t1"]) this.minTimeFinish = 5;
        if (!keysSet["t2"]) this.maxTimeFinish = 15;
    }

    /**
     * Set the configurations based on the values extracted from config.txt
     * Note: All values have to be a uint, and 0 < t1 <= t2 <= 15
     */
    private void SetConfig(string[] lines)
    {
        foreach (string line in lines)
        {
            string[] parts = line.Split(' ');

            // To only allow 2 parts (key and value) and to ensure value is a number
            if (parts.Length != 2 || !uint.TryParse(parts[1], out uint value))
            {
                Console.WriteLine($"Invalid format/value for {parts[0]}.");
                continue;
            }

            // To ensure only valid keys are used and no duplicates
            if (!keysSet.ContainsKey(parts[0]))
            {
                Console.WriteLine($"Invalid key for {parts[0]}.");
                continue;
            }
            if (keysSet[parts[0]])
            {
                Console.WriteLine($"Duplicate key for {parts[0]}.");
                continue;
            }

            keysSet[parts[0]] = true;

            switch (parts[0])
            {
                case "n":
                    this.maxInstances = value;
                    break;
                case "t":
                    this.numTanks = value;
                    break;
                case "h":
                    this.numHealers = value;
                    break;
                case "d":
                    this.numDPS = value;
                    break;
                case "t1":
                    this.minTimeFinish = value;
                    break;
                case "t2":
                    this.maxTimeFinish = value;
                    break;
            }
        }
    }

    /**
     * Validate time values (t1 and t2) to ensure 0 < t1 <= t2 <= 15
     * Validate n to ensure 0 < n <= 1000
     */
    private void ValidateValueRange()
    {
        if (this.maxInstances == 0)
        {
            Console.WriteLine("Error: n = 0, setting n = 3.");
            this.maxInstances = 3;
        }
        else if (this.maxInstances > 1000)
        {
            Console.WriteLine("Error: n > 1000, setting n = 1000.");
            this.maxInstances = 1000;
        }
        if (this.maxTimeFinish == 0)
        {
            Console.WriteLine("Error: t2 = 0, setting t2 = 1.");
            this.maxTimeFinish = 1;
        }
        else if (this.maxTimeFinish > 15)
        {
            Console.WriteLine("Error: t2 > 15, setting t2 = 15.");
            this.maxTimeFinish = 15;
        }
        if (this.minTimeFinish == 0)
        {
            Console.WriteLine("Error: t1 = 0, setting t1 = 1.");
            this.minTimeFinish = 1;
        }
        else if (this.minTimeFinish > this.maxTimeFinish)
        {
            Console.WriteLine("Error: t1 > t2, setting t1 = t2.");
            this.minTimeFinish = this.maxTimeFinish;
        }
    }

    /**
     * Update the number of players in the queue after matchmaking
     */
    public void UpdatePlayers(uint t, uint h, uint d)
    {
        this.numTanks = t;
        this.numHealers = h;
        this.numDPS = d;
    }

    /**
     * Print the player count
     */
    public void PrintRemainingPlayers()
    {
        Console.WriteLine("\nRemaining Players:");
        Console.WriteLine($"Tanks: {this.numTanks}");
        Console.WriteLine($"Healers: {this.numHealers}");
        Console.WriteLine($"DPS: {this.numDPS}");
    }
}