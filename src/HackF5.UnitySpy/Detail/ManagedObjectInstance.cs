namespace HackF5.UnitySpy.Detail
{
    using System;

    public abstract class ManagedObjectInstance : MemoryObject, IManagedObjectInstance
    {
        protected ManagedObjectInstance(AssemblyImage image, uint address)
            : base(image, address)
        {
        }

        ITypeDefinition IManagedObjectInstance.TypeDefinition => this.TypeDefinition;

        public abstract TypeDefinition TypeDefinition { get; }

        public TValue GetValue<TValue>(string fieldName) => this.GetValue<TValue>(fieldName, default);

        public TValue GetValue<TValue>(string fieldName, string typeFullName)
        {
            var field = this.TypeDefinition.GetField(fieldName, typeFullName)
                ?? throw new ArgumentException(
                    $"No field exists with name {fieldName} in type {typeFullName ?? "<any>"}.");

            return field.GetValue<TValue>(this.Address);
        }
    }
}