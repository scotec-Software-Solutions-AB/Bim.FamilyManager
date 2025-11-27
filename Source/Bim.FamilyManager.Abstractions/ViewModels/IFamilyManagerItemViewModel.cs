using System.Windows.Media;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels;

/// <summary>
///     Defines the base interface for view models representing items in Bim.FamilyManager.
/// </summary>
/// <remarks>
///     Provides common properties for items displayed in the Family Manager, including name, preview image, content
///     height, and selection state.
///     Serves as a foundation for more specific item view models such as family sources, folders, and families.
/// </remarks>
public interface IFamilyManagerItemViewModel : IViewModel
{
    /// <summary>
    ///     Gets the name of the item.
    /// </summary>
    /// <remarks>
    ///     Provides a human-readable identifier for the item, typically shown in the user interface.
    /// </remarks>
    string Name { get; }

    /// <summary>
    ///     Gets the preview image associated with the item.
    /// </summary>
    /// <value>
    ///     An <see cref="ImageSource" /> representing the item's preview image, or <c>null</c> if unavailable.
    /// </value>
    /// <remarks>
    ///     Used to visually represent the item in the UI.
    /// </remarks>
    ImageSource? Preview { get; }

    /// <summary>
    ///     Gets the height of the content block for the item.
    /// </summary>
    /// <remarks>
    ///     Determines the height of UI elements displaying item-related information.
    /// </remarks>
    int ContentHeight { get; }

    /// <summary>
    ///     Gets a value indicating whether the preview image should be displayed.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the preview image is visible; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     Controls the visibility of the item's preview image in the UI.
    /// </remarks>
    bool ShowPreviewImage { get; }

    /// <summary>
    ///     Gets a value indicating whether the item's name should be displayed.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the item's name is visible; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     Controls the visibility of the item's name in the UI.
    /// </remarks>
    bool ShowItemName { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the item is selected.
    /// </summary>
    /// <remarks>
    ///     Tracks the selection state of the item within the Family Manager.
    /// </remarks>
    bool IsSelected { get; set; }
}
