using System.IO;
using Bim.FamilyManager.Core.Abstractions;

namespace Bim.FamilyManager.Core.Logic;

/// <summary>
///     Represents a delegate for asynchronously retrieving subfolders in the Revit Family Manager hierarchy.
/// </summary>
public delegate IAsyncEnumerable<IFolder> GetSubfoldersAsyncDelegate(CancellationToken cancellationToken);

/// <summary>
///     Represents a delegate for asynchronously retrieving families in the Revit Family Manager hierarchy.
/// </summary>
public delegate IAsyncEnumerable<IRevitFamily> GetFamiliesAsyncDelegate(bool includeSubfolders, CancellationToken cancellationToken);

/// <summary>
///     Represents a folder in the Revit Family Manager hierarchy.
/// </summary>
/// <remarks>
///     A folder can contain subfolders and Revit families. This class supports lazy-loading for efficient
///     retrieval of hierarchical data. An optional <see cref="Preview" /> stream may be supplied by the
///     source to show a custom image for this folder in the UI.
/// </remarks>
public class Folder : IFolder
{
    private readonly GetFamiliesAsyncDelegate _families;
    private readonly GetSubfoldersAsyncDelegate _subFolders;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Folder" /> class.
    /// </summary>
    /// <param name="name">The name of the folder.</param>
    /// <param name="subFolders">A function to lazily load the subfolders of this folder.</param>
    /// <param name="families">A function to lazily load the Revit families contained in this folder.</param>
    /// <param name="preview">
    ///     An optional stream containing a source-provided preview image. Pass <c>null</c> when the
    ///     source has no custom image; the UI will fall back to its default folder icon.
    /// </param>
    public Folder(string name, GetSubfoldersAsyncDelegate subFolders, GetFamiliesAsyncDelegate families, Stream? preview = null)
    {
        _subFolders = subFolders;
        _families = families;
        Name = name;
        Preview = preview;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public Stream? Preview { get; }

    /// <inheritdoc />
    public IAsyncEnumerable<IFolder> GetSubfoldersAsync(CancellationToken cancellationToken)
    {
        return _subFolders(cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<IRevitFamily> GetFamiliesAsync(bool includeSubfolders, CancellationToken cancellationToken)
    {
        return _families(includeSubfolders, cancellationToken);
    }
}
