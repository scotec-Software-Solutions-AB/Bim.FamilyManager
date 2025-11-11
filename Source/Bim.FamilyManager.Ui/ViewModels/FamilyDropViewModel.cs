using Bim.FamilyManager.Abstractions.ViewModels;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Ui.ViewModels;

/// <summary>
///     Represents the view model for handling family drop operations within the Revit Family Manager.
/// </summary>
/// <remarks>
///     This class is responsible for managing the interaction between a family and its symbols during a drag-and-drop
///     operation.
///     It provides properties to access the associated family and the action to be performed upon dropping a symbol.
/// </remarks>
public class FamilyDropViewModel : ViewModel, IFamilyDropViewModel
{
    /// <summary>
    ///     A factory delegate for creating instances of <see cref="FamilyDropViewModel" />.
    /// </summary>
    /// <param name="family">The <see cref="IFamilyViewModel" /> representing the family involved in the drop operation.</param>
    /// <param name="dropAction">
    ///     An <see cref="Action{T}" /> to be executed when a symbol is dropped, where <c>T</c> is
    ///     <see cref="IFamilySymbolViewModel" />.
    /// </param>
    /// <returns>A new instance of <see cref="FamilyDropViewModel" /> initialized with the provided parameters.</returns>
    /// <remarks>
    ///     This factory delegate simplifies the creation of <see cref="FamilyDropViewModel" /> instances by encapsulating the
    ///     required parameters.
    ///     It is typically used in dependency injection scenarios or when dynamically creating view models for drag-and-drop
    ///     operations.
    /// </remarks>
    public delegate FamilyDropViewModel Factory(IFamilyViewModel family, Action<IFamilySymbolViewModel> dropAction);

    private IFamilySymbolViewModel? _selectedSymbol;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilyDropViewModel" /> class.
    /// </summary>
    /// <param name="family">
    ///     The <see cref="IFamilyViewModel" /> representing the family associated with this view model.
    /// </param>
    /// <param name="dropAction">
    ///     An <see cref="Action{T}" /> delegate to be executed when a <see cref="IFamilySymbolViewModel" /> is dropped.
    /// </param>
    /// <remarks>
    ///     This constructor sets up the family and the drop action to manage drag-and-drop operations
    ///     for family symbols within the Revit Family Manager.
    /// </remarks>
    public FamilyDropViewModel(IFamilyViewModel family, Action<IFamilySymbolViewModel> dropAction)
    {
        Family = family;
        DropAction = dropAction;
    }

    /// <summary>
    ///     Gets the family associated with the current drag-and-drop operation.
    /// </summary>
    /// <remarks>
    ///     This property provides access to the <see cref="IFamilyViewModel" /> instance,
    ///     which represents the family being manipulated in the Revit Family Manager.
    ///     It includes details such as the family name, preview image, and its symbols.
    /// </remarks>
    public IFamilyViewModel Family { get; }

    /// <summary>
    ///     Gets the action to be performed when a <see cref="IFamilySymbolViewModel" /> is dropped.
    /// </summary>
    /// <value>
    ///     An <see cref="Action{T}" /> delegate that defines the operation to execute upon dropping a
    ///     <see cref="IFamilySymbolViewModel" />.
    /// </value>
    /// <remarks>
    ///     This property is used to handle the logic associated with drag-and-drop operations for family symbols
    ///     within the Revit Family Manager. The action is invoked when the <see cref="SelectedSymbol" /> property
    ///     is set to a non-null value.
    /// </remarks>
    public Action<IFamilySymbolViewModel> DropAction { get; }

    /// <summary>
    ///     Gets or sets the currently selected <see cref="IFamilySymbolViewModel" /> in the family drop operation.
    /// </summary>
    /// <value>
    ///     The <see cref="IFamilySymbolViewModel" /> that is currently selected, or <c>null</c> if no symbol is selected.
    /// </value>
    /// <remarks>
    ///     When a new symbol is selected, the <see cref="DropAction" /> is automatically invoked with the selected symbol
    ///     as its parameter. This property is typically bound to the UI to reflect the user's selection during
    ///     drag-and-drop operations.
    /// </remarks>
    public IFamilySymbolViewModel? SelectedSymbol
    {
        get => _selectedSymbol;
        set
        {
            SetProperty(ref _selectedSymbol, value);
            if (_selectedSymbol is not null)
            {
                DropAction.Invoke(_selectedSymbol);
            }
        }
    }
}
