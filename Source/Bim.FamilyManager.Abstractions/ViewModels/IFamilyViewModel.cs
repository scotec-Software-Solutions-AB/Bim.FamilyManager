using Autodesk.Revit.UI;
using System.Windows.Input;

namespace Bim.FamilyManager.Abstractions.ViewModels;

/// <summary>
///     Represents a view model for managing Revit family items, providing properties and commands
///     to interact with and manipulate Revit families within the application.
/// </summary>
public interface IFamilyViewModel : IFamilyManagerItemViewModel
{
    /// <summary>
    ///     Gets a value indicating whether the associated Revit family is currently loaded in the active document.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the Revit family is loaded in the active document; otherwise, <c>false</c>.
    /// </value>
    public bool IsLoadedInDocument { get; }

    /// <summary>
    ///     Gets the Revit family associated with this view model.
    /// </summary>
    /// <remarks>
    ///     The <see cref="IRevitFamily" /> provides access to metadata, file information, preview image,
    ///     and associated symbols of the Revit family. This property allows interaction with the
    ///     underlying Revit family data within the application.
    /// </remarks>
    public IRevitFamily Family { get; }

    /// <summary>
    ///     Gets the command used to initiate the editing of a Revit family.
    /// </summary>
    /// <remarks>
    ///     This command is typically bound to a user interface element, such as a button,
    ///     allowing users to open and modify the selected Revit family within the application.
    /// </remarks>
    public ICommand EditFamilyCommand { get; }

    /// <summary>
    ///     Gets the command used to load a Revit family into the current document.
    /// </summary>
    /// <remarks>
    ///     This command is typically bound to a user interface element, such as a button,
    ///     and is executed to load the associated Revit family into the active Revit document.
    /// </remarks>
    public ICommand LoadFamilyCommand { get; }

    /// <summary>
    ///     Gets the command used to remove a Revit family from the application.
    /// </summary>
    /// <remarks>
    ///     This command is typically bound to a UI element, such as a button, to allow users
    ///     to remove a family from the current context. The implementation of the command
    ///     should handle the necessary logic for removing the family, such as updating the
    ///     view model and notifying the application of the change.
    /// </remarks>
    public ICommand RemoveFamilyCommand { get; }

    /// <summary>
    ///     Gets the collection of family symbols associated with the Revit family.
    ///     Each symbol represents a specific variation or type within the family.
    /// </summary>
    /// <value>
    ///     A list of <see cref="IFamilySymbolViewModel" /> instances, where each instance
    ///     provides details about a specific family symbol.
    /// </value>
    public IList<IFamilySymbolViewModel> Symbols { get; }

    IControllableDropHandler DropHandler { get; }

    public string Product { get; }
    
    public string ProductVersion { get; }
    
    public DateTime Updated { get; }
}
