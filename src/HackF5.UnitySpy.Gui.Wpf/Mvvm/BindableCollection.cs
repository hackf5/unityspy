namespace HackF5.UnitySpy.Gui.Wpf.Mvvm
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using JetBrains.Annotations;

    [PublicAPI]
    public class BindableCollection<T> : ObservableCollection<T>
    {
        public BindableCollection()
        {
        }

        public BindableCollection([NotNull] IEnumerable<T> collection)
            : base(collection)
        {
        }

        public bool IsNotifying { get; set; } = true;

        public void Refresh()
        {
            void Action()
            {
                this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            MainThreadInvoker.Current.InvokeOnMainThread(Action);
        }

        public IReadOnlyList<T> ToThreadSafeList()
        {
            var result = new List<T>();
            MainThreadInvoker.Current.InvokeOnMainThread(() => result.AddRange(this.ToArray()));
            return result;
        }

        protected override void ClearItems() => MainThreadInvoker.Current.InvokeOnMainThread(base.ClearItems);

        protected override void InsertItem(int index, T item) =>
            MainThreadInvoker.Current.InvokeOnMainThread(() => base.InsertItem(index, item));

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!this.IsNotifying)
            {
                return;
            }

            MainThreadInvoker.Current.InvokeOnMainThread(() => base.OnCollectionChanged(e));
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (!this.IsNotifying)
            {
                return;
            }

            MainThreadInvoker.Current.InvokeOnMainThread(() => base.OnPropertyChanged(e));
        }

        protected override void RemoveItem(int index) =>
            MainThreadInvoker.Current.InvokeOnMainThread(() => base.RemoveItem(index));

        protected override void SetItem(int index, T item) =>
            MainThreadInvoker.Current.InvokeOnMainThread(() => base.SetItem(index, item));
    }
}