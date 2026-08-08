using System.Windows.Media;
using Bim.FamilyManager.Core.Abstractions;
using Bim.FamilyManager.Ui.Abstractions.ViewModels;
using Bim.FamilyManager.Ui.FamilyExplorer.Options;
using Bim.FamilyManager.Ui.ViewModels;
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
    public FamilySourceViewModel(
        IFamilySource familySource,
        FolderViewModel.Factory folderFactory,
        IFamilySourcePanelViewModel.Factory panelFactory,
        IOptionsMonitor<FamilyExplorerLayoutOptions> layoutOptions)
        : base(familySource, panelFactory, layoutOptions)
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
