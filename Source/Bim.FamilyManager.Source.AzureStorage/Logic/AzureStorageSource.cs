using System.IO;
using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Base.Logic;
using Bim.FamilyManager.Source.AzureStorage.Options;
using Microsoft.Extensions.Logging;
using Scotec.Events.WeakEvents;
using Scotec.Identity.AzureActiveDirectory;
using Scotec.Revit.RevitFamily;
using TokenCache = Scotec.Identity.AzureActiveDirectory.TokenCache;

namespace Bim.FamilyManager.Source.AzureStorage.Logic;

/// <summary>
/// Represents a family source backed by Azure Blob Storage, providing access to folders and Revit families.
/// Handles authentication, connection management, and blob operations.
/// </summary>
public sealed class AzureStorageSource : FamilySource<AzureStorageSourceOptions>
{
    /// <summary>
    /// Delegate for AzureStorageSource events.
    /// </summary>
    /// <param name="sender">The <see cref="AzureStorageSource"/> instance raising the event.</param>
    public delegate void AzureStorageSourceEventHandler(AzureStorageSource sender);

    /// <summary>
    /// Delegate for creating an <see cref="AzureStorageSource"/> instance.
    /// </summary>
    /// <param name="options">The options for the Azure storage source.</param>
    /// <returns>A new <see cref="AzureStorageSource"/> instance.</returns>
    public delegate AzureStorageSource Factory(AzureStorageSourceOptions options);

    private static readonly Stream PreviewStream;
    private static readonly Regex BackupRegex = new(@"\.\d{4}\.rfa$", RegexOptions.Compiled);
    private readonly IAadAuthService _authService;
    private readonly ILogger<AzureStorageSource> _logger;
    private BlobContainerClient? _blobContainerClient;
    private IEnumerable<IFolder>? _folders;
    private IAadAuthSession? _session;

    /// <summary>
    /// Static constructor. Loads the preview image resource for Azure storage source.
    /// </summary>
    static AzureStorageSource()
    {
        const string packUri =
            "pack://application:,,,/Bim.FamilyManager.Source.AzureStorage;component/Resources/Images/Azure_128x128.png";

        PreviewStream = LoadResourceAsStream(packUri);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureStorageSource"/> class.
    /// </summary>
    /// <param name="options">The Azure storage source options.</param>
    /// <param name="familyManager">The family manager instance.</param>
    /// <param name="familyFactory">The factory for creating Revit families.</param>
    /// <param name="authService">The Azure AD authentication service.</param>
    /// <param name="logger">The logger instance.</param>
    public AzureStorageSource(
        AzureStorageSourceOptions options,
        IFamilyManager familyManager,
        IRevitFamily.Factory familyFactory,
        IAadAuthService authService,
        ILogger<AzureStorageSource> logger)
        : base(options, familyManager, familyFactory)
    {
        _authService = authService;
        _logger = logger;

        _ = ConnectToAzureStorageSilentAsync(CancellationToken.None);
    }

    /// <summary>
    /// Gets the collection of folders from Azure Blob Storage.
    /// </summary>
    public override IEnumerable<IFolder> Folders => _folders ??= GetFolders(Options.RootPath.EndsWith("/") ? Options.RootPath : Options.RootPath + "/");

    /// <summary>
    /// Gets the preview image stream for the Azure storage source.
    /// </summary>
    public override Stream Preview
    {
        get
        {
            PreviewStream.Position = 0;
            return PreviewStream;
        }
    }

    /// <summary>
    /// Gets the options for the Azure storage source.
    /// </summary>
    public AzureStorageSourceOptions SourceOptions => Options;

    /// <summary>
    /// Gets or sets the current Azure AD authentication session.
    /// Handles event registration and connection management.
    /// </summary>
    private IAadAuthSession? Session
    {
        get => _session;
        set
        {
            if (_session != value)
            {
                if (_session is not null)
                {
                    StaticWeakEventManager.RemoveWeakHandler(_session, nameof(_session.SignedIn), OnSessionSignedIn);
                    StaticWeakEventManager.RemoveWeakHandler(_session, nameof(_session.SignedOut), OnSessionSignedOut);
                    Disconnect();
                }

                _session = value;

                if (_session is not null)
                {
                    StaticWeakEventManager.AddWeakHandler(_session, nameof(_session.SignedIn), OnSessionSignedIn);
                    StaticWeakEventManager.AddWeakHandler(_session, nameof(_session.SignedOut), OnSessionSignedOut);
                    if (_session.IsSignedIn)
                    {
                        Connect();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Occurs when the Azure storage source is connected.
    /// </summary>
    public event AzureStorageSourceEventHandler? Connected;

    /// <summary>
    /// Occurs when the Azure storage source is disconnected.
    /// </summary>
    public event AzureStorageSourceEventHandler? Disconnected;

    /// <summary>
    /// Connects to Azure Blob Storage using interactive authentication.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task ConnectToAzureStorageAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (Options.TenantId is null || Options.ClientId is null)
            {
                throw new ArgumentException("Tenant ID and client ID must not be null");
            }

            if (_authService.TryGetSession(Options.TenantId.Value, Options.ClientId.Value, out var session) && session.IsSignedIn)
            {
                Session = session;
            }
            else
            {
                var authOptions = CreateAuthOptions();
                Session = await _authService.RegisterAppAsync(authOptions, true, cancellationToken);
            }
        }
        catch (Exception e)
        {
            Session = null;
            RaiseError(e);
        }
    }

    /// <summary>
    /// Copies a blob within the Azure Blob Storage container.
    /// </summary>
    /// <param name="sourceBlobName">The name of the source blob.</param>
    /// <param name="destinationBlobName">The name of the destination blob.</param>
    /// <exception cref="System.InvalidOperationException">Thrown if the blob container client is not initialized.</exception>
    public void CopyBlob(string sourceBlobName, string destinationBlobName)
    {
        if (_blobContainerClient is null)
        {
            throw new InvalidOperationException("BlobContainerClient is not initialized.");
        }

        var sourceBlob = _blobContainerClient.GetBlobClient(sourceBlobName);
        var destinationBlob = _blobContainerClient.GetBlobClient(destinationBlobName);

        // Get the URI of the source blob
        var sourceUri = sourceBlob.Uri;

        // Start the copy operation
        destinationBlob.StartCopyFromUri(sourceUri);
    }

    /// <summary>
    /// Called when the source is reloaded.
    /// </summary>
    protected override void OnReload()
    {
        _folders = null;
    }

    /// <summary>
    /// Handles the SignedOut event for the Azure AD session.
    /// </summary>
    /// <param name="session">The Azure AD session.</param>
    /// <param name="args">The event arguments.</param>
    private void OnSessionSignedOut(IAadAuthSession session, EventArgs args)
    {
        try
        {
            Disconnect();
        }
        catch (Exception)
        {
            //TODO: Logging
        }
    }

    /// <summary>
    /// Disconnects from Azure Blob Storage and clears cached data.
    /// </summary>
    private void Disconnect()
    {
        _blobContainerClient = null;
        _folders = null;
        Disconnected?.Invoke(this);
        Reload();
    }

    /// <summary>
    /// Handles the SignedIn event for the Azure AD session.
    /// </summary>
    /// <param name="session">The Azure AD session.</param>
    /// <param name="args">The event arguments.</param>
    private void OnSessionSignedIn(IAadAuthSession session, EventArgs args)
    {
        try
        {
            Connect();
        }
        catch (Exception)
        {
            //TODO: Logging
        }
    }

    /// <summary>
    /// Establishes a connection to Azure Blob Storage using the current session.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the session is null.</exception>
    private void Connect()
    {
        if (Session is null)
        {
            throw new InvalidOperationException("Session must not be null.");
        }

        _blobContainerClient = new BlobContainerClient(new Uri($"{Options.Endpoint}/{Options.ContainerName}"), Session.GetTokenCredential());
        Connected?.Invoke(this);
        Reload();
    }

    /// <summary>
    /// Creates authentication options for Azure AD.
    /// </summary>
    /// <returns>An <see cref="AadAuthOptions"/> instance configured for the current source options.</returns>
    private AadAuthOptions CreateAuthOptions()
    {
        var cacheFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BIM.FamilyManager", $"msal_cache_{Options.ClientId}.bin");

        var authOptions = new AadAuthOptions
        {
            ClientId = Options.ClientId,
            TenantId = Options.TenantId,
            Scopes = ["https://storage.azure.com/.default"],
            TokenCache = new TokenCache(cacheFile)
        };
        return authOptions;
    }

    /// <summary>
    /// Attempts to connect to Azure Blob Storage silently using cached credentials.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    private async Task ConnectToAzureStorageSilentAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (Options.TenantId is null || Options.ClientId is null)
            {
                throw new ArgumentException("Tenant ID and client ID must not be null");
            }

            if (_authService.TryGetSession(Options.TenantId.Value, Options.ClientId.Value, out var session) && session.IsSignedIn)
            {
                Session = session;
            }
            else
            {
                var authOptions = CreateAuthOptions();
                Session = await _authService.SignInSilentAsync(authOptions, cancellationToken);
            }
        }
        catch (Exception e)
        {
            Session = null;
            RaiseError(e);
        }
    }

    /// <summary>
    /// Retrieves folders from Azure Blob Storage under the specified prefix.
    /// </summary>
    /// <param name="prefix">The blob prefix to search for folders.</param>
    /// <returns>A collection of <see cref="IFolder"/> instances representing folders.</returns>
    private IEnumerable<IFolder> GetFolders(string prefix)
    {
        if (_blobContainerClient is null)
        {
            return [];
        }

        var result = new List<IFolder>();
        try
        {
            foreach (var item in _blobContainerClient.GetBlobsByHierarchy(prefix: prefix, delimiter: "/"))
            {
                if (item.IsPrefix)
                {
                    var itemPrefix = item.Prefix;
                    result.Add(new Folder(
                        Path.GetFileName(itemPrefix.Trim('/')),
                        () => GetFolders(itemPrefix), // Recursively get subfolders
                        () => GetFamilies(itemPrefix)
                    ));
                }
            }
        }
        catch (Exception)
        {
            result.Clear();
        }

        return result;
    }

    /// <summary>
    /// Retrieves Revit families from Azure Blob Storage under the specified prefix.
    /// </summary>
    /// <param name="prefix">The blob prefix to search for family files.</param>
    /// <returns>A list of <see cref="IRevitFamily"/> instances.</returns>
    private List<IRevitFamily> GetFamilies(string prefix)
    {
        if (_blobContainerClient is null)
        {
            return [];
        }

        var families = new List<IRevitFamily>();
        var blobs = _blobContainerClient.GetBlobs(prefix: prefix)
                                        .Where(b => !BackupRegex.IsMatch(b.Name) && b.Name.EndsWith(".rfa", StringComparison.OrdinalIgnoreCase))
                                        .ToList();

        foreach (var blobItem in blobs)
        {
            var familyName = Path.GetFileNameWithoutExtension(blobItem.Name);
            if (FamilyManager.TryGetRevitFamily(familyName, out var family))
            {
                families.Add(family);
            }
            else
            {
                families.Add(CreateRevitFamily(
                    familyName,
                    CreateFamilyInfo(blobItem.Name),
                    (revitFamily, stream) => SaveFamily(revitFamily, stream, blobItem.Name)
                ));
            }
        }

        return families;
    }

    /// <summary>
    /// Creates a <see cref="RevitFamilyInfo"/> instance for the specified blob.
    /// </summary>
    /// <param name="blobName">The name of the blob.</param>
    /// <returns>A <see cref="RevitFamilyInfo"/> instance containing metadata and file information.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown if the blob container client is not initialized.</exception>
    private RevitFamilyInfo CreateFamilyInfo(string blobName)
    {
        if (_blobContainerClient is null)
        {
            throw new InvalidOperationException("BlobContainerClient is not initialized.");
        }

        // RevitFamilyInfo attempts to retrieve additional information from the "BIM.FamilyManager" storage, if it is present within the family file.
        var familyInfo = CreateFamilyInfo(LoadFileStream);
        return familyInfo;

        Stream LoadFileStream()
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            var memoryStream = new MemoryStream();
            blobClient.DownloadTo(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }
    }

    /// <summary>
    /// Saves a Revit family to Azure Blob Storage and updates its metadata.
    /// </summary>
    /// <param name="family">The Revit family to save.</param>
    /// <param name="stream">The stream containing the family data.</param>
    /// <param name="blobName">The name of the blob to save to.</param>
    /// <exception cref="InvalidOperationException">Thrown if the blob container client is not initialized.</exception>
    private void SaveFamily(IRevitFamily family, Stream stream, string blobName)
    {
        if (_blobContainerClient is null)
        {
            throw new InvalidOperationException("BlobContainerClient is not initialized.");
        }

        CreateBackup(blobName);

        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        stream.Position = 0;
        blobClient.Upload(stream, true);
        family.ApplyUpdate(CreateFamilyInfo(blobName));
    }

    /// <summary>
    /// Creates a backup of the specified blob by generating a new versioned backup file name
    /// and copying the blob to the new backup location.
    /// </summary>
    /// <param name="blobName">The name of the blob to back up.</param>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the <see cref="_blobContainerClient" /> is not initialized.
    /// </exception>
    /// <remarks>
    ///     The backup file name is generated by appending a version number in the format ".0001", ".0002", etc.,
    ///     to the original blob name. Existing backup files are identified using a regex pattern.
    /// </remarks>
    private void CreateBackup(string blobName)
    {
        if (_blobContainerClient is null)
        {
            throw new InvalidOperationException("BlobContainerClient is not initialized.");
        }

        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(blobName);
        var extension = Path.GetExtension(blobName);

        var prefix = blobName.Substring(0, blobName.Length - extension.Length + 1);
        // Regex to match backup files with the format: MyFile.0001.rfa
        var pattern = $@"^{Regex.Escape(fileNameWithoutExtension)}\.\d{{4}}{Regex.Escape(extension)}$";

        var backupBlobs = _blobContainerClient.GetBlobs(prefix: prefix)
                                              .Where(b => Regex.IsMatch(Path.GetFileName(b.Name), pattern, RegexOptions.IgnoreCase))
                                              .Select(b => int.Parse(Path.GetFileName(b.Name)
                                                                         .Substring(fileNameWithoutExtension.Length + 1, 4)))
                                              .OrderByDescending(n => n);

        // Determine the next backup number
        var nextBackupNumber = backupBlobs.FirstOrDefault() + 1;
        var backupBlobName = blobName.Insert(blobName.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase), $".{nextBackupNumber:D4}");
        CopyBlob(blobName, backupBlobName);
    }

    /// <summary>
    /// Creates a new instance of a Revit family and registers it with the family manager.
    /// </summary>
    /// <param name="familyName">The name of the Revit family to be created.</param>
    /// <param name="familyInfo">The metadata and information associated with the Revit family.</param>
    /// <param name="saveAction">
    ///     An action that defines how the Revit family should be saved, taking the Revit family instance
    ///     and a stream as parameters.
    /// </param>
    /// <returns>An instance of <see cref="IRevitFamily" /> representing the created Revit family.</returns>
    /// <remarks>
    ///     This method utilizes the family factory to create a new Revit family instance based on the provided
    ///     name and metadata. The created family is then registered with the family manager to ensure it is
    ///     tracked and managed appropriately.
    /// </remarks>
    private IRevitFamily CreateRevitFamily(string familyName, RevitFamilyInfo familyInfo, Action<IRevitFamily, Stream> saveAction)
    {
        var family = FamilyFactory(familyName, familyInfo, saveAction);
        FamilyManager.RegisterRevitFamily(family);

        return family;
    }
}
