using Microsoft.Extensions.Options;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Ui.Modern.Options;
using Bim.FamilyManager.Ui.ViewModels;
using System.Windows.Media;

namespace Bim.FamilyManager.Ui.Modern.ViewModels;

/// <summary>
///     Represents the view model for a family source in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This class provides functionality to manage and interact with a family source, including its folders and selection
///     state.
/// </remarks>
public class FamilySourceViewModel : FamilySourceViewModel<ModernLayoutOptions>
{
    /// <summary>
    ///     A factory delegate for creating instances of <see cref="FamilySourceViewModel" />.
    /// </summary>
    /// <param name="familySource">
    ///     The <see cref="Bim.FamilyManager.Abstractions.IFamilySource" /> instance representing the family source to
    ///     be managed by the created view model.
    /// </param>
    /// <returns>
    ///     A new instance of <see cref="FamilySourceViewModel" /> configured with the specified
    ///     <paramref name="familySource" />.
    /// </returns>
    public delegate FamilySourceViewModel Factory(IFamilySource familySource);
    
    private readonly IFamilySource _familySource;
    private readonly FolderViewModel.Factory _folderFactory;
    private IList<IFolderViewModel>? _folders;

    private IFolderViewModel? _selectedFolder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySourceViewModel" /> class.
    /// </summary>
    /// <param name="familySource">
    ///     An instance of <see cref="IFamilySource" /> representing the family source to be managed by this view model.
    /// </param>
    /// <param name="folderFactory">
    ///     A factory delegate for creating instances of <see cref="FolderViewModel" /> to represent the folders within the
    ///     family source.
    /// </param>
    /// <remarks>
    ///     This constructor sets up the view model with the specified family source and folder factory, enabling interaction
    ///     with the family source's data and its associated folders.
    /// </remarks>
    public FamilySourceViewModel(IFamilySource familySource, 
                                 FolderViewModel.Factory folderFactory,
                                 IFamilySourcePanelViewModel.Factory panelFactory,
                                 IOptionsMonitor<ModernLayoutOptions> layoutOptions)
        : base(familySource, panelFactory, layoutOptions)
    {
        _familySource = familySource;
        _folderFactory = folderFactory;

        Preview = familySource.Preview is null ? null : GetPreviewImage(familySource.Preview);
    }


    /// <summary>
    ///     Gets the name of the family source represented by this view model.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the family source.
    /// </value>
    /// <remarks>
    ///     This property retrieves the name from the underlying <see cref="IFamilySource" /> instance.
    /// </remarks>
    public override string Name => _familySource.Name;

    public override ImageSource? Preview { get; }

    protected override IFolderViewModel CreateFolderViewModel(IFolder folder)
    {
        return _folderFactory(folder);
    }
}
