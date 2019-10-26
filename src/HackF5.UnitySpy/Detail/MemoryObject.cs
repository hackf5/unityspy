namespace HackF5.UnitySpy.Detail
{
    /// <summary>
    /// The base type for all objects accessed in a process' memory. Every object has an address in memory
    /// and all information about that object is accessed via an offset from that address.
    /// </summary>
    public abstract class MemoryObject : IMemoryObject
    {
        protected MemoryObject(IAssemblyImage image, uint address)
        {
        }

        IAssemblyImage IMemoryObject.Image => null;

        public virtual IAssemblyImage Image { get; }

        public virtual ProcessFacade Process => null;

        protected uint Address { get; }

        protected int ReadInt32(uint offset) => 0;

        protected uint ReadPtr(uint offset) => 0;

        protected string ReadString(uint offset) => null;

        protected uint ReadUInt32(uint offset) => 0;
    }
}