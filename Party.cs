using System;

/**
 *  This class holds the instance of a party
 *  id - the unique identifier of the party
 */
class Party
{
    public uint id { get; private set; }

    public Party(uint id)
    {
        this.id = id;
    }
}
