namespace HackF5.UnitySpy.Gui.Wpf.View
{
    using System.Windows.Input;
    using HackF5.UnitySpy.Gui.Wpf.ViewModel;

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