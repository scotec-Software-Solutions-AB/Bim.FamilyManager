using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Source.AzureStorage.Logic;
using Bim.FamilyManager.Source.AzureStorage.Options;
using CommunityToolkit.Mvvm.Input;
using Scotec.Identity.AzureActiveDirectory;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Bim.FamilyManager.Source.AzureStorage.ViewModels;

public class AzureStorageSourcePanelViewModel : FamilySourcePanelViewModel<AzureStorageSource>
{
    private readonly IAadAuthService _authService;
    private readonly RelayCommand _signInCommand;
    private readonly IAadAuthSession _session;

    public AzureStorageSourcePanelViewModel(AzureStorageSource familySource, IAadAuthService authService) : base(familySource)
    {
        _authService = authService;
        _signInCommand = new RelayCommand(SignInAsync, () => true);

        _session = GetAadAuthSession(familySource.SourceOptions);


        // Subscribe using WeakEventManager
        WeakEventManager<IAadAuthSession, EventArgs>.AddHandler(
            _session, nameof(_session.SignedIn), OnConnected);

    }

    private void OnConnected(object? sender, EventArgs e)
    {
    }

    private IAadAuthSession GetAadAuthSession(AzureStorageSourceOptions familySourceSourceOptions)
    {
        var tenantId = familySourceSourceOptions.TenantId;
        var clientId = familySourceSourceOptions.ClientId;

        if (!_authService.TryGetSession(tenantId, clientId, out var session))
        {
            throw new InvalidOperationException("No valid authentication session found.");
        }


        return session;
    }

    public ICommand SignInCommand => _signInCommand;

    public string SignedInAs => _session?.Account?.Username ?? "not signed in";

    private async void SignInAsync()
    {
        try
        {
            var windowHandle = Process.GetCurrentProcess().MainWindowHandle;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3600));
            var result = await _session.SignInAsync(builder => builder.WithParentActivityOrWindow(windowHandle), cts.Token);

            OnPropertyChanged(nameof(SignedInAs));
        }
        catch (Exception e)
        {
            //
        }
    }
}
