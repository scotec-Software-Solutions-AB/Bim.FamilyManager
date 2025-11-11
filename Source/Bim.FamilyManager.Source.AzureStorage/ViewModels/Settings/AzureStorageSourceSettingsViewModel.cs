using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Identity.Client;
using Scotec.Identity.AzureActiveDirectory;
using Bim.FamilyManager.Source.AzureStorage.Options;
using Bim.FamilyManager.Ui.ViewModels.Settings;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
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
        _clientId = options.ClientId;
        _tenantId = options.TenantId;
        _endpoint = options.Endpoint;

        _signInCommand = new RelayCommand(SignIn, () => CanSignIn);

        TrySilentSignInAsync();
    }

    public ICommand SignInCommand => _signInCommand;

    public bool CanSignIn => CanApply();

    //TODO Add string to resources.
    public string SignedInAs => _session?.Account?.Username ?? "Not signed in.";

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
                               && !string.IsNullOrWhiteSpace(ClientId)
                               && !string.IsNullOrWhiteSpace(TenantId)
                               && !string.IsNullOrWhiteSpace(EndPoint);
    }

    /// <summary>
    ///     Applies the current settings of the Azure Storage based family source.
    /// </summary>
    /// <remarks>
    ///     This method overrides
    ///     <see cref="FamilySourceSettingsViewModel{TOptions}.OnApply" />
    ///     to update the <see cref="Options.Path" /> property with the value of the <see cref="RootPath" /> property.
    /// </remarks>
    /// <exception cref="NotImplementedException">
    ///     Thrown if the base implementation is not properly overridden.
    /// </exception>
    protected override void OnApply()
    {
        base.OnApply();
        Options.RootPath = RootPath;
        Options.ContainerName = ContainerName;
        Options.ClientId = ClientId;
        Options.TenantId = TenantId;
        Options.Endpoint = EndPoint;
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
        ClientId = Options.ClientId;
        TenantId = Options.TenantId;
    }

    private async void TrySilentSignInAsync()
    {
        try
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

            if (_authService.TryGetSession(Options.TenantId, Options.ClientId, out _session) && !_session.IsSignedIn)
            {
                _session = await _authService.SignInSilentAsync(authOptions, CancellationToken.None);
            }
        }
        catch (Exception)
        {
        }
    }

    private async void SignIn()
    {
        try
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
            if (_authService.TryGetSession(Options.TenantId, Options.ClientId, out _session))
            {
                var windowHandle = Process.GetCurrentProcess().MainWindowHandle;
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3600));
                _session = await _authService.SignInAsync(authOptions,
                    builder => builder.WithParentActivityOrWindow(windowHandle),
                    cts.Token);
            }
        }
        //catch (MsalUiRequiredException)
        //{
        //}
        catch (Exception e)
        {
        }
    }
}
