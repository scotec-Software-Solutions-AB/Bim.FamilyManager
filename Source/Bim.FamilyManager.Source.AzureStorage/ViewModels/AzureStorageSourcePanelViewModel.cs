using System.Diagnostics;
using System.Windows.Input;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.Input;
using Scotec.Identity.AzureActiveDirectory;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Source.AzureStorage.Logic;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Source.AzureStorage.ViewModels;

public class AzureStorageSourcePanelViewModel : ViewModel, IFamilySourcePanelViewModel
{
    private readonly IAadAuthService _authService;
    private readonly AzureStorageSource _familySource;
    private readonly RelayCommand _signInCommand;
    private IAadAuthSession? _session;

    public AzureStorageSourcePanelViewModel(IFamilySource familySource, IAadAuthService authService)
    {
        _authService = authService;
        _familySource = (AzureStorageSource)familySource;
        _signInCommand = new RelayCommand(() => SignInAsync(false), () => true);
        SignInAsync(true);
    }

    public ICommand SignInCommand => _signInCommand;

    public string SignedInAs => _session?.Account?.Username ?? "not signed in";

    private async void SignInAsync(bool silent)
    {
        try
        {
            var tenantId = _familySource.SourceOptions.TenantId;
            var clientId = _familySource.SourceOptions.ClientId;
            if (!_authService.TryGetSession(tenantId, clientId, out _session) || !_session.IsSignedIn || !silent)
            {
                var options = new AadAuthOptions
                {
                    TenantId = tenantId,
                    ClientId = clientId,
                    Scopes = ["https://storage.azure.com/.default"]
                };

                var windowHandle = Process.GetCurrentProcess().MainWindowHandle;
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3600));
                
                _session = await (silent ? _authService.SignInSilentAsync(options, cts.Token) : _authService.SignInAsync(options,
                    builder => builder.WithParentActivityOrWindow(windowHandle),
                    cts.Token));

                if (!silent)
                {
                    _familySource.Reload();
                }
            }

            OnPropertyChanged(nameof(SignedInAs));
        }
        catch (Exception e)
        {
            //
        }
    }
}
