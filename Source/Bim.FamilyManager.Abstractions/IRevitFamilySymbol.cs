namespace Bim.FamilyManager.Abstractions;

/// <summary>
///     Represents a type or symbol within a Revit family.
/// </summary>
/// <remarks>
///     A Revit family type defines a specific variation of a Revit family,
///     characterized by its unique set of parameters and properties.
///     This interface provides access to the name of the family type.
/// </remarks>
public interface IRevitFamilySymbol
{
    /// <summary>
    ///     Gets the name of the Revit family type.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the family type.
    /// </value>
    /// <remarks>
    ///     The name uniquely identifies the type or symbol within a Revit family.
    /// </remarks>
    string Name { get; }

    /// <summary>
    ///     Gets the Revit family associated with this symbol.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="IRevitFamily" /> representing the family to which this symbol belongs.
    /// </value>
    /// <remarks>
    ///     This property provides access to the parent Revit family, which includes metadata, file information,
    ///     and other related symbols.
    /// </remarks>
    IRevitFamily Family { get; }
}
