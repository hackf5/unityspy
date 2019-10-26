namespace HackF5.UnitySpy.Detail
{
    using System;

    public abstract class ManagedObjectInstance : MemoryObject, IManagedObjectInstance
    {
        protected ManagedObjectInstance(IAssemblyImage image, uint address)
            : base(image, address)
        {
        }

        ITypeDefinition IManagedObjectInstance.TypeDefinition => null;

        public abstract ITypeDefinition TypeDefinition { get; }

        public dynamic this[string fieldName] => null;

        public dynamic this[string fieldName, string typeFullName] => null;

        public TValue GetValue<TValue>(string fieldName) => default(TValue);

        public TValue GetValue<TValue>(string fieldName, string typeFullName)
        {
            return default(TValue);
        }
    }
}