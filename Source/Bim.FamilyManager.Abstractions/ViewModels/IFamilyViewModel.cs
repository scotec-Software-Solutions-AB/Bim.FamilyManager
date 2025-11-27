using System.Windows.Input;
using Autodesk.Revit.UI;

namespace Bim.FamilyManager.Abstractions.ViewModels;

/// <summary>
///     Defines the view model contract for managing Revit family items in Bim.FamilyManager.
/// </summary>
/// <remarks>
///     Provides properties and commands for interacting with and manipulating Revit families, including access to family
///     metadata, symbols, and document state.
/// </remarks>
public interface IFamilyViewModel : IFamilyManagerItemViewModel
{
    /// <summary>
    ///     Gets a value indicating whether the associated Revit family is currently loaded in the active document.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the Revit family is loaded in the active document; otherwise, <c>false</c>.
    /// </value>
    bool IsLoadedInDocument { get; }

    /// <summary>
    ///     Gets the Revit family associated with this view model.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="IRevitFamily" /> providing access to metadata, file information, preview image, and
    ///     symbols.
    /// </value>
    IRevitFamily Family { get; }

    /// <summary>
    ///     Gets the command used to initiate editing of a Revit family.
    /// </summary>
    /// <value>
    ///     An <see cref="ICommand" /> bound to UI elements for opening and modifying the selected Revit family.
    /// </value>
    ICommand EditFamilyCommand { get; }

    /// <summary>
    ///     Gets the command used to load a Revit family into the current document.
    /// </summary>
    /// <value>
    ///     An <see cref="ICommand" /> bound to UI elements for loading the associated Revit family into the active document.
    /// </value>
    ICommand LoadFamilyCommand { get; }

    /// <summary>
    ///     Gets the command used to remove a Revit family from the application.
    /// </summary>
    /// <value>
    ///     An <see cref="ICommand" /> bound to UI elements for removing the family from the current context.
    /// </value>
    ICommand RemoveFamilyCommand { get; }

    /// <summary>
    ///     Gets the collection of family symbols associated with the Revit family.
    /// </summary>
    /// <value>
    ///     A list of <see cref="IFamilySymbolViewModel" /> instances, each representing a specific symbol or type within the
    ///     family.
    /// </value>
    IList<IFamilySymbolViewModel> Symbols { get; }

    /// <summary>
    ///     Gets the drop handler for drag-and-drop operations involving the family.
    /// </summary>
    /// <value>
    ///     An <see cref="IControllableDropHandler" /> instance for managing drop interactions.
    /// </value>
    IControllableDropHandler DropHandler { get; }

    /// <summary>
    ///     Gets the product name associated with the Revit family.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the product name.
    /// </value>
    string Product { get; }

    /// <summary>
    ///     Gets the product version associated with the Revit family.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the product version.
    /// </value>
    string ProductVersion { get; }

    /// <summary>
    ///     Gets the date and time when the Revit family was last updated.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime" /> representing the last update timestamp.
    /// </value>
    DateTime Updated { get; }
}
