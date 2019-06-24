namespace HackF5.UnitySpy.Gui.ViewModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.Gui.Mvvm;
    using JetBrains.Annotations;

    public class TypeDefinitionViewModel : PropertyChangedBase
    {
        private readonly ITypeDefinition definition;

        private readonly ListContentViewModel.Factory listContentFactory;

        private readonly ManagedObjectInstanceContentViewModel.Factory managedObjectContentFactory;

        private readonly TypeDefinitionContentViewModel.Factory typeDefinitionContentFactory;

        private object content;

        private string path;

        public TypeDefinitionViewModel(
            [NotNull] ITypeDefinition definition,
            [NotNull] TypeDefinitionContentViewModel.Factory typeDefinitionContentFactory,
            [NotNull] ManagedObjectInstanceContentViewModel.Factory managedObjectContentFactory,
            [NotNull] ListContentViewModel.Factory listContentFactory)
        {
            this.definition = definition ?? throw new ArgumentNullException(nameof(definition));
            this.typeDefinitionContentFactory = typeDefinitionContentFactory
                ?? throw new ArgumentNullException(nameof(typeDefinitionContentFactory));

            this.managedObjectContentFactory = managedObjectContentFactory
                ?? throw new ArgumentNullException(nameof(managedObjectContentFactory));

            this.listContentFactory = listContentFactory ?? throw new ArgumentNullException(nameof(listContentFactory));

            this.content = this.typeDefinitionContentFactory(this.definition);
        }

        public delegate TypeDefinitionViewModel Factory(ITypeDefinition definition);

        public object Content
        {
            get => this.content;
            set => this.SetProperty(ref this.content, value);
        }

        public string FullName => this.definition.FullName;

        public bool HasStaticFields => this.definition.Fields.Any(f => f.TypeInfo.IsStatic && !f.TypeInfo.IsConstant);

        public string Path
        {
            get => this.path;
            set
            {
                if (!this.SetProperty(ref this.path, value))
                {
                    return;
                }

                this.ParsePath(this.Path ?? string.Empty);
            }
        }

        public BindableCollection<string> Trail { get; } = new BindableCollection<string>();

        private void ParsePath(string value)
        {
            var parts = value.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();

            dynamic current = this.definition;
            var trail = new List<(string part, object item)>();
            foreach (var part in parts)
            {
                try
                {
                    var x = int.TryParse(part, out var index) ? (dynamic)index : part;
                    var next = current[x];
                    if (next is IMemoryObject || next is IList)
                    {
                        current = next;
                        trail.Add((part, current));
                        continue;
                    }

                    break;
                }
                catch
                {
                    break;
                }
            }

            this.Trail.Clear();
            if (!trail.Any())
            {
                this.Content = this.typeDefinitionContentFactory(this.definition);
                return;
            }

            foreach (var item in trail)
            {
                this.Trail.Add(item.part);
            }

            var content = trail.Last().item;
            if (content is ITypeDefinition type)
            {
                this.Content = this.typeDefinitionContentFactory(type);
                return;
            }

            if (content is IManagedObjectInstance instance)
            {
                this.Content = this.managedObjectContentFactory(instance);
                return;
            }

            if (content is IList list)
            {
                this.Content = this.listContentFactory(list);
                return;
            }

            this.Content = default;
        }
    }
}