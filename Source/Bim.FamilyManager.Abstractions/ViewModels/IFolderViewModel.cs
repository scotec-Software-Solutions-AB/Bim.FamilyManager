namespace Bim.FamilyManager.Abstractions.ViewModels;

/// <summary>
///     Represents a view model for a folder in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This interface provides properties and behaviors specific to folder items within the Revit Family Manager.
///     It includes access to the folder's subfolders, families, and its expanded state.
/// </remarks>
public interface IFolderViewModel : IFamilyManagerItemViewModel
{
    /// <summary>
    ///     Gets the folder associated with this view model.
    /// </summary>
    /// <remarks>
    ///     The folder represents a hierarchical structure in the Revit Family Manager,
    ///     containing subfolders and families. This property provides access to the
    ///     underlying folder data, including its name, path, subfolders, and families.
    /// </remarks>
    public IFolder Folder { get; }

    /// <summary>
    ///     Gets the collection of subfolder view models within the current folder.
    /// </summary>
    /// <value>
    ///     A collection of <see cref="IFolderViewModel" /> instances representing the subfolders of the current folder.
    ///     Returns <c>null</c> if there are no subfolders.
    /// </value>
    /// <remarks>
    ///     This property provides access to the hierarchical structure of folders in the Revit Family Manager.
    ///     It allows navigation and manipulation of subfolder items.
    /// </remarks>
    public IEnumerable<IFolderViewModel>? Subfolders { get; }

    /// <summary>
    ///     Gets the collection of Revit family view models associated with the folder.
    /// </summary>
    /// <remarks>
    ///     This property provides access to the families contained within the folder represented by this view model.
    ///     Each family is represented by an <see cref="IFamilyViewModel" />, which allows interaction with and manipulation
    ///     of the Revit families.
    /// </remarks>
    public IEnumerable<IFamilyViewModel>? Families { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the folder is expanded in the view.
    /// </summary>
    /// <remarks>
    ///     When set to <c>true</c>, the folder's contents, such as subfolders and families, are visible.
    ///     When set to <c>false</c>, the folder's contents are collapsed and hidden.
    ///     This property is typically bound to a UI element, such as a toggle button, to control the folder's visibility.
    /// </remarks>
    public bool IsExpanded { get; set; }
}
