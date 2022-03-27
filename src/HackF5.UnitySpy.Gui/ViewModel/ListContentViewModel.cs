namespace HackF5.UnitySpy.Gui.ViewModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.Gui.Mvvm;

    public class ListContentViewModel : PropertyChangedBase
    {
        private readonly IList list;

        public ListContentViewModel(IList list, ListItemViewModel.Factory itemFactory)
        {
            this.list = list;
            this.Items = this.list.Cast<object>().Select((o, i) => itemFactory(o, i)).ToArray();
        }

        public delegate ListContentViewModel Factory(IList list);

        public event EventHandler<AppendToTrailEventArgs> AppendToTrail;

        public IEnumerable<ListItemViewModel> Items { get; }

        public virtual void OnAppendToTrail(string value)
        {
            this.AppendToTrail?.Invoke(this, new AppendToTrailEventArgs(value));
        }
    }
}