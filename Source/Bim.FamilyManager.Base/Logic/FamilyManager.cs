using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.Options;
using Bim.FamilyManager.Base.Logic.EStorage;
using Bim.FamilyManager.Base.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenMcdf;
using Scotec.Extensions.Utilities.Strings;
using Scotec.Queues;
using Application = Autodesk.Revit.ApplicationServices.Application;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using TaskDialogCommonButtons = Autodesk.Revit.UI.TaskDialogCommonButtons;
using TaskDialogResult = Autodesk.Revit.UI.TaskDialogResult;
using Version = System.Version;

namespace Bim.FamilyManager.Base.Logic;

/// <summary>
///     Provides functionality to manage Revit families, including retrieving the root folder,
///     searching for families, and organizing them into folders and subfolders.
/// </summary>
/// <remarks>
///     This class implements the <see cref="Bim.FamilyManager.Abstractions.IFamilyManager" /> interface
///     and serves as the core component for handling Revit family-related operations.
/// </remarks>
public sealed class FamilyManager : IFamilyManager, IDisposable
{
    private readonly IOptions<FamilyManagerOptions> _defaultOptions;
    private readonly Dictionary<int, string> _documentPaths = new();
    private readonly RevitFamilyCache _familyCache;
    private readonly TaskQueue<IRevitFamily> _familyInitializationQueue;
    private readonly FamilyMetadataEStorage _familyMetadataEStorage = new();
    private readonly ILogger _logger;
    private readonly IDisposable? _optionsChangeTracker;
    private readonly PreviewImageEStorage _previewImageEStorage = new();
    private readonly UIControlledApplication _revitApplication;
    private readonly IServiceProvider _services;
    private Document? _activeDocument;
    private Dictionary<ElementId, string> _familyElementIds = [];
    private List<IFamilySourceOptions> _familySourceOptions;
    private IEnumerable<IFamilySource>? _familySources;
    private HashSet<string> _loadedFamilies = [];
    private CancellationTokenSource _tokenSource = new CancellationTokenSource();

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilyManager" /> class.
    /// </summary>
    /// <param name="defaultOptions">
    ///     The default options for the Family Manager, encapsulated in an
    ///     <see cref="IOptions{TOptions}" /> object.
    /// </param>
    /// <param name="options">
    ///     A monitor for tracking changes to family source options, provided as an
    ///     <see cref="IOptionsMonitor{TOptions}" />.
    /// </param>
    /// <param name="revitApplication">
    ///     The Revit application instance, represented by <see cref="Autodesk.Revit.UI.UIControlledApplication" />.
    /// </param>
    /// <param name="services">
    ///     The service provider for dependency injection, represented by <see cref="System.IServiceProvider" />.
    /// </param>
    /// <param name="logger">
    ///     The logger instance for logging operations, represented by
    ///     <see cref="ILogger{TCategoryName}" />.
    /// </param>
    /// <remarks>
    ///     This constructor sets up the Family Manager by initializing its dependencies, subscribing to Revit application
    ///     events,
    ///     and preparing the family initialization queue and cache. It also monitors changes to family source options and
    ///     reloads
    ///     them when necessary.
    /// </remarks>
    public FamilyManager(IOptions<FamilyManagerOptions> defaultOptions, IOptionsMonitor<FamilySourcesOptions> options, UIControlledApplication revitApplication,
                         IServiceProvider services, ILogger<FamilyManager> logger)
    {
        _defaultOptions = defaultOptions;
        _revitApplication = revitApplication;
        _services = services;
        _logger = logger;

        _familySourceOptions = GetAllSourcesFromOptions(options.CurrentValue);
        _optionsChangeTracker = options.OnChange(sourcesOptions =>
        {
            // The event is triggered even when an unrelated change is made to the configuration file.
            // Therefore, ensure that the sources have actually been modified before proceeding.
            if (CheckForChanges(_familySourceOptions, GetAllSourcesFromOptions(sourcesOptions)))
            {
                _ = Reload(sourcesOptions);
            }
        });

        _revitApplication.ViewActivating += OnViewActivating;
        _revitApplication.ControlledApplication.DocumentChanged += OnDocumentChanged;

        _familyInitializationQueue = new TaskQueue<IRevitFamily>(InitializeFamilies);
        _familyInitializationQueue.Start();

        _familyCache = new RevitFamilyCache(family => _familyInitializationQueue.Enqueue(family));

        _revitApplication.ControlledApplication.DocumentClosing += OnDocumentClosing;
        _revitApplication.ControlledApplication.DocumentClosed += OnDocumentClosed;
        _revitApplication.ControlledApplication.DocumentSaved += OnDocumentSaved;
        _revitApplication.ControlledApplication.DocumentSaving += OnDocumentSaving;
        _revitApplication.ControlledApplication.DocumentSavedAs += OnDocumentSavedAs;
        _revitApplication.ControlledApplication.DocumentSavingAs += OnDocumentSavingAs;
    }

    /// <summary>
    ///     Releases all resources used by the <see cref="FamilyManager" /> instance.
    /// </summary>
    /// <remarks>
    ///     This method stops the family initialization queue, clears its contents, disposes of any tracked option changes,
    ///     and unsubscribes from all Revit application events to ensure proper cleanup of resources.
    /// </remarks>
    public void Dispose()
    {
        _familyInitializationQueue.Stop();
        _familyInitializationQueue.Clear();

        _optionsChangeTracker?.Dispose();

        _revitApplication.ViewActivating -= OnViewActivating;
        _revitApplication.ControlledApplication.DocumentChanged -= OnDocumentChanged;

        _revitApplication.ControlledApplication.DocumentClosing -= OnDocumentClosing;
        _revitApplication.ControlledApplication.DocumentClosed -= OnDocumentClosed;
        _revitApplication.ControlledApplication.DocumentSaved -= OnDocumentSaved;
        _revitApplication.ControlledApplication.DocumentSaving -= OnDocumentSaving;
        _revitApplication.ControlledApplication.DocumentSavedAs -= OnDocumentSavedAs;
        _revitApplication.ControlledApplication.DocumentSavingAs -= OnDocumentSavingAs;
    }

    /// <summary>
    ///     Gets the collection of family sources available for managing Revit families.
    /// </summary>
    /// <value>
    ///     An <see cref="IEnumerable{T}" /> of <see cref="IFamilySource" /> representing the sources
    ///     from which Revit families can be retrieved or managed.
    /// </value>
    /// <remarks>
    ///     This property lazily initializes the collection of family sources by invoking the
    ///     <c>GetFamilySources</c> method if the sources have not already been loaded.
    ///     Each family source provides access to folders and families, and supports operations
    ///     such as reloading and saving families.
    /// </remarks>
    public IEnumerable<IFamilySource> FamilySources => _familySources ??= GetFamilySources();

    /// <summary>
    ///     Reloads the family manager by clearing and restarting the family initialization queue,
    ///     resetting the family sources, and clearing the family cache.
    /// </summary>
    /// <remarks>
    ///     This method ensures that the family manager is refreshed and ready to handle updated
    ///     family sources and data. It also triggers the <see cref="Reloaded" /> event to notify
    ///     subscribers of the reload operation.
    /// </remarks>
    public async Task ReloadAsync()
    {
        await _tokenSource.CancelAsync();
        _tokenSource = new CancellationTokenSource();
        await Task.Run(() =>
        {

            _familyInitializationQueue.Stop();
            _familyInitializationQueue.Clear();

            _familySources = null;
            _familyCache.Clear();

            _familyInitializationQueue.Start();

            Reloaded?.Invoke(this, EventArgs.Empty);
        });
    }

    /// <summary>
    ///     Occurs when the family manager is reloaded.
    /// </summary>
    /// <remarks>
    ///     This event is triggered after the family manager has been refreshed, including clearing
    ///     and restarting the family initialization queue, resetting the family sources, and clearing
    ///     the family cache. Subscribers can use this event to perform actions in response to the
    ///     reload operation.
    /// </remarks>
    /// <example>
    ///     To subscribe to the <see cref="Reloaded" /> event:
    ///     <code>
    /// var familyManager = new FamilyManager(...);
    /// familyManager.Reloaded += (sender, args) =>
    /// {
    ///     Console.WriteLine("Family manager has been reloaded.");
    /// };
    /// </code>
    /// </example>
    public event EventHandler<EventArgs>? Reloaded;

    /// <summary>
    ///     Asynchronously searches for Revit families within the specified folder that match the given search pattern.
    /// </summary>
    /// <param name="folder">
    ///     The folder to search within. This folder may contain subfolders and Revit families.
    /// </param>
    /// <param name="searchPattern">
    ///     The search pattern to filter Revit families by their names. The search is case-insensitive.
    ///     If the search pattern is <see langword="null" /> or empty, no filtering is applied.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     An asynchronous stream of <see cref="Bim.FamilyManager.Abstractions.IRevitFamily" /> objects
    ///     that match the specified search pattern. If no families match, the stream will be empty.
    /// </returns>
    /// <remarks>
    ///     This method retrieves all Revit families from the leaf folders of the specified folder
    ///     and filters them based on the provided search pattern.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the <paramref name="folder" /> parameter is <see langword="null" />.
    /// </exception>
    public async IAsyncEnumerable<IRevitFamily> SearchRevitFamiliesAsync(IFolder folder, string searchPattern,
                                                                         [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (folder == null)
        {
            throw new ArgumentNullException(nameof(folder));
        }

        if (string.IsNullOrEmpty(searchPattern))
        {
            yield break;
        }

        await foreach (var family in GetAllFamiliesFromLeafFoldersAsync(folder, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (family.Name.Contains(searchPattern, StringComparison.OrdinalIgnoreCase))
            {
                yield return family;
            }
        }
    }

    /// <summary>
    ///     Opens and activates a Revit family for editing in the Revit application.
    /// </summary>
    /// <param name="revitFamily">
    ///     The <see cref="Bim.FamilyManager.Abstractions.IRevitFamily" /> instance representing the Revit family to
    ///     be edited.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when the <paramref name="revitFamily" /> parameter is <c>null</c>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when there is no active document set in the Family Manager.
    /// </exception>
    /// <remarks>
    ///     If the specified Revit family is not initialized, it will be initialized before being opened.
    ///     The method ensures that the family file is either retrieved or created in the working directory,
    ///     and then opens it in Revit for editing. If the family is already open in the application, it will be activated.
    /// </remarks>
    public void EditFamily(IRevitFamily revitFamily)
    {
        if (revitFamily == null)
        {
            throw new ArgumentNullException(nameof(revitFamily));
        }

        if (_activeDocument is null)
        {
            throw new InvalidOperationException("No active document has been set.");
        }

        if (!revitFamily.IsInitialized)
        {
            revitFamily.Initialize();
        }

        var uiApp = new UIApplication(_activeDocument!.Application);
        var isOpen = uiApp.Application.Documents.OfType<Document>().Any(d => d.Title.Equals(revitFamily.Name, StringComparison.OrdinalIgnoreCase));

        var familyFile = isOpen ? GetWorkingFilePath(revitFamily) : CreateFamilyFile(revitFamily);
        _ = uiApp.OpenAndActivateDocument(familyFile);
    }

    /// <summary>
    ///     Registers a Revit family with the family manager.
    /// </summary>
    /// <param name="family">
    ///     The <see cref="IRevitFamily" /> instance representing the Revit family to be registered.
    /// </param>
    /// <remarks>
    ///     This method adds or updates the specified Revit family in the internal cache.
    ///     If a family with the same name already exists, it will be updated with the provided instance.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the <paramref name="family" /> parameter is <c>null</c>.
    /// </exception>
    public void RegisterRevitFamily(IRevitFamily family)
    {
        _familyCache.AddOrUpdateFamily(family.Name, family, (_, _) => family);
    }

    /// <summary>
    ///     Attempts to retrieve a Revit family by its name from the internal cache.
    /// </summary>
    /// <param name="familyName">
    ///     The name of the Revit family to retrieve. This parameter is case-sensitive.
    /// </param>
    /// <param name="family">
    ///     When this method returns, contains the <see cref="IRevitFamily" /> instance if found;
    ///     otherwise, <see langword="null" />. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the Revit family with the specified name exists in the cache;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     This method provides a way to efficiently access Revit families that are already loaded
    ///     into the internal cache. It avoids the need for additional searches or loading operations.
    /// </remarks>
    public bool TryGetRevitFamily(string familyName, [MaybeNullWhen(false)] out IRevitFamily family)
    {
        return _familyCache.TryGetFamily(familyName, out family);
    }

    /// <summary>
    ///     Attempts to load a Revit family into the currently active document.
    /// </summary>
    /// <param name="revitFamily">
    ///     An instance of <see cref="Bim.FamilyManager.Abstractions.IRevitFamily" /> representing the Revit family to
    ///     be loaded.
    /// </param>
    /// <param name="family">
    ///     When this method returns, contains the loaded <see cref="Autodesk.Revit.DB.Family" /> instance if the operation
    ///     succeeds; otherwise, <c>null</c>. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the Revit family was successfully loaded into the active document; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the active document is <c>null</c>. Ensure that an active document is set before invoking this method.
    /// </exception>
    /// <remarks>
    ///     This method requires an active Revit document to function correctly. It starts a transaction to load the provided
    ///     Revit family instance into the active document. If the operation fails, the transaction is rolled back.
    /// </remarks>
    public bool TryLoadFamilyIntoActiveDocument(IRevitFamily revitFamily, [NotNullWhen(true)] out Family? family)
    {
        if (_activeDocument is null)
        {
            var message = "The active document cannot be null when using this method. Ensure that an active document is set before invoking this method.";
            _logger.LogError(message);
            throw new InvalidOperationException(message);
        }

        using var transaction = new Transaction(_activeDocument, "Load Family");
        transaction.Start();

        if (TryLoadFamily(revitFamily, _activeDocument, out family))
        {
            transaction.Commit();
            return true;
        }

        transaction.RollBack();
        return false;
    }

    /// <summary>
    ///     Removes the specified Revit family from the active document.
    /// </summary>
    /// <param name="revitFamily">
    ///     The <see cref="Bim.FamilyManager.Abstractions.IRevitFamily" /> instance representing the family to be
    ///     removed.
    /// </param>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the active document is <c>null</c>.
    ///     Ensure that an active document is set before invoking this method.
    /// </exception>
    /// <remarks>
    ///     This method starts a transaction to remove the specified family from the active document.
    ///     If the family is in use, additional logic may be required to handle user decisions.
    /// </remarks>
    public void RemoveFamilyFromActiveDocument(IRevitFamily revitFamily)
    {
        // TODO:Check if the family is used in the active document.
        //  If it is, display a message and allow the user to decide what action to take.
        if (_activeDocument is null)
        {
            var message = "The active document cannot be null when using this method. Ensure that an active document is set before invoking this method.";
            _logger.LogError(message);
            throw new InvalidOperationException(message);
        }

        using var transaction = new Transaction(_activeDocument, "Remove Family");
        transaction.Start();

        RemoveFamily(revitFamily, _activeDocument);

        transaction.Commit();
    }

    public void CreatePreviewImage(UIApplication application, View view)
    {
        var document = view.Document;
        // Only handle family documents.
        if (!document.IsFamilyDocument)
        {
            return;
        }

        var familyManager = document.FamilyManager;

        var previewImages = new Dictionary<string, Stream>();

        // The `TransactionGroup` is used here to group multiple transactions into a single logical unit.
        // This ensures that all changes made within the group can be rolled back together,
        // maintaining the integrity of the document. In this code, it allows iterating through family types,
        // setting the current family type, and generating preview images while providing the ability to undo
        // all changes at the end of the process. This approach ensures that no partial or
        // inconsistent changes are left in the document.

        using var transactionGroup = new TransactionGroup(document, "Create preview images");
        {
            transactionGroup.Start();
            foreach (var familyType in familyManager.Types.Cast<FamilyType>())
            {
                SetCurrentFamilyType(familyType);
                var previewImage = ViewImageExporter.ExportViewPng(document, view);

                //using var t = new Transaction(document, "Attach preview to family");
                //t.Start();

                //var settings = document.GetDocumentPreviewSettings();
                //settings.PreviewViewId = viewId;

                // TODO: Probably modify background to transparent or any other user defined color.
                //var pathName = GetFamilyAsStream(document, out var memoryStream);
                previewImage.Position = 0;

                previewImages[familyType.Name] = previewImage;
            }

            transactionGroup.RollBack();
        }
        using var transaction = new Transaction(document, "Add previews");
        transaction.Start();
        _previewImageEStorage.Attach(document.OwnerFamily, familyManager.CurrentType.Name, previewImages);
        transaction.Commit();

        void SetCurrentFamilyType(FamilyType familyType)
        {
            // This transaction is part of a transaction group and will be rolled back at the end, even if it is committed individually.
            using var familyTypeTransaction = new Transaction(document, "Set current family type");
            familyTypeTransaction.Start();
            familyManager.CurrentType = familyType;
            familyTypeTransaction.Commit();
        }
    }

    /// <summary>
    ///     Loads a Revit family into the specified document.
    /// </summary>
    /// <param name="revitFamily">
    ///     An instance of <see cref="Bim.FamilyManager.Abstractions.IRevitFamily" /> representing the family to be
    ///     loaded.
    /// </param>
    /// <param name="document">
    ///     The <see cref="Autodesk.Revit.DB.Document" /> into which the family will be loaded.
    /// </param>
    /// <param name="family"></param>
    /// <returns>
    ///     The loaded <see cref="Autodesk.Revit.DB.Family" /> instance.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the <paramref name="document" /> is not initialized or if the family fails to load.
    /// </exception>
    /// <remarks>
    ///     This method loads the specified family into the provided document. If the family already exists in the document,
    ///     it is reused unless the provided family has a newer version, in which case it is reloaded. Family metadata is
    ///     attached
    ///     after loading. Temporary files created during the process are cleaned up automatically.
    /// </remarks>
    public bool TryLoadFamily(IRevitFamily revitFamily, Document document, [NotNullWhen(true)] out Family? family)
    {
        family = null;
        var tempFilePath = CreateFamilyLocalFile(revitFamily.Family, revitFamily.Name, document.Application);

        // Loading the family does not work as expected.
        // If the family is not already loaded, both the family and all its symbols are loaded.
        // When attempting to load a family that already exists in the document, a call to document.LoadFamily() fails and it is not reloaded.
        // If the family is already loaded but some of its symbols are missing, calling document.LoadFamily() also fails, and the missing symbols are not loaded.
        // To load all missing symbols, each symbol must be loaded individually.
        // This behavior must be handled correctly to ensure that families and their symbols are loaded as intended.
        // In the context of the family manager, we aim to load all symbols of a family when the family is loaded or reloaded (e.g., when the "Load family" button in the family view is clicked),
        // even if the family has not been modified.

        try
        {
            var overwriteOptions = new OverwriteFamilyOption();
            var currentlyLoadedFamily = FindFamily(document, revitFamily);
            if (!document.LoadFamily(tempFilePath, overwriteOptions, out var loadedFamily))
            {
                if (overwriteOptions.IsCancelled)
                {
                    // The user attempted to load a different version of the family but canceled the operation.
                    // There is nothing further to do.
                    return false;
                }

                if (currentlyLoadedFamily is not null)
                {
                    // If the family is already loaded, calling document.LoadFamily will fail if the family being loaded has the same version.
                    // However, it is necessary to ensure that all symbols are loaded.

                    LoadMissingSymbols(revitFamily, document, currentlyLoadedFamily, tempFilePath);

                    family = currentlyLoadedFamily;
                    return true;
                }

                return false;
            }
            else
            {
                family = loadedFamily;
                // If currentLyLoadedFamily is null, the family has been loaded for the first time.
                // No further actions are necessary. Otherwise, load any missing symbols if they exist.
                if (currentlyLoadedFamily is not null)
                {
                    // The family was successfully loaded and replaced the existing one.
                    // However, it is necessary to ensure that all symbols are loaded.
                    // The variable `currentlyLoadedFamily` is not null here but is invalid because the family was replaced.
                    // Therefore, do not use it in this context.
                    LoadMissingSymbols(revitFamily, document, loadedFamily, tempFilePath);
                }
            }

            // Load the family info from the local family file.
            //if (!revitFamily.TryGetFamilyInfo<FamilyMetadata>("FamilyMetadata", out var familyMetadata))
            //{
            //    //Create a new family info and get the data from the IRevitFamily.
            //    familyMetadata = new FamilyMetadata
            //    {
            //        Version = new Version(0, 0, 0, 0),
            //        LastModified = revitFamily.Updated,
            //        ModifiedBy = GetUserName(document)
            //    };
            //}

            return family is not null;
        }
        finally
        {
            // Clean up the temporary file
            RemoveFamilyLocalFile(tempFilePath);
        }
    }

    public bool TryLoadFamilySymbol(IRevitFamilySymbol revitFamilySymbol, Document document, [NotNullWhen(true)] out FamilySymbol? familySymbol)
    {
        var revitFamily = revitFamilySymbol.Family;
        var tempFilePath = CreateFamilyLocalFile(revitFamily.Family, revitFamily.Name, document.Application);

        try
        {
            if (document.LoadFamilySymbol(tempFilePath, revitFamilySymbol.Name, new OverwriteFamilyOption(), out familySymbol))
            {
                return true;
            }

            familySymbol = null;
            return false;
        }
        finally
        {
            // Clean up the temporary file
            RemoveFamilyLocalFile(tempFilePath);
        }
    }

    public static void TemporarilyHideAllFamilyConnectors(Document doc, View view)
    {
        // Collect all connector elements in a FAMILY document
        var connectorIds = new FilteredElementCollector(doc)
                           .OfClass(typeof(ConnectorElement))
                           .WhereElementIsNotElementType()
                           .Select(e => e.Id)
                           .ToList();

        if (connectorIds.Count == 0)
        {
            return;
        }

        using var transaction = new Transaction(doc, "Temp-hide connectors");
        transaction.Start();

        // Optional: reset previous temporary hide/isolate in this view
        if (view.IsTemporaryHideIsolateActive())
        {
            view.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
        }

        // Temporary hide (same as HH / sunglasses)
        view.HideElementsTemporary(connectorIds);

        transaction.Commit();
    }

    private void LoadMissingSymbols(IRevitFamily revitFamily, Document document, Family family, string familyFilePath)
    {
        var familySymbols = family.GetFamilySymbolIds()
                                  .Select(id => document.GetElement(id) as FamilySymbol)
                                  .Where(symbol => symbol != null)
                                  .ToHashSet();

        foreach (var symbol in revitFamily.FamilySymbols)
        {
            if (familySymbols.All(existingSymbol => existingSymbol!.Name != symbol.Name))
            {
                document.LoadFamilySymbol(familyFilePath, symbol.Name, new OverwriteFamilyOption(), out var familySymbol);
            }
        }
    }

    /// <summary>
    ///     Removes a specified Revit family from the given document.
    /// </summary>
    /// <param name="revitFamily">
    ///     The <see cref="Bim.FamilyManager.Abstractions.IRevitFamily" /> instance representing the family to be
    ///     removed.
    /// </param>
    /// <param name="document">
    ///     The <see cref="Autodesk.Revit.DB.Document" /> from which the family will be removed.
    /// </param>
    /// <remarks>
    ///     This method searches for the specified family in the provided document and deletes it if found.
    ///     If the family is not found, a log entry is created to indicate this.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="revitFamily" /> or <paramref name="document" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="Autodesk.Revit.Exceptions.InvalidOperationException">
    ///     Thrown if the method is called outside of a valid Revit transaction.
    /// </exception>
    private void RemoveFamily(IRevitFamily revitFamily, Document document)
    {
        var familyName = revitFamily.Name;

        var family = new FilteredElementCollector(document)
                     .OfClass(typeof(Family))
                     .Cast<Family>()
                     .FirstOrDefault(f => f.Name.Equals(familyName));

        if (family != null)
        {
            // Delete the family
            document.Delete(family.Id);
            _logger.LogInformation($"Family '{familyName}' has been deleted.");
        }
        else
        {
            _logger.LogInformation($"Family '{familyName}' not found in the document.");
        }
    }

    /// <summary>
    ///     Reloads the family sources using the provided options and reinitializes the family management system.
    /// </summary>
    /// <param name="options">
    ///     The <see cref="FamilySourcesOptions" /> containing the updated family source
    ///     configurations.
    /// </param>
    /// <remarks>
    ///     This method updates the internal family source options based on the provided configuration,
    ///     clears the existing family cache, and restarts the family initialization queue.
    ///     It ensures that the family management system reflects the latest configuration changes.
    /// </remarks>
    private async Task Reload(FamilySourcesOptions options)
    {
        _familySourceOptions = GetAllSourcesFromOptions(options);
        await ReloadAsync();
    }

    /// <summary>
    ///     Handles the <see cref="Application.DocumentClosing" /> event.
    /// </summary>
    /// <param name="sender">
    ///     The source of the event, typically the <see cref="Application" /> instance.
    /// </param>
    /// <param name="e">
    ///     An instance of <see cref="Autodesk.Revit.DB.Events.DocumentClosingEventArgs" /> containing event data,
    ///     such as the document being closed.
    /// </param>
    /// <remarks>
    ///     This method ensures that the path of the document being closed is stored if it resides in the working directory.
    /// </remarks>
    private void OnDocumentClosing(object? sender, DocumentClosingEventArgs e)
    {
        // Handle only family documents.
        if (!e.Document.IsFamilyDocument)
        {
            return;
        }

        var pathName = e.Document.PathName;
        if (!IsFamilyInWorkingDirectory(pathName))
        {
            return;
        }

        // Store the document's path and name before it is closed
        _documentPaths[e.DocumentId] = pathName;
    }

    /// <summary>
    ///     Determines whether the specified Revit family file is located in the working directory.
    /// </summary>
    /// <param name="pathName">The full path of the Revit family file to check.</param>
    /// <returns>
    ///     <see langword="true" /> if the specified file is a Revit family file (.rfa) and is located in the working
    ///     directory;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     This method checks the file extension to ensure it is a Revit family file (.rfa) and compares its directory
    ///     with the working directory to determine if the file is located there.
    /// </remarks>
    private bool IsFamilyInWorkingDirectory(string pathName)
    {
        if (!".rfa".Equals(Path.GetExtension(pathName), StringComparison.OrdinalIgnoreCase))
        {
            // No .rfa file is being closed.
            return false;
        }

        var uri1 = new Uri(Path.GetDirectoryName(pathName)!);
        var uri2 = new Uri(GetWorkingDirectory());
        if (!uri1.Equals(uri2))
        {
            // The file is not in the working directory.
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Handles the <see cref="Autodesk.Revit.DB.Events.DocumentSavingAsEventArgs" /> event when a document is being saved
    ///     as a new file.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data containing the document being saved.</param>
    private void OnDocumentSavingAs(object? sender, DocumentSavingAsEventArgs e)
    {
        OnDocumentSaving(e.Document);
    }

    /// <summary>
    ///     Handles the <see cref="Autodesk.Revit.DB.Events.DocumentSavedAsEventArgs" /> event when a document has been saved
    ///     as a new file.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data containing the document that was saved.</param>
    private void OnDocumentSavedAs(object? sender, DocumentSavedAsEventArgs e)
    {
        OnDocumentSaved(e.Document);
    }

    /// <summary>
    ///     Handles the <see cref="Autodesk.Revit.DB.Events.DocumentSavingEventArgs" /> event when a document is being saved.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data containing the document being saved.</param>
    private void OnDocumentSaving(object? sender, DocumentSavingEventArgs e)
    {
        OnDocumentSaving(e.Document);
    }

    /// <summary>
    ///     Updates the family metadata when a document is being saved.
    /// </summary>
    /// <param name="document">The <see cref="Document" /> being saved.</param>
    private void OnDocumentSaving(Document document)
    {
        // Only handle family documents.
        if (!document.IsFamilyDocument)
        {
            return;
        }

        var familyInfo = CreateFamilyMetadata(document);

        using var transaction = new Transaction(document, "Add family info");
        transaction.Start();

        _familyMetadataEStorage.Attach(document.OwnerFamily, familyInfo);

        transaction.Commit();
    }

    private FamilyMetadata CreateFamilyMetadata(Document document)
    {
        var userName = GetUserName(document);
        if (_familyMetadataEStorage.TryGet(document.OwnerFamily, out var familyInfo))
        {
            var version = familyInfo.Version;
            familyInfo.Version = new Version(version.Major, version.Minor, version.Build + 1, version.Revision);
            familyInfo.LastModified = DateTime.UtcNow;
            familyInfo.ModifiedBy = userName;
        }
        else
        {
            familyInfo = new FamilyMetadata
            {
                Version = new Version(1, 0, 0, 0),
                LastModified = DateTime.UtcNow,
                ModifiedBy = userName
            };
        }

        return familyInfo;
    }

    /// <summary>
    ///     Handles the <see cref="ControlledApplication.DocumentSaved" /> event.
    /// </summary>
    /// <param name="sender">
    ///     The source of the event, typically the <see cref="ControlledApplication" />.
    /// </param>
    /// <param name="e">
    ///     An instance of <see cref="Autodesk.Revit.DB.Events.DocumentSavedEventArgs" /> containing event data.
    /// </param>
    /// <remarks>
    ///     This method is triggered when a document is saved in Revit. It checks if the saved document is located
    ///     in the working directory. If it is, the method attempts to retrieve the corresponding family from the
    ///     cache and saves it to its source. If the document is not in the working directory, it is assumed that
    ///     it was not opened using the family manager, and no action is taken.
    /// </remarks>
    /// <seealso cref="RevitFamilyCache.TryGetFamily" />
    private void OnDocumentSaved(object? sender, DocumentSavedEventArgs e)
    {
        OnDocumentSaved(e.Document);
    }

    private void OnDocumentSaved(Document document)
    {
        // Only handle family documents.
        if (!document.IsFamilyDocument)
        {
            return;
        }

        var pathName = GetFamilyAsStream(document, out var memoryStream);

        // Add the family info to the currently saved family file.
        UpdateFamilyInfo(document, memoryStream);
        UpdatePreviewImage(document, memoryStream);
        //TODO: Show message to ask if the user wants to save the family in original location. Alternatively add an option in the settings to control this behavior.
        var familyName = Path.GetFileNameWithoutExtension(pathName);
        if (_familyCache.TryGetFamily(familyName, out var family))
        {
            family.SaveToSource(memoryStream);
        }
    }

    private static string GetFamilyAsStream(Document document, out MemoryStream memoryStream)
    {
        var pathName = document.PathName;
        memoryStream = new MemoryStream();

        using (var fileStream = new FileStream(pathName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            fileStream.CopyTo(memoryStream);
        }

        memoryStream.Position = 0;
        return pathName;
    }

    /// <summary>
    ///     Updates the version information and metadata for a Revit family and writes it to the provided memory stream.
    /// </summary>
    /// <param name="document">
    ///     The <see cref="Autodesk.Revit.DB.Document" /> instance representing the current Revit document.
    /// </param>
    /// <param name="stream">
    ///     The <see cref="System.IO.MemoryStream" /> containing the family file data to be updated with new metadata.
    /// </param>
    /// <remarks>
    ///     This method retrieves or creates the <c>FamilyMetadata</c> metadata for the specified family, increments its
    ///     version,
    ///     updates the last modified date and user, and serializes the updated information into the family file stream.
    ///     It ensures the metadata is stored in the "BIM.FamilyManager" storage section of the family file.
    /// </remarks>
    private void UpdateFamilyInfo(Document document, MemoryStream stream)
    {
        // The family file in the working directory might not contain the latest version information.
        // When a family is saved in the family editor, it writes the version information to the file
        // that existed when the family was initially loaded.
        // Therefore, we retrieve the version information from the family currently loaded into the family manager.

        // TODO: The original family files located in the family source may have been modified,
        //  potentially resulting in an increased version number.
        //  As a result, we need to implement a conflict resolution or check-out/check-in mechanism here.

        //family.TryGetFamilyInfo<FamilyMetadata>("FamilyMetadata", out var familyInfo);
        var userName = GetUserName(document);
        if (_familyMetadataEStorage.TryGet(document.OwnerFamily, out var familyInfo))
        {
            var version = familyInfo.Version;
            familyInfo.Version = new Version(version.Major, version.Minor, version.Build + 1, version.Revision);
            familyInfo.LastModified = DateTime.UtcNow;
            familyInfo.ModifiedBy = userName;
        }
        else
        {
            familyInfo = new FamilyMetadata
            {
                Version = new Version(1, 0, 0, 0),
                LastModified = DateTime.UtcNow,
                ModifiedBy = userName
            };
        }

        using (var root = RootStorage.Open(stream, StorageModeFlags.LeaveOpen))
        {
            if (!root.TryOpenStorage("BIM.FamilyManager", out var infoStorage))
            {
                infoStorage = root.CreateStorage("BIM.FamilyManager");
            }

            // Delete current family info if it exists.
            infoStorage.Delete("FamilyMetadata");

            // Add the new family info.
            var newFamilyStream = infoStorage.CreateStream("FamilyMetadata");
            JsonSerializer.Serialize(newFamilyStream, familyInfo);
            newFamilyStream.Flush();

            root.Flush(true);
        }

        stream.Position = 0;
    }

    private void UpdatePreviewImage(Document document, MemoryStream stream)
    {
        if (_previewImageEStorage.TryGet(document.OwnerFamily, out var familyPreviewImageName, out var previewStreams))
        {
            ViewImageWriter.WritePreviewImages(stream, familyPreviewImageName, previewStreams);
        }

        //using (var root = RootStorage.Open(stream, StorageModeFlags.LeaveOpen))
        //{
        //    if (!root.TryOpenStorage("BIM.FamilyManager", out var infoStorage))
        //    {
        //        infoStorage = root.CreateStorage("BIM.FamilyManager");
        //    }

        //    // Delete current family info if it exists.
        //    infoStorage.Delete("FamilyMetadata");

        //    // Add the new family info.
        //    var newFamilyStream = infoStorage.CreateStream("FamilyMetadata");
        //    JsonSerializer.Serialize(newFamilyStream, familyInfo);
        //    newFamilyStream.Flush();

        //    root.Flush(true);
        //}

        stream.Position = 0;
    }

    /// <summary>
    ///     Retrieves the username associated with the specified Revit document.
    /// </summary>
    /// <param name="document">
    ///     The <see cref="Autodesk.Revit.DB.Document" /> instance from which to retrieve the username.
    /// </param>
    /// <returns>
    ///     A <see cref="string" /> representing the username. If the username is not available in the Revit document,
    ///     the system's current user name is returned instead.
    /// </returns>
    /// <remarks>
    ///     This method first attempts to retrieve the username from the Revit application's <c>Username</c> property.
    ///     If the username is not set or is empty, it falls back to the system's environment username.
    /// </remarks>
    private static string GetUserName(Document document)
    {
        var userName = document.Application.Username;
        if (string.IsNullOrEmpty(userName))
        {
            userName = Environment.UserName;
        }

        return userName;
    }

    /// <summary>
    ///     Handles the event triggered when a Revit document is closed.
    /// </summary>
    /// <param name="sender">
    ///     The source of the event, typically the Revit application.
    /// </param>
    /// <param name="e">
    ///     The <see cref="Autodesk.Revit.DB.Events.DocumentClosedEventArgs" /> containing event data, including the ID of the
    ///     closed document.
    /// </param>
    /// <remarks>
    ///     This method performs cleanup operations for the closed document, such as removing its path from the internal
    ///     dictionary and attempting to delete the associated file. Any failure to delete the file is logged as a debug
    ///     message.
    /// </remarks>
    private void OnDocumentClosed(object? sender, DocumentClosedEventArgs e)
    {
        var documentId = e.DocumentId;
        if (_documentPaths.TryGetValue(documentId, out var documentPath))
        {
            _logger.LogDebug($"Document closed: {documentPath}");
            _documentPaths.Remove(documentId);
            try
            {
                File.Delete(documentPath);
            }
            catch (Exception)
            {
                // This is not critical.
                _logger.LogDebug($"Could not delete file: {documentPath}");
            }
        }
    }

    /// <summary>
    ///     Retrieves the working directory for managing Revit families.
    /// </summary>
    /// <remarks>
    ///     The working directory is determined based on the configuration provided in
    ///     <see cref="FamilyManagerOptions.WorkingDirectory" />.
    ///     If the directory does not exist, it will be created automatically.
    /// </remarks>
    /// <returns>
    ///     A <see cref="string" /> representing the absolute path of the working directory.
    /// </returns>
    private string GetWorkingDirectory()
    {
        var workingDirectory = _defaultOptions.Value.WorkingDirectory.ExpandVariables();
        if (!Directory.Exists(workingDirectory))
        {
            Directory.CreateDirectory(workingDirectory);
        }

        return workingDirectory;
    }

    /// <summary>
    ///     Compares two lists of <see cref="IFamilySourceOptions" />
    ///     to determine if there are any changes between the old and new options.
    /// </summary>
    /// <param name="oldOptions">
    ///     The list of existing <see cref="Abstractions.Options.IFamilySourceOptions" />.
    /// </param>
    /// <param name="newOptions">
    ///     The list of updated <see cref="Abstractions.Options.IFamilySourceOptions" />.
    /// </param>
    /// <returns>
    ///     <c>true</c> if there are changes between the two lists; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method performs a deep comparison of the properties of each
    ///     <see cref="Abstractions.Options.IFamilySourceOptions" />
    ///     instance in the lists. If the number of options, their types, or any of their properties differ, the method
    ///     considers the lists to have changed.
    /// </remarks>
    private bool CheckForChanges(List<IFamilySourceOptions> oldOptions, List<IFamilySourceOptions> newOptions)
    {
        if (oldOptions.Count != newOptions.Count)
        {
            return true;
        }

        var orderedOldOptions = oldOptions.OrderBy(o => o.Id).ToArray();
        var orderedNewOptions = newOptions.OrderBy(o => o.Id).ToArray();

        for (var i = 0; i < orderedOldOptions.Length; i++)
        {
            var oldOption = orderedOldOptions[i];
            var newOption = orderedNewOptions[i];

            if (oldOption.GetType() != newOption.GetType())
            {
                return false;
            }

            // Use reflection to compare all properties
            var properties = oldOption.GetType().GetProperties();
            try
            {
                foreach (var property in properties)
                {
                    var existingValue = property.GetValue(oldOption);
                    var newValue = property.GetValue(newOption);

                    if (!Equals(existingValue, newValue))
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // Any exception shall be interpreted as a change.
                return false;
            }
        }

        return false;
    }

    /// <summary>
    ///     Retrieves a list of active family source options from the provided <see cref="FamilySourcesOptions" />.
    /// </summary>
    /// <param name="options">
    ///     The <see cref="FamilySourcesOptions" /> instance containing the source options to be filtered and processed.
    /// </param>
    /// <returns>
    ///     A list of <see cref="IFamilySourceOptions" /> that are active, ordered by their names.
    /// </returns>
    /// <remarks>
    ///     This method filters the sources in the provided <see cref="FamilySourcesOptions" /> to include only those
    ///     marked as active (<see cref="IFamilySourceOptions.IsActive" /> is <c>true</c>) and sorts them by their names.
    /// </remarks>
    private List<IFamilySourceOptions> GetAllSourcesFromOptions(FamilySourcesOptions options)
    {
        return options.Sources
                      .Where(o => o.IsActive)
                      .OrderBy(o => o.Name)
                      .ToList();
    }

    /// <summary>
    ///     Creates a new Revit family file for the specified
    ///     <see cref="Bim.FamilyManager.Abstractions.IRevitFamily" />.
    /// </summary>
    /// <param name="family">
    ///     The <see cref="Bim.FamilyManager.Abstractions.IRevitFamily" /> instance representing the Revit family
    ///     for which the file will be created.
    /// </param>
    /// <returns>
    ///     The full file path of the created Revit family file.
    /// </returns>
    /// <remarks>
    ///     This method generates a file for the provided Revit family in the working directory.
    ///     If an error occurs during the file creation process, appropriate logging is performed.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the <paramref name="family" /> parameter is <c>null</c>.
    /// </exception>
    /// <exception cref="System.IO.IOException">
    ///     Thrown if an I/O error occurs while creating the family file.
    /// </exception>
    /// <exception cref="System.Exception">
    ///     Thrown if an unexpected error occurs during the file creation process.
    /// </exception>
    private string CreateFamilyFile(IRevitFamily family)
    {
        var familyFileName = GetWorkingFilePath(family);

        try
        {
            using var fileStream = File.OpenWrite(familyFileName);
            family.Family.CopyTo(fileStream);
        }
        catch (IOException e)
        {
            _logger.LogWarning(e, $"Could not create family file. Path: {familyFileName}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error creating family file. Path: {familyFileName}");
            //TODO: Show error message to the user.
        }

        return familyFileName;
    }

    /// <summary>
    ///     Retrieves the full file path for the working copy of the specified Revit family.
    /// </summary>
    /// <param name="family">
    ///     The <see cref="Bim.FamilyManager.Abstractions.IRevitFamily" /> instance representing the Revit family
    ///     for which the working file path is to be retrieved.
    /// </param>
    /// <returns>
    ///     A <see cref="string" /> representing the full file path of the working copy of the specified Revit family.
    /// </returns>
    /// <remarks>
    ///     The working file path is determined based on the configured working directory and the name of the Revit family.
    ///     The file path is constructed by combining the working directory path with the family name, appending the ".rfa"
    ///     file extension.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the <paramref name="family" /> parameter is <c>null</c>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the working directory cannot be determined or does not exist.
    /// </exception>
    private string GetWorkingFilePath(IRevitFamily family)
    {
        var workingDirectory = GetWorkingDirectory();

        var familyFileName = Path.Combine(workingDirectory, $"{family.Name}.rfa");
        return familyFileName;
    }

    /// <summary>
    ///     Handles the <see cref="Autodesk.Revit.DB.Events.DocumentChangedEventArgs" /> event triggered when changes occur in
    ///     a Revit document.
    /// </summary>
    /// <param name="sender">
    ///     The source of the event, typically the <see cref="Autodesk.Revit.ApplicationServices.ControlledApplication" />.
    /// </param>
    /// <param name="e">
    ///     An instance of <see cref="Autodesk.Revit.DB.Events.DocumentChangedEventArgs" /> containing details about the
    ///     changes in the document.
    /// </param>
    /// <remarks>
    ///     This method processes added and deleted elements in the document, specifically focusing on Revit families.
    ///     It updates the internal cache and tracks the loaded state of families within the document.
    /// </remarks>
    private void OnDocumentChanged(object? sender, DocumentChangedEventArgs e)
    {
        // Get the document where the change occurred
        var document = e.GetDocument();

        // Get added and deleted element IDs
        var addedElementIds = e.GetAddedElementIds();
        var deletedElementIds = e.GetDeletedElementIds();

        // Check for added families
        var addedFamilies = addedElementIds.Select(id => document.GetElement(id))
                                           .OfType<Family>();

        foreach (var addedFamily in addedFamilies)
        {
            _familyElementIds.Add(addedFamily.Id, addedFamily.Name);
            _loadedFamilies.Add(addedFamily.Name);
            if (_familyCache.TryGetFamily(addedFamily.Name, out var family))
            {
                family.IsLoadedInDocument = true;
            }
        }

        // Check for removed families
        foreach (var deletedId in deletedElementIds)
        {
            if (_familyElementIds.Remove(deletedId, out var familyName))
            {
                _loadedFamilies.Remove(familyName);
                if (_familyCache.TryGetFamily(familyName, out var family))
                {
                    family.IsLoadedInDocument = false;
                }
            }
        }
    }

    /// <summary>
    ///     Asynchronously retrieves all Revit families from the leaf folders of the specified root folder.
    /// </summary>
    /// <param name="rootFolder">
    ///     The root folder from which to retrieve Revit families. This folder may contain subfolders
    ///     and families.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     An asynchronous stream of <see cref="IRevitFamily" /> objects representing all families
    ///     found in the leaf folders of the specified root folder.
    /// </returns>
    /// <remarks>
    ///     This method traverses the folder hierarchy starting from the specified root folder
    ///     and collects families from all leaf folders. The traversal is performed asynchronously
    ///     to improve performance.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the <paramref name="rootFolder" /> is <c>null</c>.
    /// </exception>
    private static async IAsyncEnumerable<IRevitFamily> GetAllFamiliesFromLeafFoldersAsync(IFolder rootFolder,
                                                                                           [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var leafFolders = GetLeafFoldersAsync(rootFolder, cancellationToken);
        await foreach (var folder in leafFolders)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await foreach (var family in folder.GetFamiliesAsync(true, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return family;
            }
        }
    }

    /// <summary>
    ///     Recursively retrieves all leaf folders from the specified folder hierarchy.
    /// </summary>
    /// <param name="folder">
    ///     The root folder from which to retrieve leaf folders. A leaf folder is defined as a folder
    ///     that contains no subfolders.
    /// </param>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken" /> to observe while waiting for the asynchronous operation to complete.
    /// </param>
    /// <returns>
    ///     An <see cref="IAsyncEnumerable{T}" /> of <see cref="IFolder" /> representing all leaf folders
    ///     within the specified folder hierarchy.
    /// </returns>
    /// <remarks>
    ///     This method performs a recursive traversal of the folder hierarchy. If the folder has no subfolders,
    ///     it is considered a leaf folder and is included in the result. Otherwise, the method continues to traverse
    ///     its subfolders to find leaf folders.
    /// </remarks>
    private static async IAsyncEnumerable<IFolder> GetLeafFoldersAsync(IFolder folder, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var subfolder in folder.GetSubfoldersAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await foreach (var leaf in GetLeafFoldersAsync(subfolder, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return leaf;
            }
            // Found at least one subfolder, so this is not a leaf folder
        }

        // No subfolders found, so this is a leaf folder
        yield return folder;
    }

    /// <summary>
    ///     and updating their loaded state based on the current document.
    /// </summary>
    /// <param name="families">
    ///     A collection of <see cref="Bim.FamilyManager.Abstractions.IRevitFamily" /> instances
    ///     to be initialized.
    /// </param>
    /// <remarks>
    ///     This method processes the provided families in parallel to improve performance.
    ///     It ensures that each family is initialized and updates its
    ///     <see cref="Bim.FamilyManager.Abstractions.IRevitFamily.IsLoadedInDocument" />
    ///     property based on whether it is currently loaded in the active document.
    /// </remarks>
    /// <exception cref="System.OperationCanceledException">
    ///     Thrown if the operation is canceled during the initialization process.
    /// </exception>
    /// <exception cref="System.Exception">
    ///     Logs any unexpected errors encountered during the initialization process.
    /// </exception>
    private void InitializeFamilies(IEnumerable<IRevitFamily> families)
    {
        try
        {
            var options = new ParallelOptions
            {
                // Optional: Limit the number of concurrent threads. Since we have I/O-bound work, we can use a higher value.
                MaxDegreeOfParallelism = 4, //Environment.ProcessorCount * 2,
                CancellationToken = _tokenSource.Token
            };
            Parallel.ForEach(families, options, family =>
            {
                if (!options.CancellationToken.IsCancellationRequested)
                {
                    family.Initialize();
                    family.IsLoadedInDocument = _loadedFamilies.Contains(family.Name);
                }
            });
        }
        catch (OperationCanceledException)
        {
            // Nothing to do here.
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error initializing families.");
        }
    }

    /// <summary>
    ///     Handles the <see cref="UIControlledApplication.ViewActivating" /> event.
    /// </summary>
    /// <param name="sender">
    ///     The source of the event, typically the <see cref="UIControlledApplication" /> instance.
    /// </param>
    /// <param name="e">
    ///     An instance of <see cref="Autodesk.Revit.UI.Events.ViewActivatingEventArgs" /> containing event data,
    ///     such as the new active view and its associated document.
    /// </param>
    /// <remarks>
    ///     This method updates the active document reference and refreshes the family cache to reflect the
    ///     families loaded in the newly activated document. It ensures that the family cache remains consistent
    ///     with the current state of the active document.
    /// </remarks>
    private void OnViewActivating(object? sender, ViewActivatingEventArgs e)
    {
        // Get the currently active document
        var currentActiveDocument = e.NewActiveView.Document;
        // Check if the active document has changed
        if (!ReferenceEquals(_activeDocument, currentActiveDocument))
        {
            _activeDocument = currentActiveDocument;
            if (_activeDocument is not null)
            {
                _familyElementIds = GetAllFamiliesFromDocument(_activeDocument)
                    .ToDictionary(family => family.Id, family => family.Name);

                _loadedFamilies = _familyElementIds.Values.ToHashSet();

                foreach (var family in _familyCache.Families)
                {
                    family.IsLoadedInDocument = _loadedFamilies.Contains(family.Name);
                }
            }
        }
    }

    /// <summary>
    ///     Retrieves all family elements from the specified Revit document.
    /// </summary>
    /// <param name="document">
    ///     The <see cref="Autodesk.Revit.DB.Document" /> instance representing the Revit document
    ///     from which to retrieve the family elements.
    /// </param>
    /// <returns>
    ///     A list of <see cref="Autodesk.Revit.DB.Family" /> objects representing all the families
    ///     present in the specified document.
    /// </returns>
    /// <remarks>
    ///     This method uses a <see cref="Autodesk.Revit.DB.FilteredElementCollector" /> to collect
    ///     all elements of type <see cref="Autodesk.Revit.DB.Family" /> from the provided document.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the <paramref name="document" /> parameter is <c>null</c>.
    /// </exception>
    private static List<Family> GetAllFamiliesFromDocument(Document document)
    {
        // Create a filtered element collector to collect all Family elements in the document
        var collector = new FilteredElementCollector(document);
        var families = collector.OfClass(typeof(Family)).ToElements()
                                .OfType<Family>()
                                .ToList();

        return families;
    }

    /// <summary>
    ///     Retrieves a collection of family sources based on the configured options.
    /// </summary>
    /// <returns>
    ///     An <see cref="IEnumerable{T}" /> of <see cref="Bim.FamilyManager.Abstractions.IFamilySource" />
    ///     representing the available family sources.
    /// </returns>
    /// <remarks>
    ///     This method iterates through the configured family source options, resolves the corresponding
    ///     factory delegates, and invokes them to create instances of
    ///     <see cref="Bim.FamilyManager.Abstractions.IFamilySource" />.
    ///     If a factory delegate cannot be resolved or does not return a valid family source, an error is logged.
    /// </remarks>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the factory delegate for a family source type is not properly registered or does not return
    ///     a valid <see cref="Bim.FamilyManager.Abstractions.IFamilySource" />.
    /// </exception>
    private IEnumerable<IFamilySource> GetFamilySources()
    {
        foreach (var sourceOptions in _familySourceOptions)
        {
            var sourceType = _services.GetKeyedService<Type>(sourceOptions.Type);
            if (sourceType is null)
            {
                _logger.LogError($"Family source '{sourceOptions.Type}' is not registered.");
                continue;
            }

            var factoryDelegateType = sourceType.GetNestedType("Factory", BindingFlags.Public | BindingFlags.NonPublic);
            if (factoryDelegateType == null)
            {
                _logger.LogError($"Could not find 'Factory' delegate in type '{sourceType.FullName}'.");
                continue;
            }

            var factory = _services.GetService(factoryDelegateType) as Delegate;
            if (factory is null)
            {
                _logger.LogError($"Could not resolve factory delegate for type '{sourceType.FullName}'.");
                continue;
            }

            // Call the delegate
            if (factory.DynamicInvoke(sourceOptions) is not IFamilySource source)
            {
                _logger.LogError($"The 'Factory' delegate in type '{sourceType.FullName}' did not return a valid IFamilySource.");
                continue;
            }

            yield return source;
        }
    }

    /// <summary>
    ///     Creates a local copy of a Revit family file from the provided stream and optimizes it by removing unused assets.
    /// </summary>
    /// <param name="familyFile">
    ///     A <see cref="Stream" /> containing the data of the Revit family file to be processed.
    /// </param>
    /// <param name="name">
    ///     The name of the family file, which will be used to generate the local file name with a ".rfa" extension.
    /// </param>
    /// <param name="application">
    ///     An instance of <see cref="Autodesk.Revit.ApplicationServices.Application" /> for managing Revit-specific
    ///     operations.
    /// </param>
    /// <returns>
    ///     The full path to the locally created and optimized family file.
    /// </returns>
    /// <remarks>
    ///     This method ensures that the family file is optimized by removing unused assets, which can improve performance
    ///     and reduce file size. The resulting file is stored in the system's temporary directory.
    /// </remarks>
    /// <exception cref="System.IO.IOException">
    ///     Thrown if an error occurs during the creation or writing of the local file.
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if any of the parameters <paramref name="familyFile" />, <paramref name="name" />, or
    ///     <paramref name="application" /> is <c>null</c>.
    /// </exception>
    private string CreateFamilyLocalFile(Stream familyFile, string name, Application application)
    {
        var tempFileName = Path.Combine(Path.GetTempPath(), name + ".rfa");

        using (var tempFile = new FileStream(tempFileName, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            familyFile.CopyTo(tempFile);
        }

        // Removing unused assets changes to version of the family. Thus the family will always be treated as modified and a appropriate dialog will be shown to the user.
        // However, this is unwanted behavior while using the family manager to add families or symbols to a project.
        // To avoid this, it is recommended to only use families already updated to the Revit 2025 format.
        // TODO: Probably add an option to control this behavior or create a family update command.
        //RemoveUnusedAssets(tempFileName, application);

        return tempFileName;
    }

    /// <summary>
    ///     Deletes the specified local file if it exists.
    /// </summary>
    /// <param name="localFileName">
    ///     The full path of the local file to be deleted.
    /// </param>
    /// <remarks>
    ///     This method checks for the existence of the specified file and deletes it if found.
    ///     It is typically used for cleaning up temporary files created during operations.
    /// </remarks>
    /// <exception cref="System.IO.IOException">
    ///     Thrown if an I/O error occurs while attempting to delete the file.
    /// </exception>
    /// <exception cref="System.UnauthorizedAccessException">
    ///     Thrown if the caller does not have the required permission to delete the file.
    /// </exception>
    private static void RemoveFamilyLocalFile(string localFileName)
    {
        if (File.Exists(localFileName))
        {
            File.Delete(localFileName);
        }
    }

    /// <summary>
    ///     Searches for a Revit family in the specified document that matches the given family name.
    /// </summary>
    /// <param name="document">
    ///     The Revit <see cref="Autodesk.Revit.DB.Document" /> in which to search for the family.
    /// </param>
    /// <param name="revitFamily">
    ///     An instance of <see cref="Bim.FamilyManager.Abstractions.IRevitFamily" /> representing the family to find.
    /// </param>
    /// <returns>
    ///     The <see cref="Autodesk.Revit.DB.Family" /> object if a matching family is found; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    ///     This method uses a <see cref="Autodesk.Revit.DB.FilteredElementCollector" /> to filter and search for family
    ///     elements
    ///     in the provided document. The search is case-insensitive and matches the family name.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="document" /> or <paramref name="revitFamily" /> is <c>null</c>.
    /// </exception>
    private static Family? FindFamily(Document document, IRevitFamily revitFamily)
    {
        // Filter for Family elements in the document
        var collector = new FilteredElementCollector(document);
        collector.OfClass(typeof(Family));

        return collector.OfType<Family>()
                        .FirstOrDefault(family => family.Name.Equals(revitFamily.Name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     Removes unused assets from the specified Revit family file.
    /// </summary>
    /// <param name="familyFileName">
    ///     The full path to the Revit family file from which unused assets should be removed.
    /// </param>
    /// <param name="application">
    ///     The Revit application instance used to open and process the family file.
    /// </param>
    /// <remarks>
    ///     This method identifies and deletes all unused elements within the specified family file.
    ///     The cleanup process is performed iteratively to ensure that any elements that become
    ///     unused as a result of other deletions are also removed. If an error occurs during the
    ///     process, the transaction is rolled back, and the family file is closed without saving changes.
    ///     When updating families to Revit 2025 or newer, the update process sometimes produces
    ///     up to thousands of unused elements. This can dramatically reduce the performance when
    ///     such a family is added to a project. This method ensures that these unused elements
    ///     are removed, improving performance and reducing file size.
    /// </remarks>
    /// <exception cref="Autodesk.Revit.Exceptions.InvalidOperationException">
    ///     Thrown if the Revit document cannot be opened or processed.
    /// </exception>
    /// <exception cref="System.IO.IOException">
    ///     Thrown if there is an issue accessing the specified family file.
    /// </exception>
    /// <exception cref="System.Exception">
    ///     Thrown if an unexpected error occurs during the cleanup process.
    /// </exception>
    private void RemoveUnusedAssets(string familyFileName, Application application)
    {
        _logger.LogInformation($"Delete unused assets for family '{Path.GetFileName(familyFileName)}'.");

        var document = application.OpenDocumentFile(familyFileName);

        // TODO: Get the revit version from the family (e.g. 2025) and only do this newer.
        // If a family in Revit 2025 format is loaded into a project with Revit 2025 we don't need to remove unused assets.
        using var transaction = new Transaction(document, "Delete unused assets");
        transaction.Start();

        try
        {
            ISet<ElementId> unusedElementIds;

            // Retrieve all unused elements from the document and delete them.
            // Elements marked for deletion may reference other objects that, in turn, become unused
            // after the referencing element is removed. Therefore, the cleanup process must be
            // executed in a loop until no more unused elements are found.
            do
            {
                unusedElementIds = document.GetUnusedElements(new HashSet<ElementId>());
                document.Delete(unusedElementIds);
            } while (unusedElementIds.Count > 0);

            transaction.Commit();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Deletion of unused elements failed for family '{Path.GetFileName(familyFileName)}'.");
            transaction.RollBack();
            document.Close(false);
        }

        document.Close(true);
    }

    /// <summary>
    ///     Provides options for handling family loading operations in Revit, including
    ///     determining whether to overwrite existing families and their parameter values.
    /// </summary>
    /// <remarks>
    ///     This class implements the <see cref="Autodesk.Revit.DB.IFamilyLoadOptions" /> interface
    ///     and is used internally by the <see cref="Bim.FamilyManager.Base.Logic.FamilyManager" />
    ///     to manage family loading behavior, such as overwriting existing families or shared families.
    /// </remarks>
    private class OverwriteFamilyOption : IFamilyLoadOptions
    {
        public bool IsCancelled { get; private set; }

        /// <summary>
        ///     Handles the event when a family is found during the loading process in Revit.
        /// </summary>
        /// <param name="familyInUse">
        ///     A boolean value indicating whether the family is currently in use within the Revit document.
        /// </param>
        /// <param name="overwriteParameterValues">
        ///     An output parameter that specifies whether the parameter values of the existing family should be overwritten.
        /// </param>
        /// <returns>
        ///     A boolean value indicating whether the family should be overwritten.
        ///     Returns <c>true</c> to overwrite the family, or <c>false</c> to retain the existing family.
        /// </returns>
        /// <remarks>
        ///     This method is part of the <see cref="Autodesk.Revit.DB.IFamilyLoadOptions" /> implementation
        ///     and is used to customize the behavior of family loading operations, such as deciding whether
        ///     to overwrite existing families and their parameter values.
        /// </remarks>
        public bool OnFamilyFound(bool familyInUse, /*[UnscopedRef]*/ out bool overwriteParameterValues)
        {
            //TODO: Add strings to resource files.
            var dialog = new TaskDialog("Family Already Exists")
            {
                MainInstruction = "The family already exists in the project.",
                MainContent = familyInUse
                    ? "This family is currently in use. Choose how you want to proceed."
                    : "Choose how you want to proceed.",
                AllowCancellation = true
            };

            dialog.AddCommandLink(
                TaskDialogCommandLinkId.CommandLink1,
                "Overwrite the existing version",
                "Reloads the family definition (geometry), but keeps type parameter values already set in the project."
            );

            dialog.AddCommandLink(
                TaskDialogCommandLinkId.CommandLink2,
                "Overwrite the existing version and its parameter values",
                "Reloads the family definition and overwrites the type parameter values in the project with values from the loaded family."
            );

            dialog.CommonButtons = TaskDialogCommonButtons.Cancel;
            dialog.DefaultButton = TaskDialogResult.CommandLink1;

            var result = dialog.Show();

            switch (result)
            {
                case TaskDialogResult.CommandLink1:
                {
                    overwriteParameterValues = false;
                    return true;
                }
                    ;

                case TaskDialogResult.CommandLink2:
                {
                    overwriteParameterValues = true;
                    return true;
                }

                default:
                {
                    overwriteParameterValues = false;
                    IsCancelled = true;
                    return false; // Cancel
                }
            }
        }

        /// <summary>
        ///     Handles the event when a shared family is found during the loading process in Revit.
        /// </summary>
        /// <param name="sharedFamily">
        ///     The <see cref="Autodesk.Revit.DB.Family" /> object representing the shared family found in the Revit document.
        /// </param>
        /// <param name="familyInUse">
        ///     A boolean value indicating whether the shared family is currently in use within the Revit document.
        /// </param>
        /// <param name="source">
        ///     An output parameter that specifies the source of the shared family.
        ///     This is typically set to a value from the <see cref="FamilySource" /> enumeration.
        /// </param>
        /// <param name="overwriteParameterValues">
        ///     An output parameter that specifies whether the parameter values of the existing shared family should be
        ///     overwritten.
        /// </param>
        /// <returns>
        ///     A boolean value indicating whether the shared family should be overwritten.
        ///     Returns <c>true</c> to overwrite the shared family, or <c>false</c> to retain the existing shared family.
        /// </returns>
        /// <remarks>
        ///     This method is part of the <see cref="Autodesk.Revit.DB.IFamilyLoadOptions" /> implementation
        ///     and is used to customize the behavior of shared family loading operations, such as deciding whether
        ///     to overwrite existing shared families, their source, and their parameter values.
        /// </remarks>
        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, /*[UnscopedRef]*/ out FamilySource source,
                                        /*[UnscopedRef]*/ out bool overwriteParameterValues)
        {
            //TODO: Add strings to resource files.

            var familyName = sharedFamily.Name;

            var dialog = new TaskDialog("Shared Family Already Exists")
            {
                MainInstruction = $"A shared family already exists in the project: {familyName}",
                MainContent = familyInUse
                    ? "This shared family is currently in use. Choose which version to keep."
                    : "Choose which version to keep.",
                AllowCancellation = true
            };

            dialog.AddCommandLink(
                TaskDialogCommandLinkId.CommandLink1,
                "Use the family from the family manager",
                "Reloads the shared family from the family manager source, but keeps the current type parameter values in the project."
            );

            dialog.AddCommandLink(
                TaskDialogCommandLinkId.CommandLink2,
                "Use the family from the family manager and overwrite parameter values",
                "Reloads the shared family from the family manager source and overwrites the type parameter values in the project with values from the reloaded family"
            );

            dialog.AddCommandLink(
                TaskDialogCommandLinkId.CommandLink3,
                "Use the family from the project",
                "Keeps the existing shared family already loaded in the project."
            );

            dialog.CommonButtons = TaskDialogCommonButtons.Cancel;
            dialog.DefaultButton = TaskDialogResult.CommandLink1;

            var result = dialog.Show();

            switch (result)
            {
                case TaskDialogResult.CommandLink1:
                    source = FamilySource.Family; // take from file
                    overwriteParameterValues = false; // keep existing param values
                    return true;

                case TaskDialogResult.CommandLink2:
                    source = FamilySource.Family; // take from file
                    overwriteParameterValues = true; // overwrite param values
                    return true;

                case TaskDialogResult.CommandLink3:
                    source = FamilySource.Project; // keep project version
                    overwriteParameterValues = false; // irrelevant here, but set safely
                    return true;

                default:
                    source = FamilySource.Project;
                    overwriteParameterValues = false;
                    return false; // Cancel
            }
        }
    }
}
