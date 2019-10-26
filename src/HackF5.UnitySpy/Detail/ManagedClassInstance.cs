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
#pragma warning disable CS0649 // Field 'ManagedClassInstance.definitionAddress' is never assigned to, and will always have its default value 0
#pragma warning disable CS0169 // The field 'ManagedClassInstance.definitionAddress' is never used
        private readonly uint definitionAddress;
#pragma warning restore CS0169 // The field 'ManagedClassInstance.definitionAddress' is never used
#pragma warning restore CS0649 // Field 'ManagedClassInstance.definitionAddress' is never assigned to, and will always have its default value 0

#pragma warning disable CS0169 // The field 'ManagedClassInstance.vtable' is never used
        private readonly uint vtable;
#pragma warning restore CS0169 // The field 'ManagedClassInstance.vtable' is never used

        public ManagedClassInstance([NotNull] IAssemblyImage image, uint address)
            : base(image, address)
        {
        }

        public override ITypeDefinition TypeDefinition => null;
    }
}