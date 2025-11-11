using System.IO;

namespace Bim.FamilyManager.Abstractions;

/// <summary>
///     Represents a folder in the Revit Family Manager hierarchy.
/// </summary>
/// <remarks>
///     A folder can contain subfolders and Revit families. It provides access to its name, path,
///     and collections of subfolders and families.
/// </remarks>
public interface IFolder
{
    /// <summary>
    ///     Gets the name of the folder.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the folder.
    /// </value>
    /// <remarks>
    ///     The name typically corresponds to the directory name of the folder in the file system.
    /// </remarks>
    string Name { get; }

    /// <summary>
    ///     Gets the collection of subfolders contained within the current folder.
    /// </summary>
    /// <value>
    ///     A list of <see cref="IFolder" /> instances representing the subfolders of the current folder.
    /// </value>
    /// <remarks>
    ///     This property provides access to the hierarchical structure of folders in the Revit Family Manager.
    ///     Each subfolder can contain additional subfolders and families, enabling navigation through the folder hierarchy.
    /// </remarks>
    IEnumerable<IFolder> Subfolders { get; }

    /// <summary>
    ///     Gets the collection of Revit families contained within the folder.
    /// </summary>
    /// <value>
    ///     A list of <see cref="IRevitFamily" /> objects representing the Revit families in the folder.
    /// </value>
    /// <remarks>
    ///     This property provides access to all Revit families directly stored in the folder.
    ///     Each family includes metadata, file information, preview image, and associated symbols.
    /// </remarks>
    IEnumerable<IRevitFamily> Families { get; }

    /// <summary>
    ///     Gets a stream representing the preview image of the folder.
    /// </summary>
    /// <remarks>
    ///     The preview image provides a visual representation of the folder's content.
    ///     This property may return <see langword="null" /> if no preview is available.
    /// </remarks>
    Stream? Preview { get; }
}
