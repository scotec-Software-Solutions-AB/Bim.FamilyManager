using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels;

/// <summary>
///     Defines the view model contract for a Revit family symbol in Bim.FamilyManager.
/// </summary>
/// <remarks>
///     Provides access to the underlying Revit family symbol and its metadata, such as its name.
///     Implementations encapsulate the logic and data associated with Revit family symbols for use in the UI and related
///     operations.
/// </remarks>
public interface IFamilySymbolViewModel : IViewModel
{
    /// <summary>
    ///     Gets the Revit family symbol associated with this view model.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="IRevitFamilySymbol" /> representing the specific type or variation within a Revit family.
    /// </value>
    /// <remarks>
    ///     Provides access to the symbol's metadata and its parent family.
    /// </remarks>
    IRevitFamilySymbol FamilySymbol { get; }

    /// <summary>
    ///     Gets the name of the Revit family symbol represented by this view model.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> containing the name of the associated Revit family symbol.
    /// </value>
    /// <remarks>
    ///     Used as a user-friendly identifier for display and sorting within collections of symbols.
    /// </remarks>
    string Name { get; }
}
