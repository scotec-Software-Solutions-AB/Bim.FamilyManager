using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Base.Logic;
using Bim.FamilyManager.Base.Options;
using Microsoft.Extensions.Options;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Ui.ViewModels;

/// <summary>
///     Provides a base view model for items in the Revit Family Manager UI, supporting selection state,
///     preview image display, and configurable layout options.
/// </summary>
/// <typeparam name="TLayoutOptions">
///     The type of layout options used to configure the display of the item. Must derive from
///     <see cref="LayoutOptions" />.
/// </typeparam>
/// <remarks>
///     This abstract class implements common functionality for view models representing items in the
///     Revit Family Manager, such as families, folders, or sources. It manages display options
///     (e.g., text block height, preview image visibility) and provides a utility for converting
///     preview image streams to <see cref="ImageSource" /> objects, optionally applying transparency.
///     Derived classes must implement the <see cref="Name" /> and <see cref="Preview" /> properties.
/// </remarks>
public abstract class FamilyManagerItemViewModel<TLayoutOptions> : ViewModel, IFamilyManagerItemViewModel
    where TLayoutOptions : LayoutOptions
{
    private int _contentHeight;
    private bool _isSelected;
    private bool _showItemName;
    private bool _showPreviewImage;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilyManagerItemViewModel{TLayoutOptions}" /> class.
    /// </summary>
    /// <param name="layoutOptions">
    ///     The <see cref="IOptionsMonitor{TOptions}" /> that provides the current and updated layout options of type
    ///     <typeparamref name="TLayoutOptions" />.
    /// </param>
    /// <remarks>
    ///     This constructor subscribes to changes in the layout options and applies the current options to configure
    ///     display-related properties.
    /// </remarks>
    protected FamilyManagerItemViewModel(IOptionsMonitor<TLayoutOptions> layoutOptions)
    {
        layoutOptions.OnChange(SetDisplayOptions);

        SetDisplayOptions(layoutOptions.CurrentValue);
    }

    /// <summary>
    ///     Gets the name of the item represented by this view model.
    /// </summary>
    /// <remarks>
    ///     Derived classes must provide a human-readable identifier for the item, typically displayed in the UI.
    /// </remarks>
    public abstract string Name { get; }

    /// <summary>
    ///     Gets the preview image associated with the item.
    /// </summary>
    /// <remarks>
    ///     Derived classes must provide an <see cref="ImageSource" /> representing the item's preview, or <c>null</c> if
    ///     unavailable.
    /// </remarks>
    public abstract ImageSource? Preview { get; }

    /// <summary>
    ///     Gets the height of the text block associated with the item.
    /// </summary>
    /// <remarks>
    ///     This property determines the vertical size of text blocks in the UI and is updated from layout options.
    /// </remarks>
    public virtual int ContentHeight
    {
        get => _contentHeight;
        protected set => SetProperty(ref _contentHeight, value);
    }

    /// <summary>
    ///     Gets a value indicating whether the preview image should be displayed for the item.
    /// </summary>
    /// <remarks>
    ///     This property is updated from layout options and controls the visibility of the preview image in the UI.
    /// </remarks>
    public virtual bool ShowPreviewImage
    {
        get => _showPreviewImage;
        protected set => SetProperty(ref _showPreviewImage, value);
    }

    /// <summary>
    ///     Gets a value indicating whether the item's name should be displayed in the UI.
    /// </summary>
    /// <remarks>
    ///     This property is updated from layout options and controls the visibility of the item's name in the UI.
    /// </remarks>
    public virtual bool ShowItemName
    {
        get => _showItemName;
        protected set => SetProperty(ref _showItemName, value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the item is selected.
    /// </summary>
    /// <remarks>
    ///     This property tracks the selection state of the item and can be used to highlight or perform actions on selected
    ///     items.
    /// </remarks>
    public virtual bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            SetProperty(ref _isSelected, value);
        }
    }

    /// <summary>
    ///     Called when display options are set or changed, allowing derived classes to react to layout changes.
    /// </summary>
    /// <param name="options">The new <typeparamref name="TLayoutOptions" /> to apply.</param>
    /// <remarks>
    ///     Override this method in derived classes to handle additional display option changes.
    /// </remarks>
    protected virtual void OnSetDisplayOptions(TLayoutOptions options)
    {
    }

    /// <summary>
    ///     Converts the provided family preview stream into an <see cref="ImageSource" />.
    /// </summary>
    /// <param name="preview">
    ///     The <see cref="Stream" /> containing the preview image data, such as a Revit family preview.
    /// </param>
    /// <param name="transparentColor">
    ///     An optional <see cref="Color" /> to be treated as transparent in the image. If not provided, no transparency is
    ///     applied.
    /// </param>
    /// <returns>
    ///     An <see cref="ImageSource" /> representing the processed preview image. Returns <c>null</c> if the input stream is
    ///     <c>null</c>.
    /// </returns>
    /// <remarks>
    ///     This method creates a <see cref="BitmapImage" /> from the provided stream, ensuring it is fully loaded and frozen
    ///     for thread safety.
    ///     If a <paramref name="transparentColor" /> is specified, the method processes the image to make pixels matching the
    ///     color transparent.
    ///     This is commonly used to display Revit family preview images in the user interface.
    /// </remarks>
    protected static ImageSource? GetPreviewImage(Stream? preview, Color? transparentColor = null)
    {
        if (preview is null)
        {
            return null;
        }

        return Helper.CreateBitmapFromStream(preview, transparentColor);
    }

    /// <summary>
    ///     Applies the specified layout options to the view model's display-related properties.
    /// </summary>
    /// <param name="options">The <typeparamref name="TLayoutOptions" /> to apply.</param>
    /// <remarks>
    ///     This method updates <see cref="ContentHeight" />, <see cref="ShowPreviewImage" />, and
    ///     <see cref="ShowItemName" />
    ///     based on the provided options, and then calls <see cref="OnSetDisplayOptions" /> for further customization.
    /// </remarks>
    private void SetDisplayOptions(TLayoutOptions options)
    {
        ContentHeight = options.ContentHeight;
        ShowPreviewImage = options.ShowPreviewImage;
        ShowItemName = options.ShowItemName;

        OnSetDisplayOptions(options);
    }
}
