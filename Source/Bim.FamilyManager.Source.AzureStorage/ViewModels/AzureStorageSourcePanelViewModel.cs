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
using Scotec.Events.WeakEvents;

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
        StaticWeakEventManager.AddWeakHandler<IAadAuthSession, EventArgs>(_session, nameof(_session.SignedIn), OnSignedIn);
        StaticWeakEventManager.AddWeakHandler<IAadAuthSession, EventArgs>(_session, nameof(_session.SignedOut), OnSignedOut);
    }

    private void OnSignedOut(IAadAuthSession session, EventArgs args)
    {
        OnPropertyChanged(nameof(SignedInAs));
    }

    private void OnSignedIn(IAadAuthSession session, EventArgs args)
    {
        OnPropertyChanged(nameof(SignedInAs));
    }
    
    private IAadAuthSession GetAadAuthSession(AzureStorageSourceOptions options)
    {
        if (options.TenantId is null || options.ClientId is null)
        {
            throw new ArgumentException("Tenant ID and client ID must not be null");
        }

        if (!_authService.TryGetSession(options.TenantId.Value, options.ClientId.Value, out var session))
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
