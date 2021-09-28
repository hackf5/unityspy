namespace HackF5.UnitySpy.Detail
{
    /// <summary>
    /// The base type for all objects accessed in a process' memory. Every object has an address in memory
    /// and all information about that object is accessed via an offset from that address.
    /// </summary>
    public abstract class MemoryObject : IMemoryObject
    {
        protected MemoryObject(AssemblyImage image, uint address)
        {
            this.Image = image;
            this.Address = address;
        }

        public uint GetAddress()
        {
            return this.Address;
        }

        IAssemblyImage IMemoryObject.Image => this.Image;

        public virtual AssemblyImage Image { get; }

        public virtual ProcessFacade Process => this.Image.Process;

        protected uint Address { get; }

        protected int ReadInt32(uint offset) => this.Process.ReadInt32(this.Address + offset);

        protected uint ReadPtr(uint offset) => this.Process.ReadPtr(this.Address + offset);

        protected string ReadString(uint offset) => this.Process.ReadAsciiStringPtr(this.Address + offset);

        protected uint ReadUInt32(uint offset) => this.Process.ReadUInt32(this.Address + offset);

        protected byte ReadByte(uint offset) => this.Process.ReadByte(this.Address + offset);
    }
}