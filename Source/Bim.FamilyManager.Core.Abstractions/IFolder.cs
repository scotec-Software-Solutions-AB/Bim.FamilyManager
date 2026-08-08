using System.IO;

namespace Bim.FamilyManager.Core.Abstractions;

/// <summary>
///     Defines the contract for a folder in the Bim.FamilyManager hierarchy.
/// </summary>
/// <remarks>
///     Provides access to the folder's name, optional preview image, subfolders, and families.
///     The preview image is source-specific — a source may supply a custom image for a folder
///     (e.g. a PNG file placed by the user in a directory). When <c>null</c>, the UI falls back
///     to a default folder icon.
/// </remarks>
public interface IFolder
{
    /// <summary>
    ///     Gets the name of the folder.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets an optional stream containing a source-provided preview image for this folder.
    /// </summary>
    /// <value>
    ///     A <see cref="Stream" /> containing image data supplied by the source, or <c>null</c> if
    ///     the source has no custom image for this folder. When <c>null</c> the UI layer uses its
    ///     own default folder icon as a fallback.
    /// </value>
    Stream? Preview { get; }

    /// <summary>
    ///     Asynchronously retrieves the collection of subfolders contained within this folder.
    /// </summary>
    IAsyncEnumerable<IFolder> GetSubfoldersAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously retrieves the collection of Revit families contained within this folder.
    /// </summary>
    IAsyncEnumerable<IRevitFamily> GetFamiliesAsync(bool includeSubfolders, CancellationToken cancellationToken);
}
