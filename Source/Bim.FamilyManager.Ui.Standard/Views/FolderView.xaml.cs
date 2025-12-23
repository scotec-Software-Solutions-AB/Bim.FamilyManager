using System.Windows.Controls;
using Bim.FamilyManager.Ui.FamilyExplorer.ViewModels;

namespace Bim.FamilyManager.Ui.FamilyExplorer.Views;

/// <summary>
///     Represents the view for displaying and interacting with folders in the Family Manager application.
/// </summary>
/// <remarks>
///     This class is a WPF UserControl that serves as the visual representation of a folder view.
///     It is associated with the <see cref="FolderViewModel" />
///     and is registered in the application's dependency injection container.
/// </remarks>
public partial class FolderView : UserControl
{
    public FolderView()
    {
        InitializeComponent();
    }
}
