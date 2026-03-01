using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Base.Logic;
using Bim.FamilyManager.Source.AzureStorage.Options;
using Scotec.Events.WeakEvents;
using Scotec.Identity.AzureActiveDirectory;
using Scotec.Revit.RevitFamily;
using TokenCache = Scotec.Identity.AzureActiveDirectory.TokenCache;

namespace Bim.FamilyManager.Source.AzureStorage.Logic;

/// <summary>
///     Represents a family source backed by Azure Blob Storage, providing access to folders and Revit families.
///     Handles authentication, connection management, and blob operations.
/// </summary>
/// <remarks>
///     This class uses <see cref="AzureBlobCache" /> to efficiently cache and enumerate blobs and folders.
///     All folder and family queries are performed in-memory after the initial cache population, minimizing Azure API
///     calls.
/// </remarks>
public sealed class AzureStorageSource : FamilySource<AzureStorageSourceOptions>
{
    /// <summary>
    ///     Delegate for AzureStorageSource events.
    /// </summary>
    /// <param name="sender">The <see cref="AzureStorageSource" /> instance raising the event.</param>
    public delegate void AzureStorageSourceEventHandler(AzureStorageSource sender);

    /// <summary>
    ///     Delegate for creating an <see cref="AzureStorageSource" /> instance.
    /// </summary>
    /// <param name="options">The options for the Azure storage source.</param>
    /// <returns>A new <see cref="AzureStorageSource" /> instance.</returns>
    public delegate AzureStorageSource Factory(AzureStorageSourceOptions options);

    private static readonly Stream PreviewStream;
    private static readonly Regex BackupRegex = new(@"\.\d{4}\.rfa$", RegexOptions.Compiled);
    private readonly IAadAuthService _authService;
    private AzureBlobCache? _blobCache;
    private BlobContainerClient? _blobContainerClient;
    private IEnumerable<IFolder>? _folders;
    private IAadAuthSession? _session;
    private CancellationTokenSource _tokenSource = new();

    /// <summary>
    ///     Static constructor. Loads the preview image resource for Azure storage source.
    /// </summary>
    /// <remarks>
    ///     Loads a PNG image from resources for use as a preview icon.
    /// </remarks>
    static AzureStorageSource()
    {
        const string packUri =
            "pack://application:,,,/Bim.FamilyManager.Source.AzureStorage;component/Resources/Images/Azure_128x128.png";

        PreviewStream = LoadResourceAsStream(packUri);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AzureStorageSource" /> class.
    /// </summary>
    /// <param name="options">The Azure storage source options.</param>
    /// <param name="familyManager">The family manager instance.</param>
    /// <param name="familyFactory">The factory for creating Revit families.</param>
    /// <param name="authService">The Azure AD authentication service.</param>
    /// <remarks>
    ///     Sets up authentication and starts a silent connection attempt to Azure Blob Storage.
    /// </remarks>
    public AzureStorageSource(
        AzureStorageSourceOptions options,
        IFamilyManager familyManager,
        IRevitFamily.Factory familyFactory,
        IAadAuthService authService)
        : base(options, familyManager, familyFactory)
    {
        _authService = authService;

        _ = ConnectToAzureStorageSilentAsync(CancellationToken.None);
    }

    /// <summary>
    ///     Gets the preview image stream for the Azure storage source.
    /// </summary>
    /// <remarks>
    ///     Returns a stream positioned at the beginning for reading the preview image.
    /// </remarks>
    public override Stream Preview
    {
        get
        {
            PreviewStream.Position = 0;
            return PreviewStream;
        }
    }

    /// <summary>
    ///     Gets the options for the Azure storage source.
    /// </summary>
    public AzureStorageSourceOptions SourceOptions => Options;

    /// <summary>
    ///     Gets or sets the current Azure AD authentication session.
    ///     Handles event registration and connection management.
    /// </summary>
    /// <remarks>
    ///     When the session changes, event handlers are updated and the connection is managed accordingly.
    /// </remarks>
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
    ///     Gets the collection of folders from Azure Blob Storage.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>An asynchronous stream of <see cref="IFolder" /> representing the folders containing Revit families.</returns>
    /// <remarks>
    ///     This method initializes the <see cref="AzureBlobCache" /> and uses it to enumerate folders efficiently.
    ///     The folders are retrieved using <see cref="GetFoldersFromCache" />.
    /// </remarks>
    public override async IAsyncEnumerable<IFolder> GetFoldersAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (_blobContainerClient is null)
        {
            yield break;
        }

        if (_folders is null)
        {
            var folders = new List<IFolder>();
            var token = _tokenSource.Token;

            _blobCache ??= new AzureBlobCache(_blobContainerClient);
            await _blobCache.InitializeAsync(token);

            await foreach (var folder in GetFoldersFromCache(Options.RootPath.EndsWith("/") ? Options.RootPath : Options.RootPath + "/", token))
            {
                cancellationToken.ThrowIfCancellationRequested();
                folders.Add(folder);
            }

            _folders = folders;
        }

        foreach (var folder in _folders)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return folder;
        }
    }

    /// <summary>
    ///     Occurs when the Azure storage source is connected.
    /// </summary>
    public event AzureStorageSourceEventHandler? Connected;

    /// <summary>
    ///     Occurs when the Azure storage source is disconnected.
    /// </summary>
    public event AzureStorageSourceEventHandler? Disconnected;

    /// <summary>
    ///     Connects to Azure Blob Storage using interactive authentication.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    ///     This method attempts to establish an authenticated session and connect to Azure Blob Storage.
    /// </remarks>
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
    ///     Copies a blob within the Azure Blob Storage container.
    /// </summary>
    /// <param name="sourceBlobName">The name of the source blob.</param>
    /// <param name="destinationBlobName">The name of the destination blob.</param>
    /// <exception cref="System.InvalidOperationException">Thrown if the blob container client is not initialized.</exception>
    /// <remarks>
    ///     This method performs a server-side copy of a blob within the same container.
    /// </remarks>
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
    ///     Called when the source is reloaded.
    /// </summary>
    /// <remarks>
    ///     Resets the folder and cache state, and cancels any outstanding operations.
    /// </remarks>
    protected override void OnReload()
    {
        _folders = null;
        _blobCache = null;
        _tokenSource.Cancel(true);
        _tokenSource = new CancellationTokenSource();
    }

    /// <summary>
    ///     Retrieves folders from the cached blobs under the specified prefix.
    /// </summary>
    /// <param name="prefix">The blob prefix to search for folders.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>An asynchronous stream of <see cref="IFolder" /> representing the subfolders.</returns>
    /// <remarks>
    ///     This method uses <see cref="AzureBlobCache.GetImmediateSubfolders" /> for efficient folder enumeration.
    /// </remarks>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    private async IAsyncEnumerable<IFolder> GetFoldersFromCache(string prefix, [EnumeratorCancellation] CancellationToken cancellationToken)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        if (_blobCache is null)
        {
            yield break;
        }

        foreach (var itemPrefix in _blobCache.GetImmediateSubfolders(prefix))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new Folder(
                Path.GetFileName(itemPrefix.Trim('/')),
                c => GetFoldersFromCache(itemPrefix, c),
                (i, c) => GetFamiliesFromCache(itemPrefix, i, c)
            );
        }
    }

    /// <summary>
    ///     Retrieves Revit families from the cached blobs under the specified prefix.
    /// </summary>
    /// <param name="prefix">The blob prefix used to filter family files in the storage.</param>
    /// <param name="includeSubfolders">If true, includes families in all subfolders; otherwise, only direct children.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>An asynchronous stream of <see cref="IRevitFamily" /> objects representing the Revit families found.</returns>
    /// <remarks>
    ///     This method uses <see cref="AzureBlobCache.GetBlobNamesByPrefix" /> for efficient file enumeration.
    ///     It matches blob names and creates or retrieves <see cref="IRevitFamily" /> instances.
    /// </remarks>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    private async IAsyncEnumerable<IRevitFamily> GetFamiliesFromCache(string prefix, bool includeSubfolders,
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                                                                      [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (_blobCache is null)
        {
            yield break;
        }

        foreach (var blobName in _blobCache.GetBlobNamesByPrefix(prefix, includeSubfolders))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (BackupRegex.IsMatch(blobName) || !blobName.EndsWith(".rfa", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var familyName = Path.GetFileNameWithoutExtension(blobName);
            if (FamilyManager.TryGetRevitFamily(familyName, out var family))
            {
                yield return family;
            }
            else
            {
                yield return CreateRevitFamily(
                    familyName,
                    CreateFamilyInfo(blobName),
                    (revitFamily, stream) => SaveFamily(revitFamily, stream, blobName)
                );
            }
        }
    }

    /// <summary>
    ///     Handles the SignedOut event for the Azure AD session.
    /// </summary>
    /// <param name="session">The Azure AD session.</param>
    /// <param name="args">The event arguments.</param>
    /// <remarks>
    ///     Disconnects from Azure Blob Storage when the session is signed out.
    /// </remarks>
    private void OnSessionSignedOut(IAadAuthSession session, EventArgs args)
    {
        try
        {
            Disconnect();
        }
        catch (Exception)
        {
            //TODO:
        }
    }

    /// <summary>
    ///     Disconnects from Azure Blob Storage and clears cached data.
    /// </summary>
    /// <remarks>
    ///     This method resets the blob container client, folder cache, and triggers the <see cref="Disconnected" /> event.
    /// </remarks>
    private void Disconnect()
    {
        _blobContainerClient = null;
        _folders = null;
        _blobCache = null;
        Disconnected?.Invoke(this);
        Reload();
    }

    /// <summary>
    ///     Handles the SignedIn event for the Azure AD session.
    /// </summary>
    /// <param name="session">The Azure AD session.</param>
    /// <param name="args">The event arguments.</param>
    /// <remarks>
    ///     Connects to Azure Blob Storage when the session is signed in.
    /// </remarks>
    private void OnSessionSignedIn(IAadAuthSession session, EventArgs args)
    {
        try
        {
            Connect();
        }
        catch (Exception)
        {
            //TODO
        }
    }

    /// <summary>
    ///     Establishes a connection to Azure Blob Storage using the current session.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown if the session is null.</exception>
    /// <remarks>
    ///     This method creates a <see cref="BlobContainerClient" /> using the session's token credential.
    /// </remarks>
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
    ///     Creates authentication options for Azure AD.
    /// </summary>
    /// <returns>An <see cref="AadAuthOptions" /> instance configured for the current source options.</returns>
    /// <remarks>
    ///     This method builds the authentication options required for Azure AD token acquisition.
    /// </remarks>
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
    ///     Attempts to connect to Azure Blob Storage silently using cached credentials.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    ///     This method attempts to sign in using cached credentials, falling back to interactive sign-in if necessary.
    /// </remarks>
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
    ///     Creates a <see cref="RevitFamilyInfo" /> instance for the specified blob.
    /// </summary>
    /// <param name="blobName">The name of the blob.</param>
    /// <returns>A <see cref="RevitFamilyInfo" /> instance containing metadata and file information.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown if the blob container client is not initialized.</exception>
    /// <remarks>
    ///     This method downloads the blob to a memory stream and creates a <see cref="RevitFamilyInfo" /> for it.
    /// </remarks>
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
            // TODO: Consider streaming to file for large blobs to reduce memory usage.
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            var memoryStream = new MemoryStream();
            blobClient.DownloadTo(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }
    }

    /// <summary>
    ///     Saves a Revit family to Azure Blob Storage and updates its metadata.
    /// </summary>
    /// <param name="family">The Revit family to save.</param>
    /// <param name="stream">The stream containing the family data.</param>
    /// <param name="blobName">The name of the blob to save to.</param>
    /// <exception cref="System.InvalidOperationException">Thrown if the blob container client is not initialized.</exception>
    /// <remarks>
    ///     This method creates a backup of the existing blob before overwriting it, then uploads the new data and updates the
    ///     family metadata.
    /// </remarks>
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
    ///     Creates a backup of the specified blob by generating a new versioned backup file name
    ///     and copying the blob to the new backup location.
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

        var options = new GetBlobsOptions
        {
            Prefix = prefix
        };

        var backupBlobs = _blobContainerClient.GetBlobs(options)
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
    ///     Creates a new instance of a Revit family and registers it with the family manager.
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
