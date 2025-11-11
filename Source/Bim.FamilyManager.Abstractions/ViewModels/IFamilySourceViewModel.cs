namespace Bim.FamilyManager.Abstractions.ViewModels;

/// <summary>
///     Represents the view model for a family source in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This interface defines properties and behaviors specific to managing and interacting with a family source,
///     including its associated folders and the currently selected folder. It extends the base functionality
///     provided by <see cref="IFamilyManagerItemViewModel" />.
/// </remarks>
public interface IFamilySourceViewModel : IFamilyManagerItemViewModel
{
    /// <summary>
    ///     Gets the collection of folder view models associated with the family source.
    /// </summary>
    /// <value>
    ///     A list of <see cref="IFolderViewModel" /> instances representing the folders
    ///     contained within the family source. Returns <c>null</c> if no folders are available.
    /// </value>
    /// <remarks>
    ///     This property provides access to the hierarchical structure of folders associated
    ///     with the family source, enabling navigation and management of families within
    ///     the Revit Family Manager.
    /// </remarks>
    public IList<IFolderViewModel>? Folders { get; }

    /// <summary>
    ///     Gets or sets the currently selected folder within the family source.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="IFolderViewModel" /> representing the selected folder, or <c>null</c> if no folder is
    ///     selected.
    /// </value>
    /// <remarks>
    ///     This property allows interaction with the folder currently selected in the family source.
    ///     It is commonly used for filtering or displaying content specific to the selected folder.
    /// </remarks>
    public IFolderViewModel? SelectedFolder { get; }
    
    public IFamilySourcePanelViewModel? Panel { get; }
}
