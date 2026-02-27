using System.IO;

namespace Bim.FamilyManager.Abstractions;

/// <summary>
///     Defines the contract for a folder in the Bim.FamilyManager hierarchy.
/// </summary>
/// <remarks>
///     Provides access to the folder's name, path, subfolders, families, and preview image. GetFoldersAsync are used to
///     organize
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
    ///     Gets a stream containing the preview image of the folder.
    /// </summary>
    /// <value>
    ///     A <see cref="Stream" /> representing the preview image, or <c>null</c> if no preview is available.
    /// </value>
    /// <remarks>
    ///     The preview image provides a visual representation of the folder's content.
    /// </remarks>
    Stream? Preview { get; }

    /// <summary>
    ///     Asynchronously retrieves the collection of subfolders contained within this folder.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken" /> that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an
    ///     <see cref="IEnumerable{IFolder}" /> representing the subfolders.
    /// </returns>
    /// <remarks>
    ///     This method provides access to the hierarchical structure of folders. Each subfolder can
    ///     contain additional subfolders and families, enabling recursive traversal of the folder hierarchy.
    /// </remarks>
    IAsyncEnumerable<IFolder> GetSubfoldersAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves the collection of Revit families contained within this folder.
    /// </summary>
    /// <param name="includeSubfolders">
    /// A boolean value indicating whether to include families from subfolders in the result.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// An <see cref="IAsyncEnumerable{IRevitFamily}"/> representing the Revit families in the folder.
    /// </returns>
    /// <remarks>
    /// This method provides access to all Revit families directly stored in the folder, including their metadata and symbols.
    /// If <paramref name="includeSubfolders"/> is set to <c>true</c>, families from subfolders will also be included.
    /// </remarks>
    IAsyncEnumerable<IRevitFamily> GetFamiliesAsync(bool includeSubfolders, CancellationToken cancellationToken);
}
