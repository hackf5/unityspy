namespace HackF5.UnitySpy.Detail
{
    /// <summary>
    /// The base type for all objects accessed in a process' memory. Every object has an address in memory
    /// and all information about that object is accessed via an offset from that address.
    /// </summary>
    public abstract class MemoryObject : IMemoryObject
    {
    }
}