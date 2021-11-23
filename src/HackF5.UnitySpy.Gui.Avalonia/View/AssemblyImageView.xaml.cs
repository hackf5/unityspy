namespace HackF5.UnitySpy.Gui.Avalonia.View
{
    using global::Avalonia.Controls;
    using global::Avalonia.Markup.Xaml;

    public class AssemblyImageView : UserControl
    {
        private readonly ListBox definitionsList;

        public AssemblyImageView()
        {
            this.InitializeComponent();
            this.definitionsList = this.FindControl<ListBox>("DefinitionsList");
        } 

        protected void InitializeComponent() 
        {
            AvaloniaXamlLoader.Load(this); 
            //this.AttachDevTools();
        } 

        private void DefinitionsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.definitionsList.ScrollIntoView(this.definitionsList.SelectedItem);
        }
    }
}