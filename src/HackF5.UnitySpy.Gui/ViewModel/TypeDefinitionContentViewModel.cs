namespace HackF5.UnitySpy.Gui.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.Gui.Mvvm;
    using JetBrains.Annotations;

    public class TypeDefinitionContentViewModel : PropertyChangedBase
    {
        private readonly ITypeDefinition definition;

        public TypeDefinitionContentViewModel(
            [NotNull] ITypeDefinition definition,
            StaticFieldViewModel.Factory fieldFactory)
        {
            this.definition = definition ?? throw new ArgumentNullException(nameof(definition));
            this.StaticFields = this.definition.Fields.Where(f => f.TypeInfo.IsStatic && !f.TypeInfo.IsConstant)
                .Select(f => fieldFactory(f))
                .ToArray();
        }

        public delegate TypeDefinitionContentViewModel Factory(ITypeDefinition definition);

        public event EventHandler<AppendToTrailEventArgs> AppendToTrail;

        public IEnumerable<StaticFieldViewModel> StaticFields { get; }

        public virtual void OnAppendToTrail(string value)
        {
            this.AppendToTrail?.Invoke(this, new AppendToTrailEventArgs(value));
        }
    }
}