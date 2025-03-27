using System;
using System.Threading;

/**
 * Represents a dungeon instance, in which a party can enter and clear.
 * Only one party can be in an instance at a time.
 * id     - unique identifier of the instance
 * active - whether the instance is currently being used by a party
 * totalPartiesServed - the total number of parties that have cleared the instance
 * totalTimeServed    - the total time spent by all parties in the instance
 */
class Instance
{
    public uint id { get; private set; }
    public bool active { get; private set; }
    public uint totalPartiesServed { get; private set; }
    public uint totalTimeServed { get; private set; }

    public Instance(uint id)
    {
        this.id = id;
        this.active = false;
        this.totalPartiesServed = 0;
        this.totalTimeServed = 0;
    }

    /**
     * Activates the instance, allowing a party to enter
     */
    public void Activate()
    {
        this.active = true;
    }

    /**
     * Runs the instance with a party for a given clear time
     * party     - the party that is clearing the instance
     * clearTime - the time it takes for the party to clear the instance
     * (party is only called for debugging purposes and readability)
     */
    public void Run(Party party, uint clearTime)
    {
        // Console.WriteLine($"Starting Instance {this.id} with party {party.id}");
        Thread.Sleep((int)clearTime * 1000);
        // Console.WriteLine($"Finished Instance {this.id} with party {party.id} in {clearTime} s");
        this.totalPartiesServed++;
        this.totalTimeServed += clearTime;
        this.active = false;
    }
}
