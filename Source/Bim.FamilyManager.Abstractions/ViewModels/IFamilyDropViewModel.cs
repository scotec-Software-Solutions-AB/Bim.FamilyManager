using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels;

/// <summary>
///     Defines the view model contract for managing drag-and-drop operations involving Revit family elements in
///     Bim.FamilyManager.
/// </summary>
/// <remarks>
///     Provides properties and actions for facilitating interaction between the user interface and the underlying data or
///     behavior related to Revit family elements during drag-and-drop operations.
/// </remarks>
public interface IFamilyDropViewModel : IViewModel
{
    /// <summary>
    ///     Gets the view model representing the Revit family associated with this drop operation.
    /// </summary>
    /// <remarks>
    ///     Provides access to the <see cref="IFamilyViewModel" /> instance, which contains details about the Revit family, its
    ///     symbols, and related commands.
    /// </remarks>
    IFamilyViewModel Family { get; }

    /// <summary>
    ///     Gets the action to execute when a family symbol is dropped.
    /// </summary>
    /// <remarks>
    ///     The action takes an <see cref="IFamilySymbolViewModel" /> as a parameter, representing the symbol being dropped.
    /// </remarks>
    Action<IFamilySymbolViewModel> DropAction { get; }

    /// <summary>
    ///     Gets or sets the currently selected family symbol in the view model.
    /// </summary>
    /// <value>
    ///     The selected <see cref="IFamilySymbolViewModel" /> representing a Revit family symbol.
    /// </value>
    /// <remarks>
    ///     Used to bind the selected item in the user interface to the corresponding family symbol in the view model.
    /// </remarks>
    IFamilySymbolViewModel? SelectedSymbol { get; set; }
}
