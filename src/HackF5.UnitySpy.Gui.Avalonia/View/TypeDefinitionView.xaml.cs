namespace HackF5.UnitySpy.Gui.Avalonia.View
{
    using global::Avalonia.Controls;
    using global::Avalonia.Markup.Xaml;

    public class TypeDefinitionView : UserControl
    {
        public TypeDefinitionView()
        {
            this.InitializeComponent();
            this.Initialized += (sender, args) => this.FindControl<TextBox>("PathTextBox").Focus();
        }

        protected void InitializeComponent() 
        {
            AvaloniaXamlLoader.Load(this); 
            //this.AttachDevTools();
        } 
    }
}