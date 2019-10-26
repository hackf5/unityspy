namespace HackF5.UnitySpy
{
    using JetBrains.Annotations;

    /// <summary>
    /// Represents an unmanaged _MonoClassField instance in a Mono process. This object describes a field in a
    /// managed class or struct. The .NET equivalent is <see cref="System.Reflection.FieldInfo"/>.
    /// See: _MonoImage in https://github.com/Unity-Technologies/mono/blob/unity-master/mono/metadata/class-internals.h.
    /// </summary>
    [PublicAPI]
    public interface IFieldDefinition : IMemoryObject
    {
    }
}