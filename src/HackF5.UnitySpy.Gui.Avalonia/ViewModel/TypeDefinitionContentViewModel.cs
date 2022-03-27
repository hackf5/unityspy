namespace HackF5.UnitySpy.Gui.Avalonia.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using HackF5.UnitySpy.Gui.ViewModel;
    using ReactiveUI;

    public class TypeDefinitionContentViewModel : ReactiveObject
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