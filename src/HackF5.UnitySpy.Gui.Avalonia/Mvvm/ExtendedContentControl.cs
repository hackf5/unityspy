namespace HackF5.UnitySpy.Gui.Avalonia.Mvvm
{
    using System;
    using global::Avalonia;
    using global::Avalonia.Controls;
    using global::Avalonia.Styling;

    public class ExtendedContentControl : ContentControl, IStyleable
    {
        Type IStyleable.StyleKey => typeof(ContentControl);

        public static readonly DirectProperty<ExtendedContentControl, object> ExtendedContentProperty = 
            AvaloniaProperty.RegisterDirect<ExtendedContentControl, object>(
                nameof(ExtendedContent),
                o => o.ExtendedContent,
                OnExtendedContentChanged);

        public object ExtendedContent
        {
            get => (object)this.GetValue(ExtendedContentControl.ExtendedContentProperty);
            set => this.SetValue(ExtendedContentControl.ExtendedContentProperty, value);
        }

        private static void OnExtendedContentChanged(ExtendedContentControl control, object value)
        {
            control.Content = ViewLocator.GetViewFor(value);           
        }
    }
}