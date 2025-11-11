using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Material.Icons.WPF;

namespace Bim.FamilyManager.Ui.Controls;

/// <summary>
///     Represents a custom button control that displays an image alongside its content.
/// </summary>
/// <remarks>
///     The <see cref="ImageButton" /> class extends the functionality of the standard WPF <see cref="Button" /> control
///     by adding support for displaying an image. The image source can be specified using the <see cref="ImageSource" />
///     dependency property.
/// </remarks>
public class ImageButton : Button
{
    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(ImageButton), new PropertyMetadata());

    /// <summary>
    ///     Gets or sets the source of the image displayed in the <see cref="ImageButton" /> control.
    /// </summary>
    /// <value>
    ///     An <see cref="ImageSource" /> object representing the image to be displayed.
    /// </value>
    /// <remarks>
    ///     The <see cref="ImageSource" /> property is a dependency property that allows binding and styling.
    ///     It is used to specify the image displayed alongside the button's content.
    /// </remarks>
    public ImageSource ImageSource
    {
        get => (ImageSource)GetValue(MaterialIcon.KindProperty);
        set => SetValue(ImageSourceProperty, value);
    }
}
