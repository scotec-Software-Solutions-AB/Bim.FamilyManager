using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Bim.FamilyManager.Ui.Views.Converters;

/// <summary>
///     Provides a converter that creates a <see cref="System.Windows.Rect" /> object based on width and height values.
///     This converter implements the <see cref="System.Windows.Data.IMultiValueConverter" /> interface.
/// </summary>
public class RectConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 &&
            values[0] is double width &&
            values[1] is double height)
        {
            return new Rect(0, 0, width, height);
        }

        return Rect.Empty;
    }

    /// <summary>
    ///     Converts a <see cref="System.Windows.Rect" /> object back into an array of values representing its width and
    ///     height.
    /// </summary>
    /// <param name="value">The <see cref="System.Windows.Rect" /> object to be converted back.</param>
    /// <param name="targetTypes">
    ///     The array of target types expected for the conversion. This parameter is not used in the
    ///     current implementation.
    /// </param>
    /// <param name="parameter">
    ///     An optional parameter for the conversion. This parameter is not used in the current
    ///     implementation.
    /// </param>
    /// <param name="culture">
    ///     The culture to be used during the conversion. This parameter is not used in the current
    ///     implementation.
    /// </param>
    /// <returns>
    ///     An array of objects representing the width and height of the <see cref="System.Windows.Rect" /> object.
    /// </returns>
    /// <exception cref="System.NotImplementedException">
    ///     Thrown to indicate that the method is not implemented.
    /// </exception>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
