using System.Windows;

namespace Bim.FamilyManager.Ui.Views.Settings;

/// <summary>
///     Represents the settings window for managing family sources in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This window provides a user interface for configuring and managing family source settings.
///     It is designed to work with a corresponding view model implementing
///     <see cref="Abstractions.ViewModels.Settings.IFamilySourceSettingsViewModel" />.
/// </remarks>
public partial class SettingsContentWindow : Window
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySourceSettingsEditWindow" /> class.
    /// </summary>
    /// <remarks>
    ///     This constructor sets up the window by initializing its components. It is typically invoked when creating a new
    ///     instance of the settings window.
    /// </remarks>
    public SettingsContentWindow()
    {
        InitializeComponent();
    }
}
