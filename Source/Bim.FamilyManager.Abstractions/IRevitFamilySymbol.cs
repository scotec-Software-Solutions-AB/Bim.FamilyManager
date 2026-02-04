using System.IO;

namespace Bim.FamilyManager.Abstractions;

/// <summary>
///     Defines the contract for a type or symbol within a Revit family in Bim.FamilyManager.
/// </summary>
/// <remarks>
///     A Revit family symbol represents a specific variation of a Revit family, characterized by its unique set of
///     parameters and properties.
///     Provides access to the symbol's name and its parent family.
/// </remarks>
public interface IRevitFamilySymbol
{
    /// <summary>
    ///     Gets the name of the Revit family symbol.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the symbol.
    /// </value>
    /// <remarks>
    ///     The name uniquely identifies the symbol within a Revit family.
    /// </remarks>
    string Name { get; }

    /// <summary>
    ///     Gets the Revit family associated with this symbol.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="IRevitFamily" /> representing the parent family.
    /// </value>
    /// <remarks>
    ///     Provides access to the parent Revit family, including its metadata, file information, and other symbols.
    /// </remarks>
    IRevitFamily Family { get; }

    /// <summary>
    ///     Gets a stream containing the preview image for this Revit family symbol (type).
    /// </summary>
    /// <value>
    ///     A <see cref="Stream" /> containing the preview image for the symbol, or <c>null</c> if no preview is available.
    /// </value>
    /// <remarks>
    ///     The returned stream is positioned at the beginning and represents a copy of the preview image data for this symbol.
    ///     If a type-specific preview is not available, a general family preview may be returned instead.
    ///     The caller is responsible for disposing the returned stream.
    /// </remarks>
    Stream? Preview { get; }
}
