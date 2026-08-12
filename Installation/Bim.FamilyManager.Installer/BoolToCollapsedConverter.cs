using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Bim.FamilyManager.Installer
{
    /// <summary>
    /// Converts a boolean to Visibility: true → Collapsed, false → Visible.
    /// Used to hide the "Not installed" hint for versions that are detected.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b) ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
