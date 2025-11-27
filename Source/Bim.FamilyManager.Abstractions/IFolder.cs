using System.IO;

namespace Bim.FamilyManager.Abstractions;

/// <summary>
///     Defines the contract for a folder in the Bim.FamilyManager hierarchy.
/// </summary>
/// <remarks>
///     Provides access to the folder's name, path, subfolders, families, and preview image. Folders are used to organize
///     Revit families in a hierarchical structure.
/// </remarks>
public interface IFolder
{
    /// <summary>
    ///     Gets the name of the folder.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the folder name.
    /// </value>
    /// <remarks>
    ///     Typically corresponds to the directory name in the file system.
    /// </remarks>
    string Name { get; }

    /// <summary>
    ///     Gets the collection of subfolders contained within this folder.
    /// </summary>
    /// <value>
    ///     An <see cref="IEnumerable{IFolder}" /> representing the subfolders.
    /// </value>
    /// <remarks>
    ///     Provides access to the hierarchical structure of folders. Each subfolder can contain additional subfolders and
    ///     families.
    /// </remarks>
    IEnumerable<IFolder> Subfolders { get; }

    /// <summary>
    ///     Gets the collection of Revit families contained within this folder.
    /// </summary>
    /// <value>
    ///     An <see cref="IEnumerable{IRevitFamily}" /> representing the Revit families in the folder.
    /// </value>
    /// <remarks>
    ///     Provides access to all Revit families directly stored in the folder, including their metadata and symbols.
    /// </remarks>
    IEnumerable<IRevitFamily> Families { get; }

    /// <summary>
    ///     Gets a stream containing the preview image of the folder.
    /// </summary>
    /// <value>
    ///     A <see cref="Stream" /> representing the preview image, or <c>null</c> if no preview is available.
    /// </value>
    /// <remarks>
    ///     The preview image provides a visual representation of the folder's content.
    /// </remarks>
    Stream? Preview { get; }
}
