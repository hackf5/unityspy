namespace HackF5.UnitySpy.Gui.Avalonia.View
{
    using global::Avalonia.Controls;
    using global::Avalonia.Interactivity;
    using global::Avalonia.Markup.Xaml;
    using HackF5.UnitySpy.Gui.ViewModel;

    public class ListContentView : UserControl
    {
        private readonly ListBox itemsList;

        public ListContentView()
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
            if (!(this.itemsList.SelectedItem is ListItemViewModel item))
            {
                return;
            }

            if (!(this.DataContext is ListContentViewModel model))
            {
                return;
            }

            model.OnAppendToTrail(item.Index.ToString());
        }
    }
}
