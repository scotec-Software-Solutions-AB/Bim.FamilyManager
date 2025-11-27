using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Bim.FamilyManager.Ui.Views.Converters;

/// <summary>
///     Provides a converter that creates a <see cref="Rect" /> object from width and height values.
///     Implements <see cref="IMultiValueConverter" /> for use in WPF multi-binding scenarios.
/// </summary>
public class RectConverter : IMultiValueConverter
{
    /// <summary>
    ///     Converts an array of values containing width and height into a <see cref="Rect" /> object.
    /// </summary>
    /// <param name="values">
    ///     An array where the first element is the width and the second is the height (both
    ///     <see cref="double" />).
    /// </param>
    /// <param name="targetType">The target type of the binding (not used).</param>
    /// <param name="parameter">An optional parameter for the conversion (not used).</param>
    /// <param name="culture">The culture to use in the converter (not used).</param>
    /// <returns>
    ///     A <see cref="Rect" /> with origin (0,0) and the specified width and height, or <see cref="Rect.Empty" /> if the
    ///     input is invalid.
    /// </returns>
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
    ///     Converts a <see cref="Rect" /> object back into an array containing its width and height.
    /// </summary>
    /// <param name="value">The <see cref="Rect" /> object to convert back.</param>
    /// <param name="targetTypes">The array of target types expected for the conversion (not used).</param>
    /// <param name="parameter">An optional parameter for the conversion (not used).</param>
    /// <param name="culture">The culture to use in the converter (not used).</param>
    /// <returns>
    ///     An array of objects representing the width and height of the <see cref="Rect" /> object.
    /// </returns>
    /// <exception cref="NotImplementedException">
    ///     Always thrown, as this method is not implemented.
    /// </exception>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
