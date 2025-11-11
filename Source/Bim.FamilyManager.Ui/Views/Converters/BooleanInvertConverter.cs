using System.Globalization;
using System.Windows.Data;

namespace Bim.FamilyManager.Ui.Views.Converters;

/// <summary>
///     Provides a value converter that inverts a boolean value.
/// </summary>
/// <remarks>
///     This converter is used to invert a boolean value in data binding scenarios.
///     It implements the <see cref="System.Windows.Data.IValueConverter" /> interface.
/// </remarks>
[ValueConversion(typeof(bool), typeof(bool))]
public class BooleanInvertConverter : IValueConverter
{
    /// <summary>
    ///     Converts a boolean value to its inverted equivalent.
    /// </summary>
    /// <param name="value">The value produced by the binding source. Expected to be of type <see cref="bool" />.</param>
    /// <param name="targetType">The type of the binding target property. This parameter is not used in this implementation.</param>
    /// <param name="parameter">
    ///     An optional parameter to be used in the converter logic. This parameter is not used in this
    ///     implementation.
    /// </param>
    /// <param name="culture">The culture to use in the converter. This parameter is not used in this implementation.</param>
    /// <returns>
    ///     Returns the inverted boolean value if <paramref name="value" /> is of type <see cref="bool" />.
    ///     Otherwise, returns <see langword="false" />.
    /// </returns>
    /// <exception cref="InvalidCastException">
    ///     Thrown if the <paramref name="value" /> is not of type <see cref="bool" />.
    /// </exception>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool booleanValue)
        {
            return !booleanValue;
        }

        return false;
    }

    /// <summary>
    ///     Converts a value back by inverting a boolean value.
    /// </summary>
    /// <param name="value">The value that is passed to the target. Expected to be of type <see cref="bool" />.</param>
    /// <param name="targetType">The type of the binding target property. This parameter is not used in this implementation.</param>
    /// <param name="parameter">
    ///     An optional parameter to be used in the converter logic. This parameter is not used in this
    ///     implementation.
    /// </param>
    /// <param name="culture">The culture to use in the converter. This parameter is not used in this implementation.</param>
    /// <returns>
    ///     Returns the inverted boolean value if <paramref name="value" /> is of type <see cref="bool" />.
    ///     Otherwise, returns <see langword="false" />.
    /// </returns>
    /// <exception cref="InvalidCastException">
    ///     Thrown if the <paramref name="value" /> is not of type <see cref="bool" />.
    /// </exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool booleanValue)
        {
            return !booleanValue;
        }

        return false;
    }
}
