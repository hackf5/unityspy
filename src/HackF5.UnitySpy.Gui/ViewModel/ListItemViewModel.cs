namespace HackF5.UnitySpy.Gui.ViewModel
{
    using HackF5.UnitySpy.Gui.Mvvm;

    public class ListItemViewModel : PropertyChangedBase
    {
        public ListItemViewModel(object item, int index)
        {
            this.Item = item;
            this.Index = index;
        }

        public delegate ListItemViewModel Factory(object item, int index);

        public object Item { get; }

        public int Index { get; }
    }
}