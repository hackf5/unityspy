namespace HackF5.UnitySpy.Dynamic
{
    using System;
    using System.Dynamic;
    using JetBrains.Annotations;

    public abstract class DynamicObjectBase<TUnderlying> : DynamicObject
        where TUnderlying : class, IMemoryObject
    {
        protected DynamicObjectBase([NotNull] TUnderlying underlying, [NotNull] DynamicObjectCache cache)
        {
            this.Underlying = underlying ?? throw new ArgumentNullException(nameof(underlying));
            this.Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        protected DynamicObjectCache Cache { get; }

        protected TUnderlying Underlying { get; }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var property = typeof(TUnderlying).GetProperty(binder.Name);
            if (property == default)
            {
                result = null;
                return false;
            }

            result = this.Cache.GetResult(property.GetValue(this.Underlying));
            return true;
        }
    }
}