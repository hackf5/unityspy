namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents an unmanaged _MonoClass instance in a Mono process. This object describes the type of a class or
    /// struct. The .NET equivalent is <see cref="System.Type"/>.
    /// See: _MonoClass in https://github.com/Unity-Technologies/mono/blob/unity-master/mono/metadata/class-internals.h.
    /// </summary>
    [PublicAPI]
    public class TypeDefinition : MemoryObject, ITypeDefinition
    {
        public TypeDefinition([NotNull] IAssemblyImage image, uint address)
            : base(image, address)
        {
        }

        IReadOnlyList<IFieldDefinition> ITypeDefinition.Fields => null;

        public string FullName => null;

        public bool IsEnum => false;

        public bool IsValueType => false;

        public string Name { get; }

        public string NamespaceName { get; }

        ITypeInfo ITypeDefinition.TypeInfo => null;

        public dynamic this[string fieldName] => null;

        IFieldDefinition ITypeDefinition.GetField(string fieldName, string typeFullName) => null;

        public TValue GetStaticValue<TValue>(string fieldName)
        {
            return default(TValue);
        }
    }
}