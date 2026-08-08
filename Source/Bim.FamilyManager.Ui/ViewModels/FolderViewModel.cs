using System.Windows.Media;
using Bim.FamilyManager.Core.Abstractions;
using Bim.FamilyManager.Ui.Abstractions.ViewModels;
using Bim.FamilyManager.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bim.FamilyManager.Ui.ViewModels;

/// <summary>
///     Represents the view model for a folder in the Revit Family Manager application.
/// </summary>
public abstract class FolderViewModel<TLayoutOptions> : FamilyManagerItemViewModel<TLayoutOptions>, IFolderViewModel
    where TLayoutOptions : LayoutOptions
{
private static readonly ImageSource? FolderIcon;

private readonly ILogger<FolderViewModel<TLayoutOptions>> _logger;
private IEnumerable<IFamilyViewModel>? _directFamilies;
private bool _directFamiliesLoading;
private IEnumerable<IFamilyViewModel>? _families;
private bool _familiesLoading;
    private bool _isExpanded;
    private IEnumerable<IFolderViewModel>? _subfolders;
    private bool _subfoldersLoading;

    static FolderViewModel()
    {
        const string packUri = "pack://application:,,,/Bim.FamilyManager.Ui;component/Resources/Images/Folder_128x128.png";
        FolderIcon = GetPreviewImage(LoadPackUriAsStream(packUri));
        FolderIcon?.Freeze();
    }

    /// <param name="logger">The logger used to report errors while loading subfolders and families.</param>
    /// <remarks>
    ///     This constructor sets up the <see cref="FolderViewModel{TLayoutOptions}" /> with the provided folder data and
    ///     layout options, enabling
    ///     interaction with folder properties and preview image. It integrates with the <see cref="IFolder" /> abstraction and
    ///     supports dynamic creation of subfolder and family view models.
    /// </remarks>
    protected FolderViewModel(IFolder folder, IOptionsMonitor<TLayoutOptions> layoutOptions,
                              ILogger<FolderViewModel<TLayoutOptions>> logger)
        : base(layoutOptions)
    {
        Folder = folder;
        _logger = logger;
        Preview = folder.Preview is not null
            ? GetPreviewImage(folder.Preview)
            : FolderIcon;
    }

    /// <summary>
    ///     Gets the name of the folder represented by this view model.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the folder.
    /// </value>
    /// <remarks>
    ///     This property retrieves the name of the folder from the associated <see cref="IFolder" /> instance.
    ///     It is commonly used for display purposes in the UI.
    /// </remarks>
    public override string Name => Folder.Name;

    /// <summary>
    ///     Gets the preview image for this folder. Returns the source-provided image when available;
    ///     falls back to the shared default folder icon otherwise.
    /// </summary>
    public override ImageSource? Preview { get; }

    /// <summary>
    ///     Gets the folder associated with this view model.
    /// </summary>
    /// <value>
    ///     The <see cref="IFolder" /> instance representing the folder's data.
    /// </value>
    /// <remarks>
    ///     This property provides access to the underlying <see cref="IFolder" /> instance,
    ///     which represents the folder's data, including its name, path, subfolders, and families.
    /// </remarks>
    public IFolder Folder { get; }

    /// <summary>
    ///     Gets the collection of subfolders associated with the current folder.
    /// </summary>
    /// <value>
    ///     An <see cref="IEnumerable{IFolderViewModel}" /> representing the subfolders, or <c>null</c> if there are no
    ///     subfolders.
    /// </value>
    /// <remarks>
    ///     This property retrieves subfolder data from the underlying <see cref="IFolder" /> abstraction
    ///     and creates corresponding <see cref="FolderViewModel{TLayoutOptions}" /> instances using the provided factory.
    ///     GetSubfoldersAsync are loaded only when the folder is expanded or selected for performance reasons.
    /// </remarks>
    public IEnumerable<IFolderViewModel>? Subfolders
    {
        get
        {
            if (IsExpanded || IsSelected)
            {
                if (_subfolders is null && !_subfoldersLoading)
                {
                    _ = LoadSubfoldersAsync();
                }
            }

            return _subfolders;
        }
    }

    private async Task LoadSubfoldersAsync()
    {
        _subfoldersLoading = true;
        try
        {
            var subfolders = new List<IFolder>();
            await foreach (var subfolder in Folder.GetSubfoldersAsync(CancellationToken.None))
            {
                subfolders.Add(subfolder);
            }

            _subfolders = subfolders.OrderBy(f => f.Name)
                                    .Select(sf => TryCreateViewModel(sf, CreateSubfolderViewModel, sf.Name))
                                    .Where(vm => vm is not null)
                                    .Select(vm => vm!)
                                    .ToList();

            OnPropertyChanged(nameof(Subfolders));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while loading subfolders. Folder: {Folder}", Folder.Name);
            _subfolders = [];
        }
        finally
        {
            _subfoldersLoading = false;
        }
    }

    // Performance: Families and Subfolders are loaded only when the folder is expanded or selected.
    public IEnumerable<IFamilyViewModel>? Families
    {
        get
        {
            if (IsExpanded || IsSelected)
            {
                if (_families is null && !_familiesLoading)
                {
                    _ = LoadFamiliesAsync();
                }
            }

            return _families;
        }
    }

    private async Task LoadFamiliesAsync()
    {
        _familiesLoading = true;
        try
        {
            var families = new List<IRevitFamily>();
            await foreach (var family in Folder.GetFamiliesAsync(true, CancellationToken.None))
            {
                families.Add(family);
            }

            _families = families.OrderBy(f => f.Name)
                                .Select(f => TryCreateViewModel(f, CreateFamilyViewModel, f.Name))
                                .Where(vm => vm is not null)
                                .Select(vm => vm!)
                                .ToList();

            OnPropertyChanged(nameof(Families));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while loading families. Folder: {Folder}", Folder.Name);
            _families = [];
        }
        finally
        {
            _familiesLoading = false;
        }
    }

    /// <summary>
    ///     Gets the collection of families located directly in the folder, excluding families in subfolders.
    /// </summary>
    /// <value>
    ///     A collection of <see cref="IFamilyViewModel" /> objects representing the families stored directly in the
    ///     folder, or <c>null</c> if the folder is neither expanded nor selected.
    /// </value>
    /// <remarks>
    ///     Unlike <see cref="Families" />, which aggregates the families of all subfolders, this property only returns
    ///     the families whose files are direct children of the folder. It is used by views that render subfolders as
    ///     separate sections and would otherwise never display the folder's own families.
    /// </remarks>
    public IEnumerable<IFamilyViewModel>? DirectFamilies
    {
        get
        {
            // Performance: Do not load the families if the folder is not expanded or selected.
            if (IsExpanded || IsSelected)
            {
                if (_directFamilies is null && !_directFamiliesLoading)
                {
                    _ = LoadDirectFamiliesAsync();
                }
            }

            return _directFamilies;
        }
    }

    private async Task LoadDirectFamiliesAsync()
    {
        _directFamiliesLoading = true;
        try
        {
            var families = new List<IRevitFamily>();
            await foreach (var family in Folder.GetFamiliesAsync(false, CancellationToken.None))
            {
                families.Add(family);
            }

            _directFamilies = families.OrderBy(f => f.Name)
                                      .Select(f => TryCreateViewModel(f, CreateFamilyViewModel, f.Name))
                                      .Where(vm => vm is not null)
                                      .Select(vm => vm!)
                                      .ToList();

            OnPropertyChanged(nameof(DirectFamilies));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while loading direct families. Folder: {Folder}", Folder.Name);
            _directFamilies = [];
        }
        finally
        {
            _directFamiliesLoading = false;
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the folder is expanded in the UI.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the folder is expanded; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     When the folder is expanded, its associated families are loaded and displayed.
    ///     Changing this property triggers the <see cref="Families" /> property to update, ensuring
    ///     that the families are loaded only when needed. This property is typically bound to a UI element to control
    ///     expansion state.
    /// </remarks>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            SetProperty(ref _isExpanded, value);
            if (_families is null)
            {
                // First time expanded. Notify property change to ensure families are loaded.
                OnPropertyChanged(nameof(Families));
            }

            if (_directFamilies is null)
            {
                OnPropertyChanged(nameof(DirectFamilies));
            }
        }
    }

    protected abstract IFolderViewModel CreateSubfolderViewModel(IFolder subfolder);

    protected abstract IFamilyViewModel CreateFamilyViewModel(IRevitFamily family);

    /// <summary>
    ///     Creates a view model for the specified item, logging and returning <c>null</c> on failure.
    /// </summary>
    /// <typeparam name="TItem">The type of the item to wrap.</typeparam>
    /// <typeparam name="TViewModel">The type of the created view model.</typeparam>
    /// <param name="item">The item to create a view model for.</param>
    /// <param name="factory">The factory used to create the view model.</param>
    /// <param name="itemName">The display name of the item, used for logging.</param>
    /// <returns>The created view model, or <c>null</c> if the factory threw an exception.</returns>
    /// <remarks>
    ///     A single faulty item must not prevent the remaining items of the folder from being displayed.
    /// </remarks>
    private TViewModel? TryCreateViewModel<TItem, TViewModel>(TItem item, Func<TItem, TViewModel> factory, string itemName)
        where TViewModel : class
    {
        try
        {
            return factory(item);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while creating the view model. Folder: {Folder}, Item: {Item}", Folder.Name, itemName);
            return null;
        }
    }
}
