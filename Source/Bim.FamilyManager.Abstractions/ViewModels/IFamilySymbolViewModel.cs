using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels;

/// <summary>
///     Represents the view model for a Revit family symbol, providing access to its associated data and properties.
/// </summary>
/// <remarks>
///     This interface defines the contract for view models that encapsulate the logic and data associated with Revit
///     family symbols.
///     Implementations of this interface should provide access to the underlying family symbol and its metadata, such as
///     its name.
/// </remarks>
public interface IFamilySymbolViewModel : IViewModel
{
    /// <summary>
    ///     Gets the Revit family symbol associated with this view model.
    /// </summary>
    /// <remarks>
    ///     The <see cref="IRevitFamilySymbol" /> represents a specific type or variation within a Revit family,
    ///     providing access to its metadata, such as its name and associated family.
    ///     This property allows access to the underlying Revit family symbol for further operations or data retrieval.
    /// </remarks>
    public IRevitFamilySymbol FamilySymbol { get; }

    /// <summary>
    ///     Gets the name of the Revit family symbol represented by this view model.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the associated Revit family symbol.
    /// </value>
    /// <remarks>
    ///     The <see cref="Name" /> property provides a user-friendly identifier for the Revit family symbol,
    ///     typically used for display purposes or sorting within collections of symbols.
    /// </remarks>
    public string Name { get; }
}
