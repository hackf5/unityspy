namespace HackF5.UnitySpy.Detail
{
    using System;

    public abstract class ManagedObjectInstance : MemoryObject, IManagedObjectInstance
    {
        protected ManagedObjectInstance(AssemblyImage image, uint address)
            : base()
        {
        }
    }
}