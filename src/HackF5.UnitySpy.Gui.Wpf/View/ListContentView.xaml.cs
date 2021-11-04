namespace HackF5.UnitySpy.Gui.Wpf.View
{
    using System.Windows.Input;
    using HackF5.UnitySpy.Gui.Wpf.ViewModel;

    public partial class ListContentView
    {
        public ListContentView()
        {
            this.InitializeComponent();
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(this.ItemsList.SelectedItem is ListItemViewModel item))
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
