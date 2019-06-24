namespace HackF5.UnitySpy.Gui.Mvvm
{
    using System.Windows;
    using System.Windows.Controls;

    public class ExtendedContentControl : ContentControl
    {
        public static readonly DependencyProperty ExtendedContentProperty = DependencyProperty.Register(
            "ExtendedContent",
            typeof(object),
            typeof(ExtendedContentControl),
            new PropertyMetadata(default, ExtendedContentControl.OnExtendedContentChanged));

        public object ExtendedContent
        {
            get => (object)this.GetValue(ExtendedContentControl.ExtendedContentProperty);
            set => this.SetValue(ExtendedContentControl.ExtendedContentProperty, value);
        }

        private static void OnExtendedContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ExtendedContentControl)d;
            control.Content = ViewLocator.GetViewFor(e.NewValue);
        }
    }
}