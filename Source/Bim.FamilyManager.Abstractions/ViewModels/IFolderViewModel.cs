namespace Bim.FamilyManager.Abstractions.ViewModels;

/// <summary>
///     Defines the view model contract for a folder in Bim.FamilyManager.
/// </summary>
/// <remarks>
///     Provides properties and behaviors for folder items, including access to the underlying folder data, subfolders,
///     families, and expanded state.
///     Used to represent and manage hierarchical folder structures in the Family Manager UI.
/// </remarks>
public interface IFolderViewModel : IFamilyManagerItemViewModel
{
    /// <summary>
    ///     Gets the folder data associated with this view model.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="IFolder" /> representing the underlying folder, including its name, path, subfolders, and
    ///     families.
    /// </value>
    /// <remarks>
    ///     Provides access to the hierarchical structure and contents of the folder.
    /// </remarks>
    IFolder Folder { get; }

    /// <summary>
    ///     Gets the collection of subfolder view models within the current folder.
    /// </summary>
    /// <value>
    ///     A collection of <see cref="IFolderViewModel" /> instances representing the subfolders, or <c>null</c> if there are
    ///     no subfolders.
    /// </value>
    /// <remarks>
    ///     Enables navigation and management of nested folder structures in the Family Manager.
    /// </remarks>
    IEnumerable<IFolderViewModel>? Subfolders { get; }

    /// <summary>
    ///     Gets the collection of Revit family view models contained in the folder.
    /// </summary>
    /// <value>
    ///     A collection of <see cref="IFamilyViewModel" /> instances representing the families in the folder, or <c>null</c>
    ///     if there are no families.
    /// </value>
    /// <remarks>
    ///     Provides access to the families stored within the folder for display and interaction in the UI.
    /// </remarks>
    IEnumerable<IFamilyViewModel>? Families { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the folder is expanded in the UI.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the folder's contents are visible; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     Controls the visibility of subfolders and families in the UI, typically bound to an expand/collapse control.
    /// </remarks>
    bool IsExpanded { get; set; }
}
