using System.Runtime.Loader;
using System.Windows;
using System.Windows.Controls;
using Bim.FamilyManager.Ui.FamilyExplorer.ViewModels;
using Scotec.Revit.Wpf;

namespace Bim.FamilyManager.Ui.FamilyExplorer.Views;

/// <summary>
///     Represents the view for managing Revit families within the application.
/// </summary>
/// <remarks>
///     This class is a WPF UserControl that provides the user interface for interacting with Revit families.
///     It is designed to be used in conjunction with the FamilyManagerViewModel and integrates with the application's MVVM
///     architecture.
/// </remarks>
public partial class FamilyManagerView : RevitUserControl
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilyManagerView" /> class.
    /// </summary>
    /// <remarks>
    ///     This constructor sets up the interaction logic for the FamilyManagerView by initializing its components.
    /// </remarks>
    public FamilyManagerView()
    {
        InitializeComponent();
    }
    
    /// <summary>
    ///     Handles the <see cref="ComboBox.SelectionChanged" /> event for the ComboBox in the Family Manager view.
    /// </summary>
    /// <param name="sender">The source of the event, typically the ComboBox.</param>
    /// <param name="e">The event data containing information about the selection change.</param>
    /// <remarks>
    ///     This method ensures that when the selection in the ComboBox changes, the associated
    ///     <see cref="FamilyManagerViewModel" /> is notified to refresh its state. The method assumes
    ///     that the <see cref="DataContext" /> of the view is an instance of <see cref="FamilyManagerViewModel" />.
    /// </remarks>
    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is FamilyManagerViewModel familyManager)
        {
            familyManager.Refresh();
        }
    }

    /// <summary>
    ///     Handles the click event for the settings button in the Family Manager view.
    /// </summary>
    /// <param name="sender">The source of the event, typically the settings button.</param>
    /// <param name="e">The event data associated with the click event.</param>
    /// <remarks>
    ///     This method retrieves the <see cref="FamilyManagerViewModel" /> from the
    ///     <see cref="FrameworkElement.DataContext" />
    ///     and invokes the <see cref="FamilyManagerViewModel.SettingsManagerWindowFactory" /> to create and display
    ///     the settings manager window.
    /// </remarks>
    private void OnSettingsButtonClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is FamilyManagerViewModel viewModel)
        {
            var window = viewModel.SettingsManagerWindowFactory();

            window.ShowDialog();
        }
    }
}
