namespace HackF5.UnitySpy.Detail
{
    using JetBrains.Annotations;

    /// <summary>
    /// This class represents a value type /struct instance in managed memory.
    /// Mono and .NET don't necessarily use the same layout scheme, but assuming it is similar this article provides
    /// some useful information:
    /// https://web.archive.org/web/20080919091745/http://msdn.microsoft.com:80/en-us/magazine/cc163791.aspx.
    /// </summary>
    [PublicAPI]
    public class ManagedStructInstance : MemoryObject, IManagedObjectInstance
    {
        public ManagedStructInstance(TypeDefinition typeDefinition, uint address)
            : base(typeDefinition.Image, address)
        {
            // value type pointers contain no type information as a significant performance optimization. in memory
            // a value type is simply a contiguous sequence of bytes and it is up to the runtime to know how to
            // interpret those bytes. if you take the example of a integer, then it makes sense why this is
            // as if an integer needed an extra pointer that pointed back to the System.Int32 class then the size
            // of each one would be doubled. presumably interoperability with native assemblies would also be
            // completely scuppered.
            this.TypeDefinition = typeDefinition;
        }

        ITypeDefinition IManagedObjectInstance.TypeDefinition => this.TypeDefinition;

        public TypeDefinition TypeDefinition { get; }

        public TValue GetValue<TValue>(string fieldName) =>
            this.TypeDefinition.GetField(fieldName).GetValue<TValue>(this.Address);

        public TValue GetValue<TValue>(string fieldName, string typeFullName) => this.GetValue<TValue>(fieldName);
    }
}