namespace HackF5.UnitySpy.Gui.Avalonia.View
{
    using global::Avalonia.Controls;
    using global::Avalonia.Markup.Xaml;

    public class MainWindow : Window
    {
        public MainWindow()
        {   
            this.InitializeComponent();
        }       

        protected void InitializeComponent() 
        {
            AvaloniaXamlLoader.Load(this); 
            //this.AttachDevTools();
        } 
    }
}
