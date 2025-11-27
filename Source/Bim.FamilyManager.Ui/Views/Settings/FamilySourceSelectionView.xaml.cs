using System.Windows.Controls;

namespace Bim.FamilyManager.Ui.Views.Settings;

/// <summary>
///     Represents the WPF user control for selecting a family source in the settings UI.
/// </summary>
/// <remarks>
///     This control provides the interaction logic for the FamilySourceSelectionView.xaml, allowing users to choose among
///     available family sources.
/// </remarks>
public partial class FamilySourceSelectionView : UserControl
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySourceSelectionView" /> class.
    /// </summary>
    /// <remarks>
    ///     Calls <see cref="InitializeComponent" /> to set up the control and its bindings.
    /// </remarks>
    public FamilySourceSelectionView()
    {
        InitializeComponent();
    }
}
