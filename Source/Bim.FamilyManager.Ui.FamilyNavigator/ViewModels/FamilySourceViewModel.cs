using System.IO;
using System.Windows.Media;
using Bim.FamilyManager.Core.Abstractions;
using Bim.FamilyManager.Ui.Abstractions.ViewModels;
using Bim.FamilyManager.Ui.FamilyNavigator.Options;
using Bim.FamilyManager.Ui.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bim.FamilyManager.Ui.FamilyNavigator.ViewModels;

/// <summary>
///     View model for a family source in the Family Navigator layout.
/// </summary>
public class FamilySourceViewModel : FamilySourceViewModel<FamilyNavigatorLayoutOptions>
{
    /// <summary>Factory delegate for DI-based creation.</summary>
    public delegate FamilySourceViewModel Factory(IFamilySource familySource);

    private static readonly ImageSource? SourceIcon;

    private readonly IFamilySource _familySource;
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
    /// <param name="familySource">The family source to be managed by this view model.</param>
    /// <param name="folderFactory">Factory for creating folder view models.</param>
    /// <param name="panelFactory">Factory for creating family source panel view models.</param>
    /// <param name="layoutOptions">Monitor for layout options.</param>
    /// <param name="logger">The logger used to report errors while loading the folder structure.</param>
    /// <remarks>
    ///     Sets up the view model with the specified family source and folder factory, and initializes the preview image.
    /// </remarks>
    public FamilySourceViewModel(
        IFamilySource familySource,
        FolderViewModel.Factory folderFactory,
        IFamilySourcePanelViewModel.Factory panelFactory,
        IOptionsMonitor<FamilyNavigatorLayoutOptions> layoutOptions,
        ILogger<FamilySourceViewModel<FamilyNavigatorLayoutOptions>> logger)
        : base(familySource, panelFactory, layoutOptions, logger)
    {
        _familySource = familySource;
        _folderFactory = folderFactory;
    }

    /// <inheritdoc />
    public override string Name => _familySource.Name;

    /// <inheritdoc />
    public override ImageSource? Preview => SourceIcon;

    /// <inheritdoc />
    protected override IFolderViewModel CreateFolderViewModel(IFolder folder)
    {
        return _folderFactory(folder);
    }
}
