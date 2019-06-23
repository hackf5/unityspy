namespace HackF5.UnitySpy
{
    using System.Collections.Generic;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents an unmanaged _MonoImage instance in a Mono process. This object describes a managed assembly.
    /// The .NET equivalent is <see cref="System.Reflection.Assembly"/>.
    /// See: _MonoImage in https://github.com/Unity-Technologies/mono/blob/unity-master/mono/metadata/metadata-internals.h.
    /// </summary>
    [PublicAPI]
    public interface IAssemblyImage : IMemoryObject
    {
        /// <summary>
        /// Gets the type definitions that are referenced by the assembly. So for example, although
        /// <see cref="object"/> is not declared in the assembly, since all types inherit from this type, it is
        /// available in this collection.
        /// </summary>
        IEnumerable<ITypeDefinition> TypeDefinitions { get; }

        /// <summary>
        /// Gets the <see cref="ITypeDefinition"/> with given <paramref name="fullName"/> from the assembly image.
        /// </summary>
        /// <param name="fullName">The full name of the definition including namespace. For example 'System.Object'
        /// not 'object'.</param>
        /// <returns>
        /// The <see cref="ITypeDefinition"/> with given <paramref name="fullName"/> from the assembly image, or
        /// <c>null</c> if no such definition exists.
        /// </returns>
        ITypeDefinition GetTypeDefinition(string fullName);

        /// <summary>
        /// Gets a dynamic view of the <see cref="IAssemblyImage"/> that is less verbose to work with.
        /// </summary>
        /// <returns>
        /// A dynamic view of the <see cref="IAssemblyImage"/>.
        /// </returns>
        dynamic ToDynamic();
    }
}