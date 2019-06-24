namespace HackF5.UnitySpy.Gui.View
{
    using System.Windows.Input;
    using HackF5.UnitySpy.Gui.ViewModel;

    public partial class TypeDefinitionContentView
    {
        public TypeDefinitionContentView()
        {
            this.InitializeComponent();
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(this.ItemsList.SelectedItem is StaticFieldViewModel item))
            {
                return;
            }

            if (!(this.DataContext is TypeDefinitionContentViewModel model))
            {
                return;
            }

            model.OnAppendToTrail(item.Name);
        }
    }
}