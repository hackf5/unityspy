namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using HackF5.UnitySpy.Util;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents an unmanaged _MonoClass instance in a Mono process. This object describes the type of a class or
    /// struct. The .NET equivalent is <see cref="System.Type"/>.
    /// See: _MonoClass in https://github.com/Unity-Technologies/mono/blob/unity-master/mono/metadata/class-internals.h.
    /// </summary>
    [PublicAPI]
    public class TypeDefinition : MemoryObject, ITypeDefinition
    {
    }
}