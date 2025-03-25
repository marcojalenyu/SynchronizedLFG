using System;

class Instance
{
    public readonly uint id;
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

    public void Run(Party party, uint time)
    {
        this.active = true;
        // Console.WriteLine($"Instance {id} is serving party {party.id} for {time} seconds.");
        Thread.Sleep((int)time * 1000);

        this.active = false;
        this.totalPartiesServed++;
        this.totalTimeServed += time;
        
        Console.WriteLine($"Instance {id} has finished serving party {party.id} for {time} seconds.");
    }
}
