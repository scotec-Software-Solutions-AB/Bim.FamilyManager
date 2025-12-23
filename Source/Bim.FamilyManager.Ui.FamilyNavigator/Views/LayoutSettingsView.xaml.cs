using System.Windows.Controls;

namespace Bim.FamilyManager.Ui.FamilyNavigator.Views;

/// <summary>
///     Represents the view for configuring display settings in the application.
/// </summary>
/// <remarks>
///     This class is a WPF UserControl that provides the user interface for managing display-related settings.
///     It is associated with the <see cref="Bim.FamilyManager.ViewModels.DisplaySettingsViewModel" />
///     through the MVVM pattern.
/// </remarks>
public partial class LayoutSettingsView : UserControl
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LayoutSettingsView" /> class.
    /// </summary>
    /// <remarks>
    ///     This constructor sets up the view by initializing its components. It is typically used
    ///     in conjunction with the WPF framework to load and display the user interface for managing
    ///     display settings.
    /// </remarks>
    public LayoutSettingsView()
    {
        InitializeComponent();
    }
}
