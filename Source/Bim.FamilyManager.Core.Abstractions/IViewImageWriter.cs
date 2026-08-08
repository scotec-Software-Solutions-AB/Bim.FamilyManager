using System.IO;

namespace Bim.FamilyManager.Core.Abstractions;

/// <summary>
///     Defines the contract for writing preview images into a Revit family file stream.
/// </summary>
public interface IViewImageWriter
{
    /// <summary>
    ///     Writes preview images for all family types into the provided <paramref name="documentStream" />.
    /// </summary>
    /// <param name="documentStream">The writable stream of the Revit family document.</param>
    /// <param name="familyPreviewImageName">
    ///     The name of the family-level preview image within
    ///     <paramref name="typePreviewImageStreams" />.
    /// </param>
    /// <param name="typePreviewImageStreams">
    ///     A dictionary mapping each family type name to its corresponding PNG image stream.
    /// </param>
    void WritePreviewImages(Stream documentStream, string familyPreviewImageName,
        IDictionary<string, Stream> typePreviewImageStreams);
}
