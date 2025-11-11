using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels;

/// <summary>
///     Represents the view model for managing drag-and-drop operations involving Revit family elements.
/// </summary>
/// <remarks>
///     This interface defines the contract for a view model that facilitates the interaction between
///     the user interface and the underlying data or behavior related to Revit family elements.
///     It provides properties and actions necessary for handling drag-and-drop functionality.
/// </remarks>
public interface IFamilyDropViewModel : IViewModel
{
    /// <summary>
    ///     Gets the view model representing the Revit family associated with this drop operation.
    /// </summary>
    /// <remarks>
    ///     This property provides access to the <see cref="IFamilyViewModel" /> instance, which contains
    ///     details about the Revit family, such as its symbols, commands for editing or loading the family,
    ///     and its loaded state in the document.
    /// </remarks>
    public IFamilyViewModel Family { get; }

    /// <summary>
    ///     Gets the action to be executed when a family symbol is dropped.
    /// </summary>
    /// <remarks>
    ///     This property defines the behavior to be performed during a drag-and-drop operation
    ///     involving a family symbol. The action takes an <see cref="IFamilySymbolViewModel" />
    ///     as a parameter, representing the symbol being dropped.
    /// </remarks>
    public Action<IFamilySymbolViewModel> DropAction { get; }

    /// <summary>
    ///     Gets or sets the currently selected family symbol in the view model.
    /// </summary>
    /// <value>
    ///     The selected <see cref="IFamilySymbolViewModel" /> representing a Revit family symbol.
    /// </value>
    /// <remarks>
    ///     This property is used to bind the selected item in the user interface, such as a list or grid,
    ///     to the corresponding family symbol in the view model. It facilitates interaction between
    ///     the user interface and the underlying data.
    /// </remarks>
    public IFamilySymbolViewModel? SelectedSymbol { get; set; }
}
