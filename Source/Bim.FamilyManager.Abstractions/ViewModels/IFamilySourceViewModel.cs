namespace Bim.FamilyManager.Abstractions.ViewModels;

/// <summary>
///     Defines the view model contract for a family source in Bim.FamilyManager.
/// </summary>
/// <remarks>
///     Provides properties and behaviors for managing and interacting with a family source, including access to its
///     folders, the currently selected folder, and an associated panel view model.
///     Extends <see cref="IFamilyManagerItemViewModel" /> to include common item properties.
/// </remarks>
public interface IFamilySourceViewModel : IFamilyManagerItemViewModel
{
    /// <summary>
    ///     Gets the collection of folder view models associated with the family source.
    /// </summary>
    /// <value>
    ///     A list of <see cref="IFolderViewModel" /> instances representing the folders contained within the family source, or
    ///     <c>null</c> if no folders are available.
    /// </value>
    /// <remarks>
    ///     Provides access to the hierarchical structure of folders for navigation and management of families.
    /// </remarks>
    IList<IFolderViewModel>? Folders { get; }

    /// <summary>
    ///     Gets the currently selected folder within the family source.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="IFolderViewModel" /> representing the selected folder, or <c>null</c> if no folder is
    ///     selected.
    /// </value>
    /// <remarks>
    ///     Used for interaction and filtering of content specific to the selected folder.
    /// </remarks>
    IFolderViewModel? SelectedFolder { get; }

    /// <summary>
    ///     Gets the panel view model associated with the family source.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="IFamilySourcePanelViewModel" /> representing the panel for the family source, or
    ///     <c>null</c> if not available.
    /// </value>
    /// <remarks>
    ///     Provides source-specific content, controls, or configuration for display in the UI.
    /// </remarks>
    IFamilySourcePanelViewModel? Panel { get; }
}
