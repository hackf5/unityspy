namespace HackF5.UnitySpy.Detail
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    /// This class represents a value type /struct instance in managed memory.
    /// Mono and .NET don't necessarily use the same layout scheme, but assuming it is similar this article provides
    /// some useful information:
    /// https://web.archive.org/web/20080919091745/http://msdn.microsoft.com:80/en-us/magazine/cc163791.aspx.
    /// </summary>
    [PublicAPI]
    public class ManagedStructInstance : ManagedObjectInstance
    {
        public ManagedStructInstance([NotNull] TypeDefinition typeDefinition, uint address)
#pragma warning disable CS1061 // 'TypeDefinition' does not contain a definition for 'Image' and no accessible extension method 'Image' accepting a first argument of type 'TypeDefinition' could be found (are you missing a using directive or an assembly reference?)
            : base(null, address)
#pragma warning restore CS1061 // 'TypeDefinition' does not contain a definition for 'Image' and no accessible extension method 'Image' accepting a first argument of type 'TypeDefinition' could be found (are you missing a using directive or an assembly reference?)
        {
        }
    }
}