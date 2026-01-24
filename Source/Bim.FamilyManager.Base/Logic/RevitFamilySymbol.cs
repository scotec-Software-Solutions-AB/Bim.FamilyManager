using System.IO;
using Bim.FamilyManager.Abstractions;
using Scotec.Revit.RevitFamily;

namespace Bim.FamilyManager.Base.Logic;

/// <summary>
///     Represents a specific symbol or type within a Revit family.
/// </summary>
/// <remarks>
///     This class provides access to the metadata and associated family for a Revit family symbol.
///     It encapsulates the details of a family type, such as its name and the parent family it belongs to.
/// </remarks>
public sealed class RevitFamilySymbol : IRevitFamilySymbol
{
    private readonly RevitFamilySymbolInfo _symbolInfo;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitFamilySymbol" /> class.
    /// </summary>
    /// <param name="symbolInfo">
    ///     The metadata information associated with the Revit family symbol.
    /// </param>
    /// <param name="family">
    ///     The parent <see cref="IRevitFamily" /> to which this symbol belongs.
    /// </param>
    /// <remarks>
    ///     This constructor sets up the Revit family symbol by associating it with its metadata and parent family.
    /// </remarks>
    public RevitFamilySymbol(RevitFamilySymbolInfo symbolInfo, IRevitFamily family)
    {
        _symbolInfo = symbolInfo;
        Family = family;
    }

    /// <summary>
    ///     Gets the Revit family associated with this symbol.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="IRevitFamily" /> representing the parent family
    ///     to which this symbol belongs.
    /// </value>
    /// <remarks>
    ///     This property provides access to the parent family of the current symbol,
    ///     allowing retrieval of metadata, symbols, and other details associated with
    ///     the family.
    /// </remarks>
    public IRevitFamily Family { get; }

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
    public Stream? Preview => Family.GetTypePreview(_symbolInfo.Title);

    /// <summary>
    ///     Gets the name of the Revit family symbol.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the title of the Revit family symbol.
    /// </value>
    /// <remarks>
    ///     This property retrieves the title of the Revit family symbol as defined in the underlying
    ///     <see cref="RevitFamilySymbolInfo" />. The name uniquely identifies the type or symbol within the family.
    /// </remarks>
    public string Name => _symbolInfo.Title;
}
