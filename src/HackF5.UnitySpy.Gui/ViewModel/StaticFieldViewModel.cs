namespace HackF5.UnitySpy.Gui.ViewModel
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using JetBrains.Annotations;
    using TypeCode = HackF5.UnitySpy.Detail.TypeCode;

    public class StaticFieldViewModel
    {
        private readonly IFieldDefinition field;

        public StaticFieldViewModel([NotNull] IFieldDefinition field)
        {
            this.field = field ?? throw new ArgumentNullException(nameof(field));
        }

        public delegate StaticFieldViewModel Factory(IFieldDefinition field);

        public string Name => this.field.Name;

        public string TypeName
        {
            get
            {
                if (this.field.TypeInfo.TryGetTypeDefinition(out var typeDefinition))
                {
                    return typeDefinition.FullName;
                }

                var typeName = this.field.TypeInfo.TypeCode.ToString();
                var member = typeof(TypeCode).GetMember(typeName).FirstOrDefault();
                return member?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? typeName;
            }
        }

        public object Value => this.field.DeclaringType.GetStaticValue<object>(this.Name);
    }
}