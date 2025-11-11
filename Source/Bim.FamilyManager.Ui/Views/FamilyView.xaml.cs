using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.Revit.UI;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Ui.ViewModels;

namespace Bim.FamilyManager.Ui.Views;

/// <summary>
///     Represents the view for displaying and interacting with Revit family data.
/// </summary>
/// <remarks>
///     This class is a WPF UserControl that serves as the UI representation for the
///     <see cref="FamilyViewModel" />.
///     It is typically used in conjunction with the MVVM pattern, where the
///     <see cref="FamilyViewModel" />
///     provides the data and logic for the view.
/// </remarks>
public partial class FamilyView : UserControl
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilyView" /> class.
    /// </summary>
    /// <remarks>
    ///     This constructor sets up the UI components of the FamilyView. It is typically used
    ///     in the context of the MVVM pattern, where the <see cref="FamilyViewModel" />
    ///     serves as the DataContext for this view.
    /// </remarks>
    public FamilyView()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Handles the <see cref="MouseMove" /> event for the <see cref="FamilyView" />.
    /// </summary>
    /// <param name="e">
    ///     The <see cref="MouseEventArgs" /> instance containing the event data.
    /// </param>
    /// <remarks>
    ///     This method overrides the base <see cref="OnMouseMove" /> implementation to enable drag-and-drop functionality
    ///     for Revit family data. When the left mouse button is pressed and the <see cref="DataContext" /> is of type
    ///     <see cref="FamilyViewModel{TLayoutOptions}" />, it initiates a drag-and-drop operation
    ///     using the family data and a <see cref="FamilyDropHandler" />.
    /// </remarks>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (DataContext is IFamilyViewModel familyViewModel && e.LeftButton == MouseButtonState.Pressed)
        {
            UIApplication.DoDragDrop(familyViewModel, familyViewModel.DropHandler);
        }
    }
}
