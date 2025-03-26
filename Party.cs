/**
 *  This class holds the instance of a party
 *  (While this can be a primitive type, it is a class for readability)
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
