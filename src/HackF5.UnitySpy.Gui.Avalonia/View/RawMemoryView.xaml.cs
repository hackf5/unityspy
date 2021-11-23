namespace HackF5.UnitySpy.Gui.Avalonia.View
{
    using global::Avalonia.Controls;
    using global::Avalonia.Markup.Xaml;
    using HackF5.UnitySpy.Gui.Avalonia.ViewModel;

    public class RawMemoryView : Window
    {   
        public RawMemoryView() 
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