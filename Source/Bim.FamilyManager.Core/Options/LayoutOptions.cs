using Bim.FamilyManager.Core.Abstractions.Options;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Bim.FamilyManager.Core.Options;

/// <summary>
///     Represents the base class for layout options used in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This abstract class provides common properties and functionality for configuring layout options,
///     such as the key identifier, text block height, and visibility settings for preview images and item names.
///     Derived classes must be annotated with <see cref="LayoutOptionsAttribute" /> to declare their
///     <see cref="Key" /> identifier. This ensures the key is explicit and rename-safe.
/// </remarks>
public abstract class LayoutOptions : ILayoutOptions
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LayoutOptions" /> class.
    /// </summary>
    /// <remarks>
    ///     Reads the <see cref="Key" /> identifier from the <see cref="LayoutOptionsAttribute" /> applied to the
    ///     concrete derived class. Throws <see cref="InvalidOperationException" /> if the attribute is missing,
    ///     ensuring that every layout options class is explicitly registered with a stable key.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the concrete class is not annotated with <see cref="LayoutOptionsAttribute" />.
    /// </exception>
    protected LayoutOptions()
    {
        var attribute = GetType().GetCustomAttributes(typeof(LayoutOptionsAttribute), inherit: false)
                                 .OfType<LayoutOptionsAttribute>()
                                 .FirstOrDefault()
                        ?? throw new InvalidOperationException(
                            $"'{GetType().Name}' must be annotated with [{nameof(LayoutOptionsAttribute)}] to declare its layout key.");

        Key = attribute.OptionsName;
    }

    /// <summary>
    ///     Gets or sets the height of the content used in the layout.
    /// </summary>
    /// <value>
    ///     The height of the content area, measured in pixels. The default value is 64.
    /// </value>
    public int ContentHeight { get; set; } = 64;

    /// <summary>
    ///     Gets or sets a value indicating whether the preview image should be displayed in the layout.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the preview image is displayed; otherwise, <c>false</c>.
    /// </value>
    public bool ShowPreviewImage { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether the item name should be displayed in the layout.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the item name should be displayed; otherwise, <c>false</c>.
    /// </value>
    public bool ShowItemName { get; set; } = true;

    /// <summary>
    ///     Gets or sets the unique key that identifies this layout configuration.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> derived from the <see cref="LayoutOptionsAttribute.OptionsName" />
    ///     declared on the concrete class.
    /// </value>
    public string Key { get; set; }
}

