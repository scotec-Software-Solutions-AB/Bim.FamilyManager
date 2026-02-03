using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Ui.FamilyExplorer.Options;
using Bim.FamilyManager.Ui.ViewModels;
using Microsoft.Extensions.Options;

namespace Bim.FamilyManager.Ui.FamilyExplorer.ViewModels;

/// <summary>
///     Represents the view model for a family source in the Revit Family Manager with family explorer layout options.
/// </summary>
/// <remarks>
///     This class provides functionality to manage and interact with a family source, including its folders and selection
///     state,
///     using family explorer layout options. It extends <see cref="FamilySourceViewModel{FamilyExplorerLayoutOptions}" />
///     and
///     customizes
///     folder view model creation.
/// </remarks>
public class FamilySourceViewModel : FamilySourceViewModel<FamilyExplorerLayoutOptions>
{
    /// <summary>
    ///     Factory delegate for creating instances of <see cref="FamilySourceViewModel" />.
    /// </summary>
    /// <param name="familySource">The family source to be managed by the view model.</param>
    /// <returns>A new instance of <see cref="FamilySourceViewModel" />.</returns>
    /// <remarks>
    ///     This delegate is used for dependency injection and dynamic creation of family source view models.
    /// </remarks>
    public delegate FamilySourceViewModel Factory(IFamilySource familySource);

    private readonly FolderViewModel.Factory _folderFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySourceViewModel" /> class.
    /// </summary>
    /// <param name="familySource">An instance of <see cref="IFamilySource" /> representing the family source to be managed.</param>
    /// <param name="folderFactory">A factory delegate for creating <see cref="FolderViewModel" /> instances.</param>
    /// <param name="panelFactory">A factory delegate for creating <see cref="IFamilySourcePanelViewModel" /> instances.</param>
    /// <param name="layoutOptions">
    ///     An <see cref="IOptionsMonitor{FamilyExplorerLayoutOptions}" /> providing the current and updated
    ///     layout options.
    /// </param>
    /// <remarks>
    ///     This constructor sets up the view model with the specified family source, folder factory, panel factory, and layout
    ///     options.
    ///     It enables interaction with the family source's data and its associated folders and panel.
    /// </remarks>
    public FamilySourceViewModel(
        IFamilySource familySource,
        FolderViewModel.Factory folderFactory,
        IFamilySourcePanelViewModel.Factory panelFactory,
        IOptionsMonitor<FamilyExplorerLayoutOptions> layoutOptions)
        : base(familySource, panelFactory, layoutOptions)
    {
        _folderFactory = folderFactory;
    }

    /// <summary>
    ///     Creates a folder view model for the specified folder.
    /// </summary>
    /// <param name="folder">The <see cref="IFolder" /> instance to create a view model for.</param>
    /// <returns>An <see cref="IFolderViewModel" /> representing the folder.</returns>
    /// <remarks>
    ///     This method uses the injected folder factory to create a view model for the given folder.
    ///     It allows customization of folder view model instantiation in derived classes.
    /// </remarks>
    protected override IFolderViewModel CreateFolderViewModel(IFolder folder)
    {
        return _folderFactory(folder);
    }
}
