namespace HackF5.UnitySpy
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using HackF5.UnitySpy.Detail;
    using HackF5.UnitySpy.Util;
    using JetBrains.Annotations;

    /// <summary>
    /// A factory that creates <see cref="IAssemblyImage"/> instances that provides access into a Unity application's
    /// managed memory.
    /// SEE: https://github.com/Unity-Technologies/mono.
    /// </summary>
    [PublicAPI]
    public static class AssemblyImageFactory
    {
    }
}