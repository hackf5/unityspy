namespace HackF5.UnitySpy
{
    using System.Collections.Generic;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents an unmanaged _MonoClass instance in a Mono process. This object describes the type of a class or
    /// struct. The .NET equivalent is <see cref="System.Type"/>.
    /// See: _MonoImage in https://github.com/Unity-Technologies/mono/blob/unity-master/mono/metadata/class-internals.h.
    /// </summary>
    [PublicAPI]
    public interface ITypeDefinition : IMemoryObject
    {
        /// <summary>
        /// Gets the collection of the <see cref="IFieldDefinition"/> in the type and all of its base types.
        /// </summary>
        IReadOnlyList<IFieldDefinition> Fields { get; }

        /// <summary>
        /// Gets the full name of the type, for example 'System.Object'.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets a value indicating whether the type derives from <see cref="System.Enum"/>.
        /// </summary>
        bool IsEnum { get; }

        /// <summary>
        /// Gets a value indicating whether the type derives from <see cref="System.ValueType"/>.
        /// </summary>
        bool IsValueType { get; }

        /// <summary>
        /// Gets the name of the type. For example for 'System.Object' this value would be 'Object'.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the namespace of the type. For example for 'System.Object' this value would be 'System'.
        /// </summary>
        string NamespaceName { get; }

        ITypeInfo TypeInfo { get; }

        // THIS IS THE ISSUE?N???????
        dynamic this[string fieldName] { get; }

        IFieldDefinition GetField(string fieldName, string typeFullName = default);

        TValue GetStaticValue<TValue>(string fieldName);
    }
}