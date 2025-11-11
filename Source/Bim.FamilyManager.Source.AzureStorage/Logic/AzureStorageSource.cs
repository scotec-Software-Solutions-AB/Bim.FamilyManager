using System.IO;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.NativeInterop;
using Scotec.Identity.AzureActiveDirectory;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Base.Logic;
using Bim.FamilyManager.Source.AzureStorage.Options;
using Scotec.Revit.RevitFamily;
using TokenCache = Scotec.Identity.AzureActiveDirectory.TokenCache;

namespace Bim.FamilyManager.Source.AzureStorage.Logic;

public sealed class AzureStorageSource : FamilySource<AzureStorageSourceOptions>
{
    public delegate AzureStorageSource Factory(AzureStorageSourceOptions options);

    private static readonly Stream PreviewStream;
    private readonly ILogger<AzureStorageSource> _logger;
    private IAadAuthService _authService;
    private BlobContainerClient? _blobContainerClient;
    private IEnumerable<IFolder>? _folders;
    private IAadAuthSession? _session;

    static AzureStorageSource()
    {
        const string packUri =
            "pack://application:,,,/Bim.FamilyManager.Source.AzureStorage;component/Resources/Images/Azure_128x128.png";

        PreviewStream = LoadResourceAsStream(packUri);
    }
    
    public AzureStorageSource(AzureStorageSourceOptions options, IFamilyManager familyManager, IRevitFamily.Factory familyFactory, IAadAuthService authService, ILogger<AzureStorageSource> logger)
        : base(options, familyManager, familyFactory)
    {
        _authService = authService;
        _logger = logger;

        _ = ConnectToAzureStorageSilentAsync(CancellationToken.None);
    }

    public delegate void AzureStorageSourceEventHandler(AzureStorageSource sender);
    public event AzureStorageSourceEventHandler? Connected;
    public event AzureStorageSourceEventHandler? Disconnected;
    
    
    public override IEnumerable<IFolder> Folders => _folders ??= GetFolders(Options.RootPath);

    public override Stream? Preview
    {
        get
        {
            PreviewStream.Position = 0;
            return PreviewStream;
        }
    }

    protected override void OnReload()
    {
        _folders = null;
    }

    public AzureStorageSourceOptions SourceOptions => Options;

    public async Task ConnectToAzureStorageAsync(CancellationToken cancellationToken)
    {
        try
        {
            var authOptions = CreateAuthOptions();

            if (!_authService.TryGetSession(Options.TenantId, Options.ClientId, out _session) || !_session.IsSignedIn)
            {
                _session = await _authService.SignInSilentAsync(authOptions, cancellationToken);
            }

            if (_session is not null && _session.IsSignedIn)
            {
                _blobContainerClient = new BlobContainerClient(new Uri($"{Options.Endpoint}/{Options.ContainerName}"), _session.GetTokenCredential());
                Connected?.Invoke(this);
            }
            else
            {
                _blobContainerClient = null;
                Disconnected?.Invoke(this);
            }
        }
        catch (Exception e)
        {
            RaiseError(e);
            _session = null;
            _blobContainerClient = null;
        }
    }

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
            //ClientId = "d79c205d-6637-499c-a224-a90e6195eab9",
            //TenantId = "55ea1dea-56b6-4d91-be87-e452e768a583",
            //Scopes = ["https://storage.azure.com/.default"],
            //TokenCache = new TokenCache(cacheFile)
        };
        return authOptions;
    }

    private async Task ConnectToAzureStorageSilentAsync(CancellationToken cancellationToken)
    {
        try
        {
            var authOptions = CreateAuthOptions();
            if(!_authService.TryGetSession(Options.TenantId, Options.ClientId, out _session) || !_session.IsSignedIn)
            {
                _session = await _authService.SignInSilentAsync(authOptions, cancellationToken);
            }

            if (_session is not null && _session.IsSignedIn)
            {
                _blobContainerClient = new BlobContainerClient(new Uri($"{Options.Endpoint}/{Options.ContainerName}"), _session.GetTokenCredential());
                Connected?.Invoke(this);
            }
        }
        catch (Exception e)
        {
            RaiseError(e);
            _session = null;
            _blobContainerClient = null;
        }
    }

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

    private List<IRevitFamily> GetFamilies(string prefix)
    {
        if (_blobContainerClient is null)
        {
            return [];
        }

        var families = new List<IRevitFamily>();
        foreach (var blobItem in _blobContainerClient.GetBlobs(prefix: prefix))
        {
            if (blobItem.Name.EndsWith(".rfa", StringComparison.OrdinalIgnoreCase))
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
        }

        return families;
    }

    private RevitFamilyInfo CreateFamilyInfo(string blobName)
    {
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

    private void SaveFamily(IRevitFamily family, Stream stream, string blobName)
    {
        var blobClient = _blobContainerClient.GetBlobClient(blobName);
        stream.Position = 0;
        blobClient.Upload(stream, true);
        family.ApplyUpdate(CreateFamilyInfo(blobName));
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
