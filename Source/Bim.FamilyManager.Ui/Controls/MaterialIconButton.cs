using System.Windows;
using System.Windows.Controls;
using Material.Icons;
using Material.Icons.WPF;

namespace Bim.FamilyManager.Ui.Controls;

/// <summary>
///     Represents a custom button control that integrates a Material Design icon.
/// </summary>
/// <remarks>
///     The <see cref="MaterialIconButton" /> class extends the functionality of the standard WPF <see cref="Button" />
///     control
///     by adding support for Material Design icons. The icon displayed by the button is determined by the
///     <see cref="Kind" /> property.
///     This control is styled using a custom control template defined in the application's resource dictionaries.
/// </remarks>
public class MaterialIconButton : Button
{
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(nameof(Kind), typeof(MaterialIconKind), typeof(MaterialIconButton),
        new PropertyMetadata(default(MaterialIconKind), OnKindChanged));

    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(nameof(IconSize), typeof(double), typeof(MaterialIconButton),
        new PropertyMetadata(double.NaN));

    private MaterialIcon? _iconPart;

    /// <summary>
    ///     Initializes the <see cref="MaterialIconButton" /> class by overriding the default style key property metadata.
    /// </summary>
    /// <remarks>
    ///     This static constructor ensures that the <see cref="MaterialIconButton" /> control uses its custom style defined
    ///     in the application's resource dictionaries. It is invoked automatically by the .NET runtime before any instances
    ///     of the <see cref="MaterialIconButton" /> class are created.
    /// </remarks>
    static MaterialIconButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MaterialIconButton), new FrameworkPropertyMetadata(typeof(MaterialIconButton)));
    }

    /// <summary>
    ///     Gets or sets the kind of Material Design icon displayed by the button.
    /// </summary>
    /// <value>
    ///     A <see cref="MaterialIconKind" /> value that specifies the icon to be displayed.
    ///     The default value is <c>default(MaterialIconKind)</c>.
    /// </value>
    /// <remarks>
    ///     This property is a dependency property and supports data binding.
    ///     Changing its value updates the icon displayed in the button.
    /// </remarks>
    public MaterialIconKind Kind
    {
        get => (MaterialIconKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>
    ///     Gets or sets the size (height and width) of the icon displayed by the button.
    /// </summary>
    /// <value>
    ///     A <see cref="double" /> value that specifies the icon size.
    ///     The default value is <see cref="double.NaN" />.
    /// </value>
    /// <remarks>
    ///     This property is a dependency property and supports data binding.
    /// </remarks>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>
    ///     Invoked whenever application code or internal processes call <see cref="FrameworkElement.ApplyTemplate" />.
    /// </summary>
    /// <remarks>
    ///     This method is overridden to retrieve and initialize the "PART_Icon" element from the control's template.
    ///     The retrieved element is expected to be of type <see cref="MaterialIcon" /> and is used to display the icon
    ///     specified by the <see cref="Kind" /> property.
    /// </remarks>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        // Retrieve the PART_Icon control from the template
        _iconPart = GetTemplateChild("PART_Icon") as MaterialIcon;
    }

    /// <summary>
    ///     Handles changes to the <see cref="KindProperty" /> dependency property.
    /// </summary>
    /// <param name="d">
    ///     The <see cref="DependencyObject" /> on which the property value has changed. Expected to be an instance
    ///     of <see cref="MaterialIconButton" />.
    /// </param>
    /// <param name="e">The event data containing information about the property change, including the old and new values.</param>
    /// <remarks>
    ///     This method updates the <see cref="MaterialIcon.Kind" /> property of the "PART_Icon" element in the control's
    ///     template
    ///     to reflect the new value of the <see cref="Kind" /> property. If the control's template has not been applied yet,
    ///     the update is deferred until the template is applied.
    ///     Note: This method ensures that the Material Design icon displayed by the button is updated whenever the
    ///     <see cref="Kind" /> property changes.
    /// </remarks>
    private static void OnKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var button = (MaterialIconButton)d;
        var newValue = (MaterialIconKind)e.NewValue;

        // This if condition is likely never true, as this method is almost always called before OnApplyTemplate().
        // However, WPF optimizes the template, and PART_Icon might be removed if it is not referenced elsewhere.
        // Without this code, the binding to MaterialIcon.Kind has no effect, causing the control to always display the default icon.
        // This might be an issue with the MaterialIcon itself.
        if (button._iconPart is not null)
        {
            button._iconPart.Kind = newValue;
        }
    }
}
