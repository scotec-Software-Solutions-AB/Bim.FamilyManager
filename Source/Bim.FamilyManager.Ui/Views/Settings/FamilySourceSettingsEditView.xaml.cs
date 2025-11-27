using System.Windows.Controls;

namespace Bim.FamilyManager.Ui.Views.Settings;

/// <summary>
///     Represents the WPF user control for editing a family source in the settings UI.
/// </summary>
/// <remarks>
///     This control provides the interaction logic for FamilySourceSettingsEditView.xaml, allowing users to modify family
///     source settings.
/// </remarks>
public partial class FamilySourceSettingsEditView : UserControl
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySourceSettingsEditView" /> class.
    /// </summary>
    /// <remarks>
    ///     Calls <see cref="InitializeComponent" /> to set up the control and its bindings.
    /// </remarks>
    public FamilySourceSettingsEditView()
    {
        InitializeComponent();
    }
}
