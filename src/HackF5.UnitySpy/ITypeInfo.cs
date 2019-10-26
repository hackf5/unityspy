namespace HackF5.UnitySpy
{
    using HackF5.UnitySpy.Detail;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents an unmanaged _MonoType instance in a Mono process. This object describes type information.
    /// There is no direct .NET equivalent, but probably comes under the umbrella of <see cref="System.Type"/>.
    /// See: _MonoType in https://github.com/Unity-Technologies/mono/blob/unity-master/mono/metadata/metadata-internals.h.
    /// </summary>
    [PublicAPI]
    public interface ITypeInfo
    {
    }
}