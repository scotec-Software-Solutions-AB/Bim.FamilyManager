using System.Windows.Controls;

namespace Bim.FamilyManager.Source.AzureStorage.Views.Settings;

/// <summary>
///     Represents the view for configuring directory source settings in the application.
/// </summary>
/// <remarks>
///     This class is a WPF UserControl that provides the UI for managing directory source settings.
///     It is associated with the <see cref="FamilyManager.ViewModels.DirectorySourceSettingsViewModel" />
///     through the MVVM pattern.
/// </remarks>
public partial class AzureStorageSourceSettingsView : UserControl
{
    public AzureStorageSourceSettingsView()
    {
        InitializeComponent();
    }
}
