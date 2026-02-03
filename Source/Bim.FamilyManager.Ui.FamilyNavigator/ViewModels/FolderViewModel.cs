using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Ui.FamilyNavigator.Options;
using Bim.FamilyManager.Ui.ViewModels;
using Microsoft.Extensions.Options;

namespace Bim.FamilyManager.Ui.FamilyNavigator.ViewModels;

/// <summary>
///     Provides a view model for a folder, enabling interaction with folder data, subfolders, and families using family
///     navigator
///     layout options.
/// </summary>
/// <remarks>
///     This class extends <see cref="FolderViewModel{FamilyNavigatorLayoutOptions}" /> and customizes the creation of
///     subfolder
///     and family view models.
///     It manages dependencies for folder operations, subfolder instantiation, and family instantiation.
/// </remarks>
public class FolderViewModel : FolderViewModel<FamilyNavigatorLayoutOptions>
{
    /// <summary>
    ///     Delegate for creating instances of <see cref="FolderViewModel" />.
    /// </summary>
    /// <param name="folder">The <see cref="IFolder" /> to be managed by the view model.</param>
    /// <returns>A new instance of <see cref="FolderViewModel" />.</returns>
    /// <remarks>
    ///     This delegate is used for dependency injection and dynamic instantiation of folder view models.
    /// </remarks>
    public delegate FolderViewModel Factory(IFolder folder);

    private readonly FamilyViewModel.Factory _familyFactory;
    private readonly Factory _subfolderFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FolderViewModel" /> class.
    /// </summary>
    /// <param name="folder">The <see cref="IFolder" /> instance representing the folder to be managed.</param>
    /// <param name="subfolderFactory">A factory delegate for creating subfolder view models.</param>
    /// <param name="familyFactory">A factory delegate for creating family view models.</param>
    /// <param name="layoutOptions">
    ///     An <see cref="IOptionsMonitor{FamilyNavigatorLayoutOptions}" /> for monitoring layout
    ///     options.
    /// </param>
    /// <remarks>
    ///     This constructor sets up the view model with the provided dependencies, enabling management of folder data,
    ///     subfolder instantiation, family instantiation, and layout configuration.
    /// </remarks>
    public FolderViewModel(
        IFolder folder,
        Factory subfolderFactory,
        FamilyViewModel.Factory familyFactory,
        IOptionsMonitor<FamilyNavigatorLayoutOptions> layoutOptions)
        : base(folder, layoutOptions)
    {
        _subfolderFactory = subfolderFactory;
        _familyFactory = familyFactory;
    }

    /// <summary>
    ///     Creates a subfolder view model for the specified subfolder.
    /// </summary>
    /// <param name="subfolder">The <see cref="IFolder" /> to create a subfolder view model for.</param>
    /// <returns>An <see cref="IFolderViewModel" /> representing the subfolder.</returns>
    /// <remarks>
    ///     This method uses the injected subfolder factory to create a view model for the given subfolder.
    ///     It allows customization of subfolder view model instantiation.
    /// </remarks>
    protected override IFolderViewModel CreateSubfolderViewModel(IFolder subfolder)
    {
        return _subfolderFactory(subfolder);
    }

    /// <summary>
    ///     Creates a family view model for the specified Revit family.
    /// </summary>
    /// <param name="family">The <see cref="IRevitFamily" /> to create a family view model for.</param>
    /// <returns>An <see cref="IFamilyViewModel" /> representing the Revit family.</returns>
    /// <remarks>
    ///     This method uses the injected family factory to create a view model for the given Revit family.
    ///     It allows customization of family view model instantiation.
    /// </remarks>
    protected override IFamilyViewModel CreateFamilyViewModel(IRevitFamily family)
    {
        return _familyFactory(family);
    }
}
