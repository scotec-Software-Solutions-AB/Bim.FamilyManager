using System.Windows.Media;
using Bim.FamilyManager.Core.Abstractions;
using Bim.FamilyManager.Ui.Abstractions.ViewModels;
using Bim.FamilyManager.Ui.FamilyExplorer.Options;
using Bim.FamilyManager.Ui.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bim.FamilyManager.Ui.FamilyExplorer.ViewModels;

/// <summary>
///     View model for a family source in the Family Explorer layout.
/// </summary>
public class FamilySourceViewModel : FamilySourceViewModel<FamilyExplorerLayoutOptions>
{
    /// <summary>Factory delegate for DI-based creation.</summary>
    public delegate FamilySourceViewModel Factory(IFamilySource familySource);

    private static readonly ImageSource? SourceIcon;

    private readonly FolderViewModel.Factory _folderFactory;

    static FamilySourceViewModel()
    {
        const string packUri = "pack://application:,,,/Bim.FamilyManager.Ui;component/Resources/Images/FamilySourcesPrimary_24x24.png";
        SourceIcon = GetPreviewImage(LoadPackUriAsStream(packUri));
        SourceIcon?.Freeze();
    }

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
    /// <param name="logger">The logger used to report errors while loading the folder structure.</param>
    /// <remarks>
    ///     This constructor sets up the view model with the specified family source, folder factory, panel factory, and layout
    ///     options.
    ///     It enables interaction with the family source's data and its associated folders and panel.
    /// </remarks>
    public FamilySourceViewModel(
        IFamilySource familySource,
        FolderViewModel.Factory folderFactory,
        IFamilySourcePanelViewModel.Factory panelFactory,
        IOptionsMonitor<FamilyExplorerLayoutOptions> layoutOptions,
        ILogger<FamilySourceViewModel<FamilyExplorerLayoutOptions>> logger)
        : base(familySource, panelFactory, layoutOptions, logger)
    {
        _folderFactory = folderFactory;
    }

    /// <inheritdoc />
    public override ImageSource? Preview => SourceIcon;

    /// <inheritdoc />
    protected override IFolderViewModel CreateFolderViewModel(IFolder folder)
    {
        return _folderFactory(folder);
    }
}
