namespace HackF5.UnitySpy.Dynamic
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Dynamic;
    using System.Linq;
    using JetBrains.Annotations;

    public class DynamicManagedObjectInstance : DynamicObjectBase<IManagedObjectInstance>
    {
        public DynamicManagedObjectInstance([NotNull] IManagedObjectInstance underlying, DynamicObjectCache cache)
            : base(underlying, cache)
        {
        }

        [SuppressMessage("ReSharper", "NotResolvedInText")]
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if ((indexes.Length == 0) || (indexes.Length > 2))
            {
                throw new ArgumentException(
                    $"Expected either one or two index parameters, got {indexes.Length}.",
                    nameof(indexes));
            }

            var fieldNameObj = indexes[0];
            if (!(fieldNameObj is string fieldName) || (fieldName == default))
            {
                throw new ArgumentException(
                    $"Expected string parameter, got {fieldNameObj?.GetType() ?? (object)"null."}",
                    "fieldName");
            }

            var classNameObj = indexes.ElementAtOrDefault(1);
            if (!(classNameObj is string className))
            {
                if (classNameObj == default)
                {
                    className = default;
                }
                else
                {
                    throw new ArgumentException(
                        $"Expected string parameter, got {classNameObj.GetType()}.",
                        "className");
                }
            }

            result = this.Cache.GetResult(this.Underlying.GetValue<object>(fieldName, className));
            return true;
        }
    }
}