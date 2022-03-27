namespace HackF5.UnitySpy.Gui.Avalonia.View.Helpers
{
    using System;
    using System.Globalization;
    using global::Avalonia.Media;
    using global::Avalonia.Data.Converters;

    /// <summary>
    /// Converts a null value to parameter
    /// </summary>
    public class BoolToFontWeightConverter : IValueConverter
    {        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            return (bool) value ? (FontWeight)Enum.Parse(typeof(FontWeight), parameter.ToString()) : FontWeight.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
