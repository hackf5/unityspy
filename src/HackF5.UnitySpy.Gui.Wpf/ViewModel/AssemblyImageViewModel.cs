namespace HackF5.UnitySpy.Gui.Wpf.ViewModel
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using HackF5.UnitySpy.Gui.Wpf.Mvvm;
    using JetBrains.Annotations;

    public class AssemblyImageViewModel : PropertyChangedBase
    {
        private readonly IAssemblyImage image;

        private readonly TypeDefinitionViewModel.Factory typeDefinitionFactory;

        private TypeDefinitionViewModel selectedType;

        public AssemblyImageViewModel(
            [NotNull] IAssemblyImage image,
            [NotNull] TypeDefinitionViewModel.Factory typeDefinitionFactory)
        {
            this.image = image ?? throw new ArgumentNullException(nameof(image));

            this.typeDefinitionFactory =
                typeDefinitionFactory ?? throw new ArgumentNullException(nameof(typeDefinitionFactory));

            Task.Run(this.Init);
        }

        public delegate AssemblyImageViewModel Factory(IAssemblyImage image);

        public TypeDefinitionViewModel SelectedType
        {
            get => this.selectedType;
            set => this.SetProperty(ref this.selectedType, value);
        }

        public BindableCollection<TypeDefinitionViewModel> Types { get; } =
            new BindableCollection<TypeDefinitionViewModel>();

        private void Init()
        {
            this.Types.IsNotifying = false;
            this.Types.Clear();

            foreach (var type in this.image.TypeDefinitions.OrderBy(td => td.FullName)
                .Select(td => this.typeDefinitionFactory(td)))
            {
                this.Types.Add(type);
            }

            this.Types.IsNotifying = true;
            this.Types.Refresh();

            this.SelectedType = this.Types.FirstOrDefault(t => t.FullName == "CollectionManager");
        }
    }
}