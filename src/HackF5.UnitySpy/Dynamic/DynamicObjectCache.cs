namespace HackF5.UnitySpy.Dynamic
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Dynamic;
    using JetBrains.Annotations;

    public class DynamicObjectCache
    {
        private readonly ConcurrentDictionary<object, DynamicObject> cache =
            new ConcurrentDictionary<object, DynamicObject>();

        public object GetResult(dynamic value) => value == null ? null : this.GetAsDynamic(value);

        private object GetAsDynamic(IManagedObjectInstance value) => this.cache.GetOrAdd(
            value,
            o => new DynamicManagedObjectInstance((IManagedObjectInstance)o, this));

        private object GetAsDynamic(IAssemblyImage value) => this.cache.GetOrAdd(
            value,
            o => new DynamicAssemblyImage((IAssemblyImage)o, this));

        private object GetAsDynamic(ITypeDefinition value) => this.cache.GetOrAdd(
            value,
            o => new DynamicTypeDefinition((ITypeDefinition)o, this));

        private object GetAsDynamic(IList<object> value)
        {
            for (var index = 0; index < value.Count; index++)
            {
                value[index] = this.GetResult(value[index]);
            }

            return value;
        }

        [UsedImplicitly]
        private object GetAsDynamic(object value) => value;
    }
}