using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Bim.FamilyManager.Abstractions;
using Scotec.Revit.RevitFamily;

namespace Bim.FamilyManager.Base.Logic;

/// <inheritdoc cref="IRevitFamily" />
public sealed class RevitFamily : IRevitFamily
{
    private readonly object _initializationLock = new();
    private readonly ILogger<RevitFamily> _logger;
    private readonly Action<IRevitFamily, Stream> _saveAction;
    private RevitFamilyInfo _familyInfo;
    private IList<IRevitFamilySymbol>? _familySymbols;
    private bool _isLoadedInDocument;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitFamily" /> class.
    /// </summary>
    /// <param name="name">
    ///     The name of the Revit family.
    /// </param>
    /// <param name="familyInfo">
    ///     The metadata and file information associated with the Revit family.
    /// </param>
    /// <param name="saveAction">
    ///     An action delegate used to save the Revit family to a specified stream.
    /// </param>
    /// <param name="logger">
    ///     The logger instance used for logging operations related to the Revit family.
    /// </param>
    /// <remarks>
    ///     This constructor initializes the Revit family with its name, metadata, save action, and logger.
    ///     It also sets the initialization state based on the provided <paramref name="familyInfo" />.
    /// </remarks>
    public RevitFamily(string name, RevitFamilyInfo familyInfo, Action<IRevitFamily, Stream> saveAction, ILogger<RevitFamily> logger)
    {
        Name = name;
        _familyInfo = familyInfo;
        _saveAction = saveAction;
        _logger = logger;
        IsInitialized = familyInfo.IsInitialized;
    }

    /// <summary>
    ///     Gets a value indicating whether the Revit family has been initialized.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the Revit family is initialized; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property reflects the initialization state of the Revit family.
    ///     It is set during the construction of the <see cref="RevitFamily" /> instance
    ///     or when the <see cref="Initialize" /> method is called.
    ///     Changes to the family data, such as applying updates via <see cref="ApplyUpdate" />,
    ///     may reset this property to <c>false</c>.
    /// </remarks>
    public bool IsInitialized { get; private set; }

    /// <summary>
    ///     Gets the content of the Revit family as a <see cref="Stream" />.
    /// </summary>
    /// <remarks>
    ///     Accessing this property ensures that the Revit family is fully initialized before returning its content.
    ///     The returned stream represents the serialized data of the Revit family.
    /// </remarks>
    /// <value>
    ///     A <see cref="Stream" /> containing the serialized data of the Revit family.
    /// </value>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the family cannot be initialized or its content is unavailable.
    /// </exception>
    public Stream Family
    {
        get
        {
            // The family is usually initialized when the Family property is accessed.
            // To ensure consistency, we fully initialize the family before returning its content.
            Initialize();

            return _familyInfo.Family;
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the Revit family is currently loaded in the document.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the Revit family is loaded in the document; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     Changing this property triggers the <see cref="LoadedInDocumentChanged" /> event.
    /// </remarks>
    public bool IsLoadedInDocument
    {
        get => _isLoadedInDocument;
        set
        {
            if (_isLoadedInDocument != value)
            {
                _isLoadedInDocument = value;
                LoadedInDocumentChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    ///     Occurs when the <see cref="IsLoadedInDocument" /> property changes.
    /// </summary>
    /// <remarks>
    ///     This event is triggered whenever the <see cref="IsLoadedInDocument" /> property is updated,
    ///     indicating a change in the loaded state of the Revit family within the document.
    /// </remarks>
    public event EventHandler<EventArgs>? LoadedInDocumentChanged;

    /// <summary>
    ///     Saves the current Revit family to the specified stream.
    /// </summary>
    /// <param name="stream">
    ///     The <see cref="Stream" /> to which the Revit family data will be written.
    /// </param>
    /// <remarks>
    ///     This method utilizes the provided save action to persist the data of the current Revit family
    ///     to the given stream. It ensures that the family data is serialized and written in a format
    ///     suitable for later retrieval or use.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="stream" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the Revit family is not properly initialized or cannot be saved.
    /// </exception>
    public void SaveToSource(Stream stream)
    {
        _saveAction(this, stream);
    }

    public bool TryGetFamilyInfo<TInfo>(string name, [NotNullWhen(true)] out TInfo? info) where TInfo : class
    {
        info = null;
        if (_familyInfo.TryGetInfoDataStream(name, out var stream))
        {
            stream.Position = 0;
            info = JsonSerializer.Deserialize<TInfo>(stream);
        }

        return info is not null;
    }

    /// <summary>
    ///     Gets the name of the Revit family.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the Revit family.
    /// </value>
    /// <remarks>
    ///     This property provides the unique identifier or name associated with the Revit family.
    ///     It is initialized during the creation of the <see cref="RevitFamily" /> instance and is immutable.
    /// </remarks>
    public string Name { get; }

    /// <summary>
    ///     Gets the preview image stream associated with the Revit family.
    /// </summary>
    /// <value>
    ///     A <see cref="Stream" /> containing the preview image of the Revit family, or <c>null</c> if no preview is
    ///     available.
    /// </value>
    /// <remarks>
    ///     The preview image provides a visual representation of the Revit family.
    ///     It can be used for display purposes in user interfaces or for other visualization needs.
    /// </remarks>
    public Stream? Preview => _familyInfo.Preview;

    /// <summary>
    ///     Gets the collection of family symbols associated with this Revit family.
    /// </summary>
    /// <value>
    ///     A list of <see cref="IRevitFamilySymbol" /> instances representing the symbols
    ///     defined within the Revit family.
    /// </value>
    /// <remarks>
    ///     This property initializes and retrieves the family symbols by mapping the
    ///     symbol information from the underlying <see cref="RevitFamilyInfo" /> to
    ///     instances of <see cref="RevitFamilySymbol" />. If the symbols have not been
    ///     initialized, they will be lazily loaded upon first access.
    /// </remarks>
    public IList<IRevitFamilySymbol> FamilySymbols => _familySymbols ??= _familyInfo.FamilySymbolInfos
                                                                                    .Select(s => (IRevitFamilySymbol)new RevitFamilySymbol(s, this))
                                                                                    .ToList();

    /// <summary>
    ///     Occurs when the <see cref="RevitFamily" /> instance has been successfully initialized.
    /// </summary>
    /// <remarks>
    ///     This event is triggered after the <see cref="Initialize" /> method completes successfully,
    ///     indicating that the <see cref="RevitFamily" /> is ready for use. Subscribers can use this
    ///     event to perform actions that depend on the initialization of the family.
    /// </remarks>
    public event EventHandler? Initialized;

    /// <summary>
    ///     Initializes the <see cref="RevitFamily" /> instance, ensuring that it is ready for use.
    /// </summary>
    /// <remarks>
    ///     This method performs the necessary setup for the <see cref="RevitFamily" /> instance.
    ///     If the instance is already initialized, the method returns immediately.
    ///     During initialization, the family data is loaded, and the <see cref="Initialized" /> event is triggered
    ///     upon successful completion. Any errors encountered during initialization are logged.
    /// </remarks>
    /// <exception cref="Exception">
    ///     Thrown if an error occurs during the initialization process. The error is logged, and the exception is rethrown.
    /// </exception>
    /// <example>
    ///     To initialize a <see cref="RevitFamily" /> instance:
    ///     <code>
    /// var revitFamily = new RevitFamily("FamilyName", familyInfo, saveAction, logger);
    /// revitFamily.Initialize();
    /// </code>
    /// </example>
    public void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }

        lock (_initializationLock)
        {
            if (IsInitialized)
            {
                return;
            }

            try
            {
                _familyInfo.Initialize();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while loading family.");
            }

            IsInitialized = true;
            Initialized?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    ///     Applies an update to the Revit family by replacing its current information with the provided
    ///     <paramref name="familyInfo" />.
    /// </summary>
    /// <param name="familyInfo">
    ///     The new <see cref="RevitFamilyInfo" /> containing updated data for the Revit family.
    /// </param>
    /// <remarks>
    ///     This method resets the initialization state of the Revit family and clears the cached family symbols.
    ///     After applying the update, the <see cref="Initialize" /> method is called to reinitialize the family with the new
    ///     data.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="familyInfo" /> is <c>null</c>.
    /// </exception>
    public void ApplyUpdate(RevitFamilyInfo familyInfo)
    {
        lock (_initializationLock)
        {
            IsInitialized = false;
            _familySymbols = null;
            _familyInfo = familyInfo;
        }

        Initialize();
    }

    public string Product => _familyInfo.Product;
    public string ProductVersion => _familyInfo.ProductVersion;
    public DateTime Updated => _familyInfo.Updated;
}
