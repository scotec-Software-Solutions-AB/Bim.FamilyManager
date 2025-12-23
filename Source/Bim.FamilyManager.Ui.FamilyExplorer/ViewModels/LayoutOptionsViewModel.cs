using Bim.FamilyManager.Abstractions.Options;
using Bim.FamilyManager.Abstractions.ViewModels.Settings;
using Bim.FamilyManager.Ui.FamilyExplorer.Options;
using Bim.FamilyManager.Ui.FamilyExplorer.Resources;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Ui.FamilyExplorer.ViewModels;

/// <summary>
///     Represents the view model for managing layout options in the Family Manager UI.
/// </summary>
/// <remarks>
///     This view model provides properties and logic for configuring layout options such as
///     displaying item names, preview images, and text block height. It ensures that at least
///     one of the display options (item name or preview image) is always enabled.
/// </remarks>
public class LayoutOptionsViewModel : ViewModel, ILayoutOptionsViewModel
{
    private readonly FamilyExplorerLayoutOptions _options;
    private int _contentHeight;
    private bool _showItemName;
    private bool _showPreviewImage;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LayoutOptionsViewModel" /> class.
    /// </summary>
    /// <param name="options">The layout options to initialize the view model with.</param>
    /// <remarks>
    ///     The constructor sets the initial values for the view model properties based on the provided options.
    /// </remarks>
    public LayoutOptionsViewModel(FamilyExplorerLayoutOptions options)
    {
        _options = options;

        _contentHeight = _options.ContentHeight;
        _showItemName = _options.ShowItemName;
        _showPreviewImage = _options.ShowPreviewImage;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the preview image should be shown in the layout.
    /// </summary>
    /// <remarks>
    ///     If both <see cref="ShowPreviewImage" /> and <see cref="ShowItemName" /> are set to false,
    ///     <see cref="ShowItemName" /> will be automatically set to true to ensure at least one display option is enabled.
    /// </remarks>
    public bool ShowPreviewImage
    {
        get => _showPreviewImage;
        set
        {
            SetProperty(ref _showPreviewImage, value);
            if (!value && !ShowItemName)
            {
                ShowItemName = true;
            }
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the item name should be shown in the layout.
    /// </summary>
    /// <remarks>
    ///     If both <see cref="ShowItemName" /> and <see cref="ShowPreviewImage" /> are set to false,
    ///     <see cref="ShowPreviewImage" /> will be automatically set to true to ensure at least one display option is enabled.
    /// </remarks>
    public bool ShowItemName
    {
        get => _showItemName;
        set
        {
            SetProperty(ref _showItemName, value);
            if (!value && !ShowPreviewImage)
            {
                ShowPreviewImage = true;
            }
        }
    }

    /// <summary>
    ///     Gets or sets the height of the text block in the layout.
    /// </summary>
    /// <remarks>
    ///     This property controls the vertical size of text blocks within the layout.
    /// </remarks>
    public int ContentHeight
    {
        get => _contentHeight;
        set => SetProperty(ref _contentHeight, value);
    }

    /// <summary>
    ///     Gets the minimum height for the content area in the layout.
    /// </summary>
    /// <value>
    ///     The minimum height value, which is a constant set to 32.
    /// </value>
    /// <remarks>
    ///     This property defines the lower boundary for the content height in the layout settings.
    ///     It is used in conjunction with <see cref="ContentMaxHeight" /> and <see cref="ContentHeight" />
    ///     to ensure the content height remains within a valid range.
    /// </remarks>
    public int ContentMinHeight => 32;

    /// <summary>
    ///     Gets the maximum height for the content area in the layout.
    /// </summary>
    /// <value>
    ///     The maximum height value, which is a constant set to 128.
    /// </value>
    /// <remarks>
    ///     This property defines the upper boundary for the content height in the layout settings.
    ///     It is used in conjunction with <see cref="ContentMinHeight" /> and <see cref="ContentHeight" />
    ///     to ensure the content height remains within a valid range.
    /// </remarks>
    public int ContentMaxHeight => 128;

    /// <summary>
    ///     Gets the localized name of the layout.
    /// </summary>
    /// <remarks>
    ///     This property retrieves the layout name from the string resources for display in the UI.
    /// </remarks>
    public string LayoutName => StringResources.LayoutSettings_LayoutName;

    /// <summary>
    ///     Retrieves the current layout options as a <see cref="ILayoutOptions" /> instance.
    /// </summary>
    /// <returns>
    ///     A <see cref="FamilyExplorerLayoutOptions" /> object representing the current configuration.
    /// </returns>
    /// <remarks>
    ///     This method is used to obtain the current state of the layout options for persistence or further processing.
    /// </remarks>
    public ILayoutOptions GetOptions()
    {
        return new FamilyExplorerLayoutOptions
        {
            ShowItemName = ShowItemName,
            ShowPreviewImage = ShowPreviewImage,
            ContentHeight = ContentHeight
        };
    }

    /// <summary>
    ///     Gets the unique key associated with the layout options.
    /// </summary>
    /// <remarks>
    ///     The key is used to identify the specific layout configuration.
    /// </remarks>
    public string Key => _options.Key;
}
