namespace HackF5.UnitySpy.Dynamic
{
    using System;
    using System.Dynamic;
    using JetBrains.Annotations;

    public class DynamicTypeDefinition : DynamicObjectBase<ITypeDefinition>
    {
        public DynamicTypeDefinition([NotNull] ITypeDefinition underlying, DynamicObjectCache cache)
        : base(underlying, cache)
        {
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            // ReSharper disable NotResolvedInText
            if (indexes.Length != 1)
            {
                throw new ArgumentException(
                    $"Expected exactly one index parameter, got {indexes.Length}.",
                    "fullClassName");
            }

            if (!(indexes[0] is string fullClassName) || (indexes[0] == default))
            {
                throw new ArgumentException(
                    $"Expected string parameter, got {indexes[0]?.GetType() ?? (object)"null."}",
                    "fullClassName");
            }
            //// ReSharper restore NotResolvedInText

            result = this.Cache.GetResult(this.Underlying.GetStaticValue<object>(fullClassName));
            return true;
        }
    }
}