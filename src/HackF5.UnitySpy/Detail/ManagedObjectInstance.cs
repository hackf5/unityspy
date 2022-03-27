namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.Collections.Generic;

    public abstract class ManagedObjectInstance : MemoryObject, IManagedObjectInstance
    {
        private readonly List<TypeInfo> genericTypeArguments;

        protected ManagedObjectInstance(AssemblyImage image, List<TypeInfo> genericTypeArguments, IntPtr address)
            : base(image, address)
        {
            this.genericTypeArguments = genericTypeArguments;
        }

        ITypeDefinition IManagedObjectInstance.TypeDefinition => this.TypeDefinition;

        public abstract TypeDefinition TypeDefinition { get; }

        public dynamic this[string fieldName] => this.GetValue<dynamic>(fieldName);

        public dynamic this[string fieldName, string typeFullName] => this.GetValue<dynamic>(fieldName, typeFullName);

        public TValue GetValue<TValue>(string fieldName) => this.GetValue<TValue>(fieldName, default);

        public TValue GetValue<TValue>(string fieldName, string typeFullName)
        {
            var field = this.TypeDefinition.GetField(fieldName, typeFullName)
                ?? throw new ArgumentException(
                    $"No field exists with name {fieldName} in type {typeFullName ?? "<any>"}.");

            return field.GetValue<TValue>(this.genericTypeArguments, this.Address);
        }
    }
}