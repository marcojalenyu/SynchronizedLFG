using System;

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

    public void Activate()
    {
        this.active = true;
    }

    public void Run(Party party, uint clearTime)
    {
        // Console.WriteLine($"Starting Instance {this.id} with party {party.id}");
        Thread.Sleep((int)clearTime * 1000);

        // Console.WriteLine($"Instance {this.id} - {party.id} for {clearTime} s");
        totalPartiesServed++;
        totalTimeServed += clearTime;
        this.active = false;
    }
}
