using Bim.FamilyManager.Abstractions.Options;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Bim.FamilyManager.Base.Options;

/// <summary>
///     Represents the base class for layout options used in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This abstract class provides common properties and functionality for configuring layout options,
///     such as the key identifier, text block height, and visibility settings for preview images and item names.
///     Derived classes can extend this functionality to define specific layout configurations.
/// </remarks>
public abstract class LayoutOptions : ILayoutOptions
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LayoutOptions" /> class.
    /// </summary>
    /// <remarks>
    ///     This constructor sets the <see cref="Key" /> property to the name of the derived class,
    ///     with the "Options" suffix removed. It ensures that the <see cref="Key" /> property
    ///     is initialized with a meaningful default value based on the class name.
    /// </remarks>
    protected LayoutOptions()
    {
        Key = GetType().Name.Replace("Options", "");
    }

    /// <summary>
    ///     Gets or sets the height of the content used in the layout.
    /// </summary>
    /// <value>
    ///     The height of the content area, measured in pixels. The default value is 64.
    /// </value>
    /// <remarks>
    ///     This property is used to configure the vertical size of content areas within the layout.
    ///     It is commonly utilized in UI components to ensure consistent content area dimensions.
    /// </remarks>
    public int ContentHeight { get; set; } = 64;

    /// <summary>
    ///     Gets or sets a value indicating whether the preview image should be displayed in the layout.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the preview image is displayed; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property allows enabling or disabling the visibility of the preview image in the layout.
    ///     It is set to <c>true</c> by default.
    /// </remarks>
    public bool ShowPreviewImage { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether the item name should be displayed in the layout.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the item name should be displayed; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property is used to control the visibility of item names in the layout configuration.
    ///     It is particularly useful for customizing the display settings in various UI components
    ///     of the Revit Family Manager.
    /// </remarks>
    public bool ShowItemName { get; set; } = true;

    /// <summary>
    ///     Gets or sets the unique identifier for the layout options.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the key that identifies the layout options.
    /// </value>
    /// <remarks>
    ///     The <see cref="Key" /> property is automatically initialized to the name of the derived class
    ///     with the "Options" suffix removed. This ensures that the key is meaningful and corresponds
    ///     to the specific layout configuration.
    /// </remarks>
    public string Key { get; set; }
}
