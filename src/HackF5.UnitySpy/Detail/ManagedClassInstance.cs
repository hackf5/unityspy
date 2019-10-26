namespace HackF5.UnitySpy.Detail
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents a class instance in managed memory.
    /// Mono and .NET don't necessarily use the same layout scheme, but assuming it is similar this article provides
    /// some useful information:
    /// https://web.archive.org/web/20080919091745/http://msdn.microsoft.com:80/en-us/magazine/cc163791.aspx.
    /// </summary>
    [PublicAPI]
    public class ManagedClassInstance : ManagedObjectInstance
    {
        public ManagedClassInstance([NotNull] AssemblyImage image, uint address)
            : base(image, address)
        {
        }
    }
}