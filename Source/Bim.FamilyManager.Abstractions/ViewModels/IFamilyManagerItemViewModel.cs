using System.Windows.Media;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels;

/// <summary>
///     Represents the base interface for view models in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This interface defines common properties and behaviors for items displayed in the Revit Family Manager,
///     such as their name, preview image, and selection state. It serves as a foundation for more specific
///     view model interfaces, such as family sources, folders, and families.
/// </remarks>
public interface IFamilyManagerItemViewModel : IViewModel
{
    /// <summary>
    ///     Gets the name of the item represented by this view model.
    /// </summary>
    /// <remarks>
    ///     The <see cref="Name" /> property provides a human-readable identifier for the item,
    ///     which is typically displayed in the user interface. It is used to distinguish
    ///     between different items in the Revit Family Manager.
    /// </remarks>
    public string Name { get; }

    /// <summary>
    ///     Gets the preview image associated with the item.
    /// </summary>
    /// <value>
    ///     An <see cref="ImageSource" /> representing the preview image of the item, or <c>null</c> if no preview is
    ///     available.
    /// </value>
    /// <remarks>
    ///     The preview image is typically used to visually represent the item in the user interface.
    ///     It can be displayed in various views, such as family sources, folders, or families.
    /// </remarks>
    public ImageSource? Preview { get; }

    /// <summary>
    ///     Gets the height of the text block associated with the item.
    /// </summary>
    /// <remarks>
    ///     This property determines the height of the content used in the UI to display item-related information.
    ///     It is commonly used to ensure consistent sizing of UI elements such as images and text blocks.
    /// </remarks>
    public int ContentHeight { get; }

    /// <summary>
    ///     Gets a value indicating whether the preview image should be displayed for the item.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the preview image should be displayed; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property is typically used in the UI to control the visibility of the preview image
    ///     associated with the item. It is bound to the visibility of the image element in the view.
    /// </remarks>
    public bool ShowPreviewImage { get; }

    /// <summary>
    ///     Gets a value indicating whether the item's name should be displayed in the UI.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the item's name is visible; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property is typically used to control the visibility of the item's name in the user interface.
    ///     It is bound to UI elements, such as <see cref="System.Windows.Controls.TextBlock" />,
    ///     and can be toggled to show or hide the item's name dynamically.
    /// </remarks>
    public bool ShowItemName { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the item is selected.
    /// </summary>
    /// <remarks>
    ///     This property is used to track the selection state of an item within the Revit Family Manager.
    ///     It is commonly utilized to highlight or perform actions on selected items.
    /// </remarks>
    public bool IsSelected { get; set; }
}
