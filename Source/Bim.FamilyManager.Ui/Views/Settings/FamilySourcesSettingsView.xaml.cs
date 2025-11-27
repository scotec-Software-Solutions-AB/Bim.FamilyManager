using System.Windows.Controls;

namespace Bim.FamilyManager.Ui.Views.Settings;

/// <summary>
///     Represents the view for managing family source settings in the application.
/// </summary>
/// <remarks>
///     This class is a partial class that interacts with the corresponding XAML file to define the UI for family source
///     settings.
///     It inherits from <see cref="System.Windows.Controls.UserControl" /> and is part of the WPF framework.
/// </remarks>
public partial class FamilySourcesSettingsView : UserControl
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySourcesSettingsView" /> class.
    /// </summary>
    /// <remarks>
    ///     This constructor sets up the view for managing family source settings by initializing its components.
    ///     It is part of the WPF framework and relies on the corresponding XAML file for its UI definition.
    /// </remarks>
    public FamilySourcesSettingsView()
    {
        InitializeComponent();
    }
}
