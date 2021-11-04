namespace HackF5.UnitySpy.Gui.Wpf.Mvvm
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;

    [PublicAPI]
    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool SetProperty<T>(ref T propertyField, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(propertyField, value))
            {
                return false;
            }

            propertyField = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler == null)
            {
                return;
            }

            MainThreadInvoker.Current.InvokeOnMainThread(
                () => handler(this, new PropertyChangedEventArgs(propertyName)));
        }
    }
}