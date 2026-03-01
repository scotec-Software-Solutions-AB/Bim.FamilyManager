using System.IO;

namespace Bim.FamilyManager.Source.Directory.Logic;

/// <summary>
///     Provides an in-memory cache for directory files and their folder structure in the local file system.
/// </summary>
/// <remarks>
///     This class efficiently caches and organizes Revit family files (*.rfa) and description files (*.yaml) from a root
///     directory.
///     The cache is initialized once via <see cref="InitializeAsync" />, after which all queries are performed without
///     additional disk access.
///     Folder structure is emulated using directory paths, enabling fast enumeration of folders and files.
///     This approach is highly performant for large directory trees and minimizes repeated file system access.
/// </remarks>
public sealed class DirectoryFileCache
{
    private readonly string _rootPath;
    private HashSet<string>? _allFiles;
    private Dictionary<string, List<string>>? _folderFileMap;

    public DirectoryFileCache(string rootPath)
    {
        _rootPath = rootPath;
    }

    /// <summary>
    ///     Asynchronously initializes the cache by loading all family files and organizing them by folder.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    ///     This method enumerates all directories and family files (*.rfa) under the root path and builds the internal
    ///     dictionaries for fast lookup.
    ///     It should be called once before using any other methods.
    ///     For large directory trees, this operation may take time proportional to the number of files and folders.
    /// </remarks>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_folderFileMap != null && _allFiles != null)
        {
            return;
        }

        _folderFileMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        _allFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await Task.Run(() =>
        {
            var directories = new List<string>();
            if (System.IO.Directory.Exists(_rootPath))
            {
                directories.Add(_rootPath);
                try
                {
                    directories.AddRange(System.IO.Directory.GetDirectories(_rootPath, "*", SearchOption.AllDirectories));
                }
                catch
                {
                    // Ignore errors for inaccessible subdirectories.
                }
            }

            foreach (var dir in directories)
            {
                List<string> files;
                try
                {
                    files = System.IO.Directory.GetFiles(dir, "*.rfa", SearchOption.TopDirectoryOnly).ToList();
                }
                catch
                {
                    files = new List<string>();
                }

                _folderFileMap[dir] = files;
                foreach (var file in files)
                {
                    _allFiles.Add(file);
                }
            }
        }, cancellationToken);
    }

    /// <summary>
    ///     Gets the immediate subfolders under the specified directory.
    /// </summary>
    /// <param name="directory">The parent directory path.</param>
    /// <returns>An enumerable of immediate subfolder paths.</returns>
    /// <remarks>
    ///     This method returns only the next-level subfolders under the given directory.
    ///     The returned paths can be used for recursive folder enumeration.
    /// </remarks>
    public IEnumerable<string> GetImmediateSubfolders(string directory)
    {
        if (_folderFileMap == null)
        {
            return [];
        }

        try
        {
            return System.IO.Directory.Exists(directory)
                ? System.IO.Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly)
                : Enumerable.Empty<string>();
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    ///     Gets the family files (*.rfa) under the specified directory.
    /// </summary>
    /// <param name="directory">The directory path to search.</param>
    /// <param name="includeSubfolders">If true, includes files in all subfolders; otherwise, only direct children.</param>
    /// <returns>An enumerable of family file paths matching the criteria.</returns>
    /// <remarks>
    ///     When <paramref name="includeSubfolders" /> is <c>true</c>, all family files whose paths start with the given
    ///     directory are returned.
    ///     When <c>false</c>, only family files directly under the specified directory (not in subfolders) are returned.
    ///     This method relies on the internal cache and does not make any additional disk access.
    /// </remarks>
    public IEnumerable<string> GetFamilyFiles(string directory, bool includeSubfolders)
    {
        if (_folderFileMap == null || _allFiles == null)
        {
            return [];
        }

        if (includeSubfolders)
        {
            // Only return files under the directory (recursively)
            var dirPrefix = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return _allFiles.Where(f =>
                Path.GetFullPath(f).StartsWith(dirPrefix, StringComparison.OrdinalIgnoreCase));
        }

        return _folderFileMap.TryGetValue(directory, out var files) ? files : Enumerable.Empty<string>();
    }

    /// <summary>
    ///     Gets the description files (*.yaml) under the specified directory.
    /// </summary>
    /// <param name="directory">The directory path to search.</param>
    /// <param name="includeSubfolders">If true, includes files in all subfolders; otherwise, only direct children.</param>
    /// <returns>An enumerable of description file paths matching the criteria.</returns>
    /// <remarks>
    ///     This method performs a file system query for description files (*.yaml) under the specified directory.
    ///     If the directory does not exist or is inaccessible, an empty sequence is returned.
    /// </remarks>
    public IEnumerable<string> GetDescriptionFiles(string directory, bool includeSubfolders)
    {
        try
        {
            var searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return System.IO.Directory.Exists(directory)
                ? System.IO.Directory.GetFiles(directory, "*.yaml", searchOption)
                : Enumerable.Empty<string>();
        }
        catch
        {
            return [];
        }
    }
}
