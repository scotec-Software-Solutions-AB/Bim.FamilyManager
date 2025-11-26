using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Bim.FamilyManager.Source.AzureStorage.Options;
using Bim.FamilyManager.Ui.ViewModels.Settings;
using CommunityToolkit.Mvvm.Input;
using Scotec.Events.WeakEvents;
using Scotec.Identity.AzureActiveDirectory;
using TokenCache = Scotec.Identity.AzureActiveDirectory.TokenCache;

namespace Bim.FamilyManager.Source.AzureStorage.ViewModels.Settings;

/// <summary>
///     Represents the view model for managing settings related to a Azure Storage based family source.
/// </summary>
/// <remarks>
///     This class extends
///     <see cref="FamilySourceSettingsViewModel{TOptions}" />
///     with specific functionality for handling AzureStorage based family source options.
/// </remarks>
public class AzureStorageSourceSettingsViewModel : FamilySourceSettingsViewModel<AzureStorageSourceOptions>
{
    private readonly IAadAuthService _authService;
    private readonly RelayCommand _signInCommand;
    private string _clientId;
    private string _containerName;
    private string _endpoint;
    private string _rootPath;
    private IAadAuthSession? _session;
    private string _tenantId;

    public AzureStorageSourceSettingsViewModel(IAadAuthService authService, AzureStorageSourceOptions options)
        : base(options)
    {
        _authService = authService;
        _rootPath = options.RootPath;
        _containerName = options.ContainerName;
        _clientId = options.ClientId?.ToString("D") ?? string.Empty;
        _tenantId = options.TenantId?.ToString("D") ?? string.Empty;
        _endpoint = options.Endpoint;

        _signInCommand = new RelayCommand(SignIn, () => CanSignIn);

        if (CanSignIn)
        {
            TrySilentSignInAsync();
        }
    }

    public ICommand SignInCommand => _signInCommand;

    public bool CanSignIn => CanApply();

    //TODO Add string to resources.
    public string SignedInAs => Session?.Account?.Username ?? "Not signed in.";

    /// <summary>
    ///     Gets the source identifier for the Azure Storage based family source.
    /// </summary>
    /// <remarks>
    ///     This property overrides the
    ///     <see cref="FamilySourceSettingsViewModel{TOptions}.Source" />
    ///     property to return the value of the <see cref="RootPath" /> property, which represents the directory path.
    /// </remarks>
    public override string Source => RootPath;

    /// <summary>
    ///     Gets or sets the directory path associated with the Azure Storage based family source.
    /// </summary>
    /// <remarks>
    ///     This property represents the path to the directory used as the source for family management.
    ///     Modifying this property will mark the settings as modified and trigger necessary updates.
    /// </remarks>
    public string RootPath
    {
        get => _rootPath;
        set
        {
            SetProperty(ref _rootPath, value);
            IsModified = true;
            OnPropertyChanged(nameof(Source));
        }
    }

    public string ContainerName
    {
        get => _containerName;
        set
        {
            SetProperty(ref _containerName, value);
            IsModified = true;
        }
    }

    public string ClientId
    {
        get => _clientId;
        set
        {
            SetProperty(ref _clientId, value);
            IsModified = true;
        }
    }

    public string TenantId
    {
        get => _tenantId;
        set
        {
            SetProperty(ref _tenantId, value);
            IsModified = true;
        }
    }

    public string EndPoint
    {
        get => _endpoint;
        set
        {
            SetProperty(ref _endpoint, value);
            IsModified = true;
        }
    }

    public override string TypeName => "Azure Storage";

    private IAadAuthSession? Session
    {
        get => _session;
        set
        {
            if (_session == value)
            {
                return;
            }

            if (_session is not null)
            {
                StaticWeakEventManager.RemoveWeakHandler<IAadAuthSession, EventArgs>(_session, nameof(_session.SignedIn), OnSignedIn);
                StaticWeakEventManager.RemoveWeakHandler<IAadAuthSession, EventArgs>(_session, nameof(_session.SignedOut), OnSignedOut);
            }

            _session = value;

            if (_session is not null)
            {
                StaticWeakEventManager.AddWeakHandler<IAadAuthSession, EventArgs>(_session, nameof(_session.SignedIn), OnSignedIn);
                StaticWeakEventManager.AddWeakHandler<IAadAuthSession, EventArgs>(_session, nameof(_session.SignedOut), OnSignedOut);
            }

            OnPropertyChanged(nameof(SignedInAs));
        }
    }

    /// <summary>
    ///     Determines whether the settings for the Azure Storage based family source can be applied.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the settings can be applied; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method overrides
    ///     <see cref="FamilySourceSettingsViewModel{TOptions}.CanApply" />
    ///     to include additional validation specific to Azure Storage based family sources.
    ///     It ensures that the <see cref="RootPath" /> property is not null, empty, or whitespace,
    ///     and that the specified directory exists.
    /// </remarks>
    public override bool CanApply()
    {
        
        return base.CanApply() && !string.IsNullOrWhiteSpace(RootPath)
                               && !string.IsNullOrWhiteSpace(ContainerName)
                               && !string.IsNullOrWhiteSpace(ClientId) && Guid.TryParse(ClientId, out _)
                               && !string.IsNullOrWhiteSpace(TenantId) && Guid.TryParse(TenantId, out _)
                               && !string.IsNullOrWhiteSpace(EndPoint);
    }

    /// <summary>
    /// Applies the current settings for the Azure Storage-based family source.
    /// </summary>
    /// <remarks>
    /// This method overrides <see cref="FamilySourceSettingsViewModel{TOptions}.OnApply" /> to update the
    /// <see cref="AzureStorageSourceOptions" /> properties, such as <see cref="AzureStorageSourceOptions.RootPath" />,
    /// <see cref="AzureStorageSourceOptions.ContainerName" />, <see cref="AzureStorageSourceOptions.ClientId" />,
    /// <see cref="AzureStorageSourceOptions.TenantId" />, and <see cref="AzureStorageSourceOptions.Endpoint" />.
    /// Additionally, it attempts a silent sign-in operation.
    /// </remarks>
    /// <exception cref="NotImplementedException">
    /// Thrown if the base implementation is not properly overridden.
    /// </exception>
    protected override void OnApply()
    {
        base.OnApply();
        Options.RootPath = RootPath;
        Options.ContainerName = ContainerName;
        Options.ClientId = Guid.Parse(ClientId);
        Options.TenantId = Guid.Parse(TenantId);
        Options.Endpoint = EndPoint;

        TrySilentSignInAsync();
    }

    protected override void OnIsModified()
    {
        base.OnIsModified();
        Session = null;
        NotifyCanExecuteChanged();
    }

    private void NotifyCanExecuteChanged()
    {
        _signInCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    ///     Handles the cancellation of changes made to the Azure Storage based family source settings.
    /// </summary>
    /// <remarks>
    ///     This method overrides
    ///     <see cref="FamilySourceSettingsViewModel{TOptions}.OnReset" />
    ///     to reset the <see cref="RootPath" /> property to its value in <see cref="Options" />.
    /// </remarks>
    protected override void OnReset()
    {
        base.OnReset();
        EndPoint = Options.Endpoint;
        RootPath = Options.RootPath;
        ContainerName = Options.ContainerName;
        ClientId = Options.ClientId?.ToString("D") ?? string.Empty;
        TenantId = Options.TenantId?.ToString("D") ?? string.Empty;
    }

    private async void TrySilentSignInAsync()
    {
        try
        {
            var cacheFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BIM.FamilyManager", $"msal_cache_{Options.ClientId}.bin");

            var clientId = Guid.Parse(_clientId);
            var tenantId = Guid.Parse(_tenantId);
            if (_authService.TryGetSession(tenantId, clientId, out var session) && !session.IsSignedIn)
            {
                var authOptions = new AadAuthOptions
                {
                    ClientId =clientId,
                    TenantId = tenantId,
                    Scopes = ["https://storage.azure.com/.default"],
                    TokenCache = new TokenCache(cacheFile)
                };
                session = await _authService.SignInSilentAsync(authOptions, CancellationToken.None);
            }

            Session = session;
        }
        catch (Exception)
        {
            Session = null;
            OnPropertyChanged(nameof(SignedInAs));
        }
    }

    private void OnSignedOut(IAadAuthSession arg1, EventArgs arg2)
    {
        OnPropertyChanged(nameof(SignedInAs));
    }

    private void OnSignedIn(IAadAuthSession arg1, EventArgs arg2)
    {
        OnPropertyChanged(nameof(SignedInAs));
    }

    private async void SignIn()
    {
        try
        {
            var cacheFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BIM.FamilyManager", $"msal_cache_{Options.ClientId}.bin");

            var authOptions = new AadAuthOptions
            {
                ClientId = Guid.Parse(_clientId),
                TenantId = Guid.Parse(_tenantId),
                Scopes = ["https://storage.azure.com/.default"],
                TokenCache = new TokenCache(cacheFile)
            };

            var windowHandle = Process.GetCurrentProcess().MainWindowHandle;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3600));
            var session = await _authService.SignInAsync(authOptions,
                builder => builder.WithParentActivityOrWindow(windowHandle),
                cts.Token);

            Session = session;
        }
        //catch (MsalUiRequiredException)
        //{
        //}
        catch (Exception e)
        {
            Session = null;
        }
    }
}
