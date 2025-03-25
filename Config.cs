using System;
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
    private static Config instance = null;
    private static readonly object padlock = new object();

    // Configurations
    private uint maxInstances;
    private uint numTanks;
    private uint numHealers;
    private uint numDPS;
    private uint minTimeFinish;
    private uint maxTimeFinish;

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
            // Get the file path of config.txt
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
            }
            
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine("Config file not found. Setting default values.");
            this.SetDefaultConfig();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occurred. Setting default values.");
            this.SetDefaultConfig();
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
    private void SetDefaultConfig()
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
     * Note: All values have to be a uint, and t1 <= t2 <= 15
     */
    private void SetConfig(string[] lines)
    {
        Console.WriteLine("Setting configurations from config.txt");
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

        // To ensure t1 <= t2 <= 15
        if (this.maxTimeFinish > 15)
        {
            Console.WriteLine("Error: t2 > 15, setting t2 = 15.");
            this.maxTimeFinish = 15;
        }
        if (this.minTimeFinish > this.maxTimeFinish)
        {
            Console.WriteLine("Error: t1 > t2, setting t1 = t2");
            this.minTimeFinish = this.maxTimeFinish;
        }

        // To set default values for keys that were not set
        this.SetDefaultConfig();
    }
}