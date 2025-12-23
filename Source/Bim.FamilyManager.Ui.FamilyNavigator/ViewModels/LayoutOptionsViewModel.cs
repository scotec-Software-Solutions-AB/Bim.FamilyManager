using Bim.FamilyManager.Abstractions.Options;
using Bim.FamilyManager.Abstractions.ViewModels.Settings;
using Bim.FamilyManager.Ui.FamilyNavigator.Options;
using Bim.FamilyManager.Ui.FamilyNavigator.Resources;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Ui.FamilyNavigator.ViewModels;

/// <summary>
///     Represents the view model for layout options in the Family Manager application.
/// </summary>
/// <remarks>
///     This view model provides properties and logic for managing layout options such as
///     showing item names, preview images, and configuring text block height. It ensures
///     that at least one of the item name or preview image is always visible.
/// </remarks>
public class LayoutOptionsViewModel : ViewModel, ILayoutOptionsViewModel
{
    private readonly FamilyNavigatorLayoutOptions _options;
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
    public LayoutOptionsViewModel(FamilyNavigatorLayoutOptions options)
    {
        _options = options;

        _contentHeight = _options.ContentHeight;
        _showItemName = _options.ShowItemName;
        _showPreviewImage = _options.ShowPreviewImage;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the preview image should be shown.
    /// </summary>
    /// <remarks>
    ///     If both <see cref="ShowPreviewImage" /> and <see cref="ShowItemName" /> are set to false,
    ///     <see cref="ShowItemName" /> will be automatically set to true to ensure at least one is visible.
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
    ///     Gets or sets a value indicating whether the item name should be shown.
    /// </summary>
    /// <remarks>
    ///     If both <see cref="ShowItemName" /> and <see cref="ShowPreviewImage" /> are set to false,
    ///     <see cref="ShowPreviewImage" /> will be automatically set to true to ensure at least one is visible.
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
    ///     Gets or sets the height of the text block.
    /// </summary>
    /// <remarks>
    ///     This property controls the vertical size of text blocks in the layout.
    /// </remarks>
    public int ContentHeight
    {
        get => _contentHeight;
        set => SetProperty(ref _contentHeight, value);
    }

    public int ContentMinHeight => 32;
    public int ContentMaxHeight => 128;

    /// <summary>
    ///     Retrieves the current layout options as a new <see cref="FamilyNavigatorLayoutOptions" /> instance.
    /// </summary>
    /// <returns>
    ///     An instance of <see cref="ILayoutOptions" /> representing the current configuration.
    /// </returns>
    /// <remarks>
    ///     This method is used to obtain the current state of the layout options for persistence or further processing.
    /// </remarks>
    public ILayoutOptions GetOptions()
    {
        return new FamilyNavigatorLayoutOptions
        {
            ShowItemName = ShowItemName,
            ShowPreviewImage = ShowPreviewImage,
            ContentHeight = ContentHeight
        };
    }

    /// <summary>
    ///     Gets the display name of the layout.
    /// </summary>
    /// <remarks>
    ///     The layout name is retrieved from the string resources.
    /// </remarks>
    public string LayoutName => StringResources.LayoutSettings_LayoutName;

    /// <summary>
    ///     Gets the unique key for the layout options.
    /// </summary>
    /// <remarks>
    ///     The key is used to identify the specific layout configuration.
    /// </remarks>
    public string Key => _options.Key;
}
