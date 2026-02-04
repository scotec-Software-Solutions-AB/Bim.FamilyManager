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
///     Represents the view model for managing settings related to an Azure Storage-based family source.
/// </summary>
/// <remarks>
///     This class extends <see cref="FamilySourceSettingsViewModel{TOptions}" /> with specific functionality for handling
///     Azure Storage family source options,
///     including authentication, property management, and command handling.
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

    /// <summary>
    ///     Initializes a new instance of the <see cref="AzureStorageSourceSettingsViewModel" /> class.
    /// </summary>
    /// <param name="authService">The Azure AD authentication service.</param>
    /// <param name="options">The Azure Storage source options.</param>
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

    /// <summary>
    ///     Gets the command used to initiate sign-in to Azure AD.
    /// </summary>
    public ICommand SignInCommand => _signInCommand;

    /// <summary>
    ///     Gets a value indicating whether sign-in can be performed based on the current settings.
    /// </summary>
    public bool CanSignIn => CanApply();

    /// <summary>
    ///     Gets a string indicating the signed-in user, or "Not signed in." if no session is active.
    /// </summary>
    public string SignedInAs => Session?.Account?.Username ?? "Not signed in.";

    /// <summary>
    ///     Gets the source identifier for the Azure Storage-based family source.
    /// </summary>
    /// <remarks>
    ///     This property overrides <see cref="FamilySourceSettingsViewModel{TOptions}.Source" /> to return the value of the
    ///     <see cref="RootPath" /> property.
    /// </remarks>
    public override string Source => RootPath;

    /// <summary>
    ///     Gets or sets the directory path associated with the Azure Storage-based family source.
    /// </summary>
    /// <remarks>
    ///     Modifying this property marks the settings as modified and triggers necessary updates.
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

    /// <summary>
    ///     Gets or sets the name of the Azure Blob container.
    /// </summary>
    public string ContainerName
    {
        get => _containerName;
        set
        {
            SetProperty(ref _containerName, value);
            IsModified = true;
        }
    }

    /// <summary>
    ///     Gets or sets the Azure AD client ID as a string.
    /// </summary>
    public string ClientId
    {
        get => _clientId;
        set
        {
            SetProperty(ref _clientId, value);
            IsModified = true;
        }
    }

    /// <summary>
    ///     Gets or sets the Azure AD tenant ID as a string.
    /// </summary>
    public string TenantId
    {
        get => _tenantId;
        set
        {
            SetProperty(ref _tenantId, value);
            IsModified = true;
        }
    }

    /// <summary>
    ///     Gets or sets the Azure Storage endpoint URL.
    /// </summary>
    public string EndPoint
    {
        get => _endpoint;
        set
        {
            SetProperty(ref _endpoint, value);
            IsModified = true;
        }
    }

    /// <summary>
    ///     Gets the display name for the Azure Storage source type.
    /// </summary>
    public override string TypeName => "Azure Storage";

    /// <summary>
    ///     Gets or sets the current Azure AD authentication session.
    /// </summary>
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
    ///     Determines whether the settings for the Azure Storage-based family source can be applied.
    /// </summary>
    /// <returns><c>true</c> if the settings can be applied; otherwise, <c>false</c>.</returns>
    /// <remarks>
    ///     This method overrides <see cref="FamilySourceSettingsViewModel{TOptions}.CanApply" /> to include additional
    ///     validation specific to Azure Storage sources.
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
    ///     Applies the current settings for the Azure Storage-based family source.
    /// </summary>
    /// <remarks>
    ///     This method overrides <see cref="FamilySourceSettingsViewModel{TOptions}.OnApply" /> to update the
    ///     <see cref="AzureStorageSourceOptions" /> properties and attempts a silent sign-in operation.
    /// </remarks>
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

    /// <summary>
    ///     Called when the modification state changes. Resets the session and updates command state.
    /// </summary>
    protected override void OnIsModified()
    {
        base.OnIsModified();
        Session = null;
        NotifyCanExecuteChanged();
    }

    /// <summary>
    ///     Handles the cancellation of changes made to the Azure Storage-based family source settings.
    /// </summary>
    /// <remarks>
    ///     This method overrides <see cref="FamilySourceSettingsViewModel{TOptions}.OnReset" /> to reset all relevant
    ///     properties to their values in <see cref="Options" />.
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

    /// <summary>
    ///     Notifies the sign-in command that its ability to execute may have changed.
    /// </summary>
    private void NotifyCanExecuteChanged()
    {
        _signInCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    ///     Attempts to silently sign in to Azure AD using cached credentials.
    /// </summary>
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
                    ClientId = clientId,
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

    /// <summary>
    ///     Handles the SignedOut event for the Azure AD session.
    /// </summary>
    /// <param name="arg1">The Azure AD session.</param>
    /// <param name="arg2">The event arguments.</param>
    private void OnSignedOut(IAadAuthSession arg1, EventArgs arg2)
    {
        OnPropertyChanged(nameof(SignedInAs));
    }

    /// <summary>
    ///     Handles the SignedIn event for the Azure AD session.
    /// </summary>
    /// <param name="arg1">The Azure AD session.</param>
    /// <param name="arg2">The event arguments.</param>
    private void OnSignedIn(IAadAuthSession arg1, EventArgs arg2)
    {
        OnPropertyChanged(nameof(SignedInAs));
    }

    /// <summary>
    ///     Initiates the interactive sign-in process to Azure AD.
    /// </summary>
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
        catch (Exception)
        {
            Session = null;
        }
    }
}
