using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Base.Options;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scotec.Events.WeakEvents;
using Scotec.Revit;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;

namespace Bim.FamilyManager.Ui.ViewModels;

/// <summary>
///     Represents a view model for a Revit family, providing access to family data, commands, and related symbols.
/// </summary>
/// <typeparam name="TLayoutOptions">The type of layout options used for display configuration.</typeparam>
/// <remarks>
///     Encapsulates a <see cref="IRevitFamily" /> instance and exposes properties and commands for interacting with family
///     details, such as its name, preview image, and symbols.
/// </remarks>
public abstract class FamilyViewModel<TLayoutOptions> : FamilyManagerItemViewModel<TLayoutOptions>, IFamilyViewModel
    where TLayoutOptions : LayoutOptions
{
    /// <summary>
    ///     Default image used when no preview is available.
    /// </summary>
    /// <remarks>
    ///     Used as a fallback when the family does not provide a preview image.
    /// </remarks>
    private static readonly BitmapImage NoPreviewImage;

    /// <summary>
    ///     Default image used when a download image is required.
    /// </summary>
    /// <remarks>
    ///     Used as a fallback when the family is not initialized and a preview image is not available.
    /// </remarks>
    private static readonly BitmapImage DownloadImage;

    private readonly Func<FamilyDropHandler> _dropHandlerFactory;
    private readonly RelayCommand _editFamilyCommand;
    private readonly IFamilyManager _familyManager;
    private readonly RelayCommand _loadFamilyCommand;
    private readonly ILogger<FamilyViewModel<TLayoutOptions>> _logger;
    private readonly RelayCommand _removeFamilyCommand;
    private readonly RevitTask _revitTask;
    private IList<IFamilySymbolViewModel>? _symbols;

    /// <summary>
    ///     Initializes static resources for the <see cref="FamilyViewModel{TLayoutOptions}" /> class.
    /// </summary>
    /// <remarks>
    ///     Loads default images for use when preview or download images are required.
    /// </remarks>
    static FamilyViewModel()
    {
        NoPreviewImage = new BitmapImage(new Uri("pack://application:,,,/Bim.FamilyManager.Ui;component/Resources/Images/NoPreview_128x128.png",
            UriKind.Absolute));
        DownloadImage = new BitmapImage(new Uri("pack://application:,,,/Bim.FamilyManager.Ui;component/Resources/Images/Download_128x128.png",
            UriKind.Absolute));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilyViewModel{TLayoutOptions}" /> class.
    /// </summary>
    /// <param name="family">The <see cref="IRevitFamily" /> instance to be managed.</param>
    /// <param name="familyManager">The <see cref="IFamilyManager" /> responsible for family operations.</param>
    /// <param name="dropHandlerFactory">Factory for creating <see cref="FamilyDropHandler" /> instances.</param>
    /// <param name="layoutOptions">Monitor for layout/display options.</param>
    /// <param name="revitTask">Task runner for Revit operations.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    /// <remarks>
    ///     Sets up commands, event handlers, and initializes the view model with provided dependencies.
    /// </remarks>
    protected FamilyViewModel(IRevitFamily family, IFamilyManager familyManager,
                              Func<FamilyDropHandler> dropHandlerFactory,
                              IOptionsMonitor<TLayoutOptions> layoutOptions,
                              RevitTask revitTask,
                              ILogger<FamilyViewModel<TLayoutOptions>> logger)
        : base(layoutOptions)
    {
        _familyManager = familyManager;
        _dropHandlerFactory = dropHandlerFactory;
        _revitTask = revitTask;
        _logger = logger;
        _editFamilyCommand = new RelayCommand(OpenFamily);
        _loadFamilyCommand = new RelayCommand(LoadFamily);
        _removeFamilyCommand = new RelayCommand(RemoveFamily, CanRemoveFamily);
        Family = family;

        StaticWeakEventManager.AddWeakHandler(family, nameof(IRevitFamily.Initialized),
            (sender, args) =>
            {
                // Invalidate symbols cache on reinitialization to ensure they are recreated.
                _symbols = null;
                Application.Current.Dispatcher.Invoke(OnInitialized, DispatcherPriority.ApplicationIdle);
            });

        StaticWeakEventManager.AddWeakHandler(family, nameof(IRevitFamily.LoadedInDocumentChanged),
            (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(NotifyChanges);
            });
    }

    private void OnInitialized()
    {
        OnPropertyChanged(nameof(Preview));
        OnPropertyChanged(nameof(Product));
        OnPropertyChanged(nameof(ProductVersion));
        OnPropertyChanged(nameof(Updated));
        OnPropertyChanged(nameof(Symbols));
    }

    /// <summary>
    ///     Gets a handler for drag-and-drop operations involving the family.
    /// </summary>
    /// <remarks>
    ///     The handler is created using the provided factory and enables drag-and-drop functionality for the family.
    /// </remarks>
    public IControllableDropHandler DropHandler => _dropHandlerFactory();

    /// <summary>
    ///     Indicates whether the family is loaded in the active Revit document.
    /// </summary>
    /// <remarks>
    ///     Returns <c>true</c> if the family is loaded in the document; otherwise, <c>false</c>.
    /// </remarks>
    public bool IsLoadedInDocument => Family.IsLoadedInDocument;

    /// <summary>
    ///     Gets the name of the family.
    /// </summary>
    /// <remarks>
    ///     The name is retrieved from the encapsulated <see cref="IRevitFamily" /> instance.
    /// </remarks>
    public override string Name => Family.Name;

    /// <summary>
    ///     Gets the encapsulated <see cref="IRevitFamily" /> instance.
    /// </summary>
    /// <remarks>
    ///     Provides access to the underlying family data and metadata.
    /// </remarks>
    public IRevitFamily Family { get; }

    /// <summary>
    ///     Gets the preview image of the family.
    /// </summary>
    /// <remarks>
    ///     Returns the preview image if available, otherwise a default image.
    /// </remarks>
    public override ImageSource? Preview => 
        GetPreview(Family.Preview);

    /// <summary>
    ///     Command to edit the family.
    /// </summary>
    /// <remarks>
    ///     Executes logic to open the family for editing.
    /// </remarks>
    public ICommand EditFamilyCommand => _editFamilyCommand;

    /// <summary>
    ///     Command to load the family into the current document.
    /// </summary>
    /// <remarks>
    ///     Executes logic to load the family into the active Revit document.
    /// </remarks>
    public ICommand LoadFamilyCommand => _loadFamilyCommand;

    /// <summary>
    ///     Command to remove the family from the current document.
    /// </summary>
    /// <remarks>
    ///     Executes logic to remove the family from the active Revit document.
    /// </remarks>
    public ICommand RemoveFamilyCommand => _removeFamilyCommand;

    /// <summary>
    ///     Gets the collection of symbols associated with the family.
    /// </summary>
    /// <value>
    ///     A list of <see cref="IFamilySymbolViewModel" /> instances representing the symbols of the family.
    /// </value>
    /// <remarks>
    ///     The collection is lazily initialized and ordered by symbol name.
    /// </remarks>
    public IList<IFamilySymbolViewModel> Symbols => _symbols ??= Family.FamilySymbols
                                                                       .Select(CreateSymbolViewModel)
                                                                       .OrderBy(symbol => symbol.Name)
                                                                       .ToList();

    /// <summary>
    ///     Gets the product name associated with the family.
    /// </summary>
    /// <remarks>
    ///     The product name is provided by the underlying <see cref="IRevitFamily" />.
    /// </remarks>
    public string Product => Family.Product;

    /// <summary>
    ///     Gets the product version associated with the family.
    /// </summary>
    /// <remarks>
    ///     The product version is provided by the underlying <see cref="IRevitFamily" />.
    /// </remarks>
    public string ProductVersion => Family.ProductVersion;

    /// <summary>
    ///     Gets the last updated date of the family.
    /// </summary>
    /// <remarks>
    ///     The date is provided by the underlying <see cref="IRevitFamily" />.
    /// </remarks>
    public DateTime Updated => Family.Updated;

    /// <summary>
    ///     Creates a view model for a given family symbol.
    /// </summary>
    /// <param name="symbol">The <see cref="IRevitFamilySymbol" /> to wrap.</param>
    /// <returns>An <see cref="IFamilySymbolViewModel" /> instance.</returns>
    /// <remarks>
    ///     This method must be implemented by derived classes to provide the appropriate symbol view model.
    /// </remarks>
    protected abstract IFamilySymbolViewModel CreateSymbolViewModel(IRevitFamilySymbol symbol);

    /// <summary>
    ///     Determines if the family can be removed from the document.
    /// </summary>
    /// <returns><c>true</c> if the family is loaded; otherwise, <c>false</c>.</returns>
    /// <remarks>
    ///     Used to control the enabled state of the remove command.
    /// </remarks>
    private bool CanRemoveFamily()
    {
        return IsLoadedInDocument;
    }

    /// <summary>
    ///     Removes the family from the active document.
    /// </summary>
    /// <remarks>
    ///     Executes the removal operation and notifies property changes. Logs errors if the operation fails.
    /// </remarks>
    private async void RemoveFamily()
    {
        try
        {
            await _revitTask.Run(_ => { _familyManager.RemoveFamilyFromActiveDocument(Family); });
            NotifyChanges();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, $"Error while removing family from the active document. Family: {Family.Name}");
        }
    }

    /// <summary>
    ///     Opens the family for editing.
    /// </summary>
    /// <remarks>
    ///     Invokes the family manager to open the family for editing.
    /// </remarks>
    private void OpenFamily()
    {
        _familyManager.EditFamily(Family);
    }

    /// <summary>
    ///     Loads the family into the active document.
    /// </summary>
    /// <remarks>
    ///     Executes the load operation and notifies property changes. Logs errors if the operation fails.
    /// </remarks>
    private async void LoadFamily()
    {
        try
        {
            var family = await _revitTask.Run(_ =>
            {
                _familyManager.TryLoadFamilyIntoActiveDocument(Family, out var loadedFamily);
                return loadedFamily;
            }).ConfigureAwait(true);

            NotifyChanges();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, $"Error while loading family into the active document. Family: {Family.Name}");
        }
    }

    /// <summary>
    ///     Notifies property changes and updates command states.
    /// </summary>
    /// <remarks>
    ///     Ensures the UI reflects the current state of the family and its commands.
    /// </remarks>
    private void NotifyChanges()
    {
        OnPropertyChanged(nameof(IsLoadedInDocument));
        _removeFamilyCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    ///     Gets the preview image for the family, or a default image if none is available.
    /// </summary>
    /// <param name="preview">Stream containing the preview image data.</param>
    /// <returns>An <see cref="ImageSource" /> for the preview image.</returns>
    /// <remarks>
    ///     Returns a default image if the preview stream is null.
    /// </remarks>
    private ImageSource? GetPreview(Stream? preview)
    {
        return preview is null
            ? GetDefaultPreviewImage()
            : GetPreviewImage(preview, Color.FromRgb(255, 255, 255));
    }

    /// <summary>
    ///     Gets the default preview image for the family.
    /// </summary>
    /// <returns>A <see cref="BitmapImage" /> representing the default preview image.</returns>
    /// <remarks>
    ///     Returns a "No Preview" image if the family is initialized, otherwise returns a "Download" image.
    /// </remarks>
    private BitmapImage GetDefaultPreviewImage()
    {
        return Family.IsInitialized
            ? NoPreviewImage
            : DownloadImage;
    }
}
