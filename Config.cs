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

    private uint maxInstances;
    private uint numTanks;
    private uint numHealers;
    private uint numDPS;
    private uint minTimeFinish;
    private uint maxTimeFinish;

    // Thread-safe singleton constructors
    private Config() 
    { 
        this.ExtractConfig();
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
    private void ExtractConfig()
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

            
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine("Config file not found. Please create a config.txt file in the root directory.");
            this.SetDefaultConfig();
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while reading the config file.");
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
        Console.WriteLine("Setting default configurations.");
        this.maxInstances = 3;
        this.numTanks = 10;
        this.numHealers = 10;
        this.numDPS = 10;
        this.minTimeFinish = 5;
        this.maxTimeFinish = 15;
    }
}