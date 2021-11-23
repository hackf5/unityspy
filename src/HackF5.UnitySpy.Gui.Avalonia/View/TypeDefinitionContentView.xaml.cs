namespace HackF5.UnitySpy.Gui.Avalonia.View
{
    using global::Avalonia.Controls;
    using global::Avalonia.Input;
    using global::Avalonia.Interactivity;
    using global::Avalonia.Markup.Xaml;
    using HackF5.UnitySpy.Gui.ViewModel;
    using HackF5.UnitySpy.Gui.Avalonia.ViewModel;

    public class TypeDefinitionContentView : UserControl
    {
        private readonly ListBox itemsList;

        public TypeDefinitionContentView()
        {
            this.InitializeComponent();
            this.itemsList = this.FindControl<ListBox>("ItemsList");
        }

        protected void InitializeComponent() 
        {
            AvaloniaXamlLoader.Load(this); 
            //this.AttachDevTools();
        } 

        public void Control_OnMouseDoubleClick(object sender, RoutedEventArgs e)
        {            
            if (!(this.itemsList.SelectedItem is StaticFieldViewModel item))
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