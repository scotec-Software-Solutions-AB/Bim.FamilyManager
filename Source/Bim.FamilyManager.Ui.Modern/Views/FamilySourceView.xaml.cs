using System.Windows.Controls;
using Bim.FamilyManager.Ui.FamilyNavigator.ViewModels;

namespace Bim.FamilyManager.Ui.FamilyNavigator.Views;

/// <summary>
///     Represents the view for displaying and interacting with folders in the Family Manager application.
/// </summary>
/// <remarks>
///     This class is a WPF UserControl that serves as the visual representation of a folder view.
///     It is associated with the <see cref="FolderViewModel" />
///     and is registered in the application's dependency injection container.
/// </remarks>
public partial class FamilySourceView : UserControl
{
    public FamilySourceView()
    {
        InitializeComponent();
    }
}
