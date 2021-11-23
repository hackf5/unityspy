namespace HackF5.UnitySpy.Gui.Avalonia.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using global::Avalonia.Threading;
    using JetBrains.Annotations;
    using ReactiveUI;

    public class AssemblyImageViewModel : ReactiveObject
    {
        private readonly IAssemblyImage image;

        private readonly List<TypeDefinitionViewModel> types;

        private readonly TypeDefinitionViewModel.Factory typeDefinitionFactory;

        private TypeDefinitionViewModel selectedType;

        private bool hasStaticOnly = true;

        private string listViewFilter = "";

        public AssemblyImageViewModel(
            [NotNull] IAssemblyImage image,
            [NotNull] TypeDefinitionViewModel.Factory typeDefinitionFactory)
        {
            this.image = image ?? throw new ArgumentNullException(nameof(image));

            this.typeDefinitionFactory =
                typeDefinitionFactory ?? throw new ArgumentNullException(nameof(typeDefinitionFactory));

            this.types = new List<TypeDefinitionViewModel>();
            Dispatcher.UIThread.InvokeAsync(this.Init);
        }

        public delegate AssemblyImageViewModel Factory(IAssemblyImage image);

        public TypeDefinitionViewModel SelectedType
        {
            get => this.selectedType;
            set => this.RaiseAndSetIfChanged(ref this.selectedType, value);
        }

        public bool HasStaticOnly
        {
            get => this.hasStaticOnly;
            set 
            {
                if(this.hasStaticOnly != value)
                {
                    this.RaiseAndSetIfChanged(ref this.hasStaticOnly, value);
                    this.Refresh();
                }
            } 
        }

        public string ListViewFilter
        {
            get => this.listViewFilter;
            set 
            {
                bool refresh = value != this.listViewFilter;
                this.RaiseAndSetIfChanged(ref this.listViewFilter, value);
                if(refresh)
                {
                    this.Refresh();
                }
            } 
        }

        public ObservableCollection<TypeDefinitionViewModel> FilteredTypes { get; } =
            new ObservableCollection<TypeDefinitionViewModel>();

        private void Refresh()
        {
            this.FilteredTypes.Clear();

            types.ForEach(definition =>
            {
                if(Filter(definition)) 
                {
                    this.FilteredTypes.Add(definition);
                }
            });
        }

        private bool Filter(TypeDefinitionViewModel definition) 
        {
            if (this.hasStaticOnly && !definition.HasStaticFields)
            {
                return false;
            }

            var text = this.listViewFilter.Trim();
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            if (!(definition.FullName.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return false;
            }

            return true;
        }

        private void Init()
        {
            var typeDefinitionsOrdered = this.image.TypeDefinitions.OrderBy(td => td.FullName);

            this.types.AddRange(new List<TypeDefinitionViewModel>(typeDefinitionsOrdered.Select(td => this.typeDefinitionFactory(td))));
            this.Refresh();

            this.SelectedType = this.FilteredTypes.FirstOrDefault(t => t.FullName == "CollectionManager");
        }
    }
}