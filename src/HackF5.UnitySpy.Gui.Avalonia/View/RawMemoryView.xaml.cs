namespace HackF5.UnitySpy.Gui.Avalonia.View
{
    using System;
    using System.Threading.Tasks;
    using global::Avalonia.Controls;
    using global::Avalonia.Markup.Xaml;
    using HackF5.UnitySpy.Gui.Avalonia.ViewModel;

    public class RawMemoryView : Window
    {   
        private readonly RawMemoryViewModel viewModel;

        public RawMemoryView()
        {
            viewModel = new RawMemoryViewModel();
            DataContext = viewModel;

            this.InitializeComponent();

            // Hide the window instead of closing it (cannot re-show a closed window)
            Closing += (s, e) =>
            {
                ((Window)s).Hide();
                e.Cancel = true;
            };
        }       

        protected void InitializeComponent() 
        {
            AvaloniaXamlLoader.Load(this); 
            //this.AttachDevTools();
        }


        public Task ShowDialog(Window owner, IntPtr address)
        {
            this.viewModel.StartAddress = address.ToString("X");
            return ShowDialog(owner);
        }
    }

}