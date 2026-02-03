using System.Windows.Media;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Ui.FamilyNavigator.Options;
using Bim.FamilyManager.Ui.ViewModels;
using Microsoft.Extensions.Options;

namespace Bim.FamilyManager.Ui.FamilyNavigator.ViewModels;

/// <summary>
///     View model for a family source in the Revit Family Manager, providing access to its folders, preview image, and
///     selection state.
/// </summary>
/// <remarks>
///     This class manages the interaction with a family source, including its folders and preview image, and supports UI
///     binding.
/// </remarks>
public class FamilySourceViewModel : FamilySourceViewModel<FamilyNavigatorLayoutOptions>
{
    /// <summary>
    ///     Factory delegate for creating <see cref="FamilySourceViewModel" /> instances.
    /// </summary>
    /// <param name="familySource">The <see cref="IFamilySource" /> instance to be managed by the created view model.</param>
    /// <returns>
    ///     A new <see cref="FamilySourceViewModel" /> instance configured with the specified
    ///     <paramref name="familySource" />.
    /// </returns>
    public delegate FamilySourceViewModel Factory(IFamilySource familySource);

    private readonly IFamilySource _familySource;
    private readonly FolderViewModel.Factory _folderFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySourceViewModel" /> class.
    /// </summary>
    /// <param name="familySource">The family source to be managed by this view model.</param>
    /// <param name="folderFactory">Factory for creating folder view models.</param>
    /// <param name="panelFactory">Factory for creating family source panel view models.</param>
    /// <param name="layoutOptions">Monitor for layout options.</param>
    /// <remarks>
    ///     Sets up the view model with the specified family source and folder factory, and initializes the preview image.
    /// </remarks>
    public FamilySourceViewModel(
        IFamilySource familySource,
        FolderViewModel.Factory folderFactory,
        IFamilySourcePanelViewModel.Factory panelFactory,
        IOptionsMonitor<FamilyNavigatorLayoutOptions> layoutOptions)
        : base(familySource, panelFactory, layoutOptions)
    {
        _familySource = familySource;
        _folderFactory = folderFactory;

        Preview = familySource.Preview is null ? null : GetPreviewImage(familySource.Preview);
    }

    /// <summary>
    ///     Gets the name of the family source represented by this view model.
    /// </summary>
    /// <value>A <see cref="string" /> representing the name of the family source.</value>
    /// <remarks>
    ///     Retrieves the name from the underlying <see cref="IFamilySource" /> instance.
    /// </remarks>
    public override string Name => _familySource.Name;

    /// <summary>
    ///     Gets the preview image for the family source, if available.
    /// </summary>
    /// <value>An <see cref="ImageSource" /> representing the preview image, or <c>null</c> if not available.</value>
    public override ImageSource? Preview { get; }

    /// <summary>
    ///     Creates a folder view model for the specified folder.
    /// </summary>
    /// <param name="folder">The folder to create a view model for.</param>
    /// <returns>An <see cref="IFolderViewModel" /> representing the folder.</returns>
    protected override IFolderViewModel CreateFolderViewModel(IFolder folder)
    {
        return _folderFactory(folder);
    }
}
