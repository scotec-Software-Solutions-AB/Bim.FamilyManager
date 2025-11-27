using System.Diagnostics;
using System.Windows.Input;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Source.AzureStorage.Logic;
using Bim.FamilyManager.Source.AzureStorage.Options;
using CommunityToolkit.Mvvm.Input;
using Scotec.Events.WeakEvents;
using Scotec.Identity.AzureActiveDirectory;

namespace Bim.FamilyManager.Source.AzureStorage.ViewModels;

/// <summary>
///     View model for the Azure Storage source panel, providing authentication and sign-in functionality for Azure-based
///     family sources.
/// </summary>
public class AzureStorageSourcePanelViewModel : FamilySourcePanelViewModel<AzureStorageSource>
{
    private readonly IAadAuthService _authService;
    private readonly IAadAuthSession _session;
    private readonly RelayCommand _signInCommand;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AzureStorageSourcePanelViewModel" /> class.
    /// </summary>
    /// <param name="familySource">The Azure storage family source.</param>
    /// <param name="authService">The Azure AD authentication service.</param>
    /// <exception cref="System.ArgumentException">Thrown if tenant ID or client ID is null.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown if no valid authentication session is found.</exception>
    public AzureStorageSourcePanelViewModel(AzureStorageSource familySource, IAadAuthService authService) : base(familySource)
    {
        _authService = authService;
        _signInCommand = new RelayCommand(SignInAsync, () => true);

        _session = GetAadAuthSession(familySource.SourceOptions);

        // Subscribe using WeakEventManager
        StaticWeakEventManager.AddWeakHandler<IAadAuthSession, EventArgs>(_session, nameof(_session.SignedIn), OnSignedIn);
        StaticWeakEventManager.AddWeakHandler<IAadAuthSession, EventArgs>(_session, nameof(_session.SignedOut), OnSignedOut);
    }

    /// <summary>
    ///     Gets the command used to initiate sign-in to Azure AD.
    /// </summary>
    public ICommand SignInCommand => _signInCommand;

    /// <summary>
    ///     Gets the username of the currently signed-in Azure AD account, or "not signed in" if no session is active.
    /// </summary>
    public string SignedInAs => _session.Account?.Username ?? "not signed in";

    /// <summary>
    ///     Handles the SignedOut event for the Azure AD session and updates the signed-in status.
    /// </summary>
    /// <param name="session">The Azure AD session.</param>
    /// <param name="args">The event arguments.</param>
    private void OnSignedOut(IAadAuthSession session, EventArgs args)
    {
        OnPropertyChanged(nameof(SignedInAs));
    }

    /// <summary>
    ///     Handles the SignedIn event for the Azure AD session and updates the signed-in status.
    /// </summary>
    /// <param name="session">The Azure AD session.</param>
    /// <param name="args">The event arguments.</param>
    private void OnSignedIn(IAadAuthSession session, EventArgs args)
    {
        OnPropertyChanged(nameof(SignedInAs));
    }

    /// <summary>
    ///     Retrieves the Azure AD authentication session for the specified options.
    /// </summary>
    /// <param name="options">The Azure storage source options.</param>
    /// <returns>The Azure AD authentication session.</returns>
    /// <exception cref="System.ArgumentException">Thrown if tenant ID or client ID is null.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown if no valid authentication session is found.</exception>
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

    /// <summary>
    ///     Initiates the interactive sign-in process to Azure AD.
    /// </summary>
    private async void SignInAsync()
    {
        try
        {
            var windowHandle = Process.GetCurrentProcess().MainWindowHandle;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3600));
            _ = await _session.SignInAsync(builder => builder.WithParentActivityOrWindow(windowHandle), cts.Token);

            OnPropertyChanged(nameof(SignedInAs));
        }
        catch (Exception)
        {
            // Exception handling can be extended for logging or user feedback.
        }
    }
}
