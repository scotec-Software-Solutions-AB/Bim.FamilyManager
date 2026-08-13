#region

using System.Globalization;
using System.Windows;
using System.Windows.Data;

#endregion

namespace Bim.FamilyManager.Ui.Converter;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BooleanToVisibilityConverter : IValueConverter
{
    public BooleanToVisibilityConverter()
    {
        TrueVisibility = Visibility.Visible;
        FalseVisibility = Visibility.Collapsed;
    }

    public Visibility TrueVisibility { get; set; }
    public Visibility FalseVisibility { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!(value is bool boolValue))
        {
            return FalseVisibility;
        }

        if (parameter != null && parameter.ToString() == "Invert")
        {
            return boolValue ? FalseVisibility : TrueVisibility;
        }

        return boolValue ? TrueVisibility : FalseVisibility;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
