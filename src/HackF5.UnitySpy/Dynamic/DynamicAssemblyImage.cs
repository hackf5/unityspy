namespace HackF5.UnitySpy.Dynamic
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Dynamic;
    using JetBrains.Annotations;

    public class DynamicAssemblyImage : DynamicObjectBase<IAssemblyImage>
    {
        public DynamicAssemblyImage([NotNull] IAssemblyImage underlying, [NotNull] DynamicObjectCache cache)
            : base(underlying, cache)
        {
        }

        [SuppressMessage("ReSharper", "NotResolvedInText")]
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException(
                    $"Expected exactly one index parameter, got {indexes.Length}.",
                    "fullClassName");
            }

            if (!(indexes[0] is string fullClassName) || (indexes[0] == default))
            {
                throw new ArgumentException(
                    $"Expected string parameter, got {indexes[0]?.GetType() ?? (object)"null"}.",
                    "fullClassName");
            }

            var definition = this.Underlying.GetTypeDefinition(fullClassName);
            if (definition == default)
            {
                throw new ArgumentException(
                    $"Expected string parameter, got {indexes[0]?.GetType()}.",
                    "fullClassName");
            }

            result = this.Cache.GetResult(definition);
            return true;
        }
    }
}