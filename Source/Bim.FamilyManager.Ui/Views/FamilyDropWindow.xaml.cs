using Autodesk.Revit.UI;
using Bim.FamilyManager.Ui.ViewModels;
using Scotec.Revit.Wpf;

namespace Bim.FamilyManager.Ui.Views;

/// <summary>
///     Interaction logic for FamilyDropWindow.xaml.
///     This class represents the code-behind for the FamilyDropWindow view, which is a WPF window
///     designed to facilitate drag-and-drop operations for Revit families and their symbols.
///     It is tightly coupled with the <see cref="FamilyDropViewModel" /> to provide a seamless
///     interaction between the UI and the underlying data.
///     The window is initialized with a specific <see cref="FamilyDropViewModel" /> instance,
///     which serves as its data context, and integrates with the Revit API through the
///     <see cref="UIControlledApplication" /> parameter.
/// </summary>
public partial class FamilyDropWindow : RevitWindow
{
    /// <summary>
    ///     Represents a factory delegate for creating instances of <see cref="FamilyDropWindow" />.
    /// </summary>
    /// <param name="family">
    ///     The <see cref="FamilyDropViewModel" /> instance that serves as the data context for the created
    ///     <see cref="FamilyDropWindow" />.
    /// </param>
    /// <returns>
    ///     A new instance of <see cref="FamilyDropWindow" /> initialized with the specified <see cref="FamilyDropViewModel" />
    ///     .
    /// </returns>
    public delegate FamilyDropWindow Factory(FamilyDropViewModel family);

    private readonly FamilyDropViewModel _family;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilyDropWindow" /> class.
    /// </summary>
    /// <param name="family">
    ///     The <see cref="FamilyDropViewModel" /> instance that serves as the data context for this window.
    /// </param>
    /// <param name="revitApplication">
    ///     The <see cref="UIControlledApplication" /> instance representing the Revit application context.
    /// </param>
    /// <remarks>
    ///     This constructor sets up the <see cref="FamilyDropWindow" /> with the specified view model and integrates it
    ///     with the Revit API. It also initializes the WPF components and binds the provided view model to the window's
    ///     data context.
    /// </remarks>
    public FamilyDropWindow(FamilyDropViewModel family, UIControlledApplication revitApplication) : base(revitApplication)
    {
        _family = family;
        DataContext = family;

        InitializeComponent();
    }
}
