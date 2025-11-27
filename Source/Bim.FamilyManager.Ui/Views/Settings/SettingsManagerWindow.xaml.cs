using Autodesk.Revit.UI;
using Bim.FamilyManager.Ui.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;
using Scotec.Revit.Wpf;

namespace Bim.FamilyManager.Ui.Views.Settings;

/// <summary>
///     Interaction logic for <see cref="SettingsManagerWindow" />.
/// </summary>
public partial class SettingsManagerWindow : RevitWindow
{
    //private readonly SettingsManagerViewModel _settings;

    /// <summary>
    ///     Represents a factory delegate for creating instances of the <see cref="SettingsManagerWindow" /> class.
    /// </summary>
    /// <returns>
    ///     A new instance of <see cref="SettingsManagerWindow" />.
    /// </returns>
    public delegate SettingsManagerWindow Factory();

    /// <summary>
    ///     Initializes a new instance of the <see cref="SettingsManagerWindow" /> class.
    /// </summary>
    /// <param name="revitApplication">
    ///     The <see cref="Autodesk.Revit.UI.UIControlledApplication" /> instance representing the Revit application.
    /// </param>
    /// <param name="scopeFactory">
    ///     The <see cref="IServiceScopeFactory" /> used to create a scope for dependency injection.
    /// </param>
    /// <remarks>
    ///     This constructor sets up the data context for the window using the provided settings view model,
    ///     assigns the close action for the settings, and initializes the window components.
    ///     The created service scope is disposed when the window is closed.
    /// </remarks>
    public SettingsManagerWindow(UIControlledApplication revitApplication, IServiceScopeFactory scopeFactory) : base(revitApplication)
    {
        var scope = scopeFactory.CreateScope();
        Closed += (s, e) => { scope.Dispose(); };

        var settings = scope.ServiceProvider.GetRequiredService<SettingsManagerViewModel>();

        settings.CloseAction = Close;
        DataContext = settings;
        InitializeComponent();
    }
}
