using System.IO;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger? _logger;
    private readonly string _rootPath;
    private HashSet<string>? _allDescriptionFiles;
    private HashSet<string>? _allFiles;
    private Dictionary<string, List<string>>? _folderDescriptionMap;
    private Dictionary<string, List<string>>? _folderFileMap;
    private Dictionary<string, List<string>>? _subfolderMap;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DirectoryFileCache" /> class.
    /// </summary>
    /// <param name="rootPath">The root directory whose folder structure and files are cached.</param>
    /// <param name="logger">An optional logger used to report inaccessible directories during initialization.</param>
    public DirectoryFileCache(string rootPath, ILogger? logger = null)
    {
        _rootPath = rootPath;
        _logger = logger;
    }

    /// <summary>
    ///     Asynchronously initializes the cache by loading all family and description files and organizing them by folder.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    ///     This method enumerates all directories, family files (*.rfa) and description files (*.yaml) under the root path
    ///     and builds the internal dictionaries for fast lookup.
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
        _folderDescriptionMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        _allDescriptionFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _subfolderMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

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
                catch (Exception e)
                {
                    // Continue with the directories enumerated so far.
                    _logger?.LogWarning(e, "Failed to enumerate the subdirectories of the family source root. Root: {RootPath}", _rootPath);
                }
            }

            foreach (var dir in directories)
            {
                _subfolderMap[dir] = new List<string>();
            }

            foreach (var dir in directories)
            {
                if (!string.Equals(dir, _rootPath, StringComparison.OrdinalIgnoreCase))
                {
                    var parent = Path.GetDirectoryName(dir);
                    if (parent != null && _subfolderMap.TryGetValue(parent, out var siblings))
                    {
                        siblings.Add(dir);
                    }
                }

                _folderFileMap[dir] = CollectFiles(dir, "*.rfa", _allFiles);
                _folderDescriptionMap[dir] = CollectFiles(dir, "*.yaml", _allDescriptionFiles);
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
    ///     This method relies on the internal cache and does not make any additional disk access.
    /// </remarks>
    public IEnumerable<string> GetImmediateSubfolders(string directory)
    {
        if (_subfolderMap == null)
        {
            return [];
        }

        return _subfolderMap.TryGetValue(directory, out var subfolders) ? subfolders : Enumerable.Empty<string>();
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
        return GetCachedFiles(_folderFileMap, _allFiles, directory, includeSubfolders);
    }

    /// <summary>
    ///     Gets the description files (*.yaml) under the specified directory.
    /// </summary>
    /// <param name="directory">The directory path to search.</param>
    /// <param name="includeSubfolders">If true, includes files in all subfolders; otherwise, only direct children.</param>
    /// <returns>An enumerable of description file paths matching the criteria.</returns>
    /// <remarks>
    ///     When <paramref name="includeSubfolders" /> is <c>true</c>, all description files whose paths start with the given
    ///     directory are returned.
    ///     When <c>false</c>, only description files directly under the specified directory (not in subfolders) are returned.
    ///     This method relies on the internal cache and does not make any additional disk access.
    /// </remarks>
    public IEnumerable<string> GetDescriptionFiles(string directory, bool includeSubfolders)
    {
        return GetCachedFiles(_folderDescriptionMap, _allDescriptionFiles, directory, includeSubfolders);
    }

    /// <summary>
    ///     Collects the files matching the given pattern that are direct children of the specified directory.
    /// </summary>
    /// <param name="directory">The directory to enumerate.</param>
    /// <param name="searchPattern">The file search pattern, e.g. "*.rfa".</param>
    /// <param name="allFiles">The set that accumulates all collected files across directories.</param>
    /// <returns>The list of files found directly in the directory, or an empty list if the directory is inaccessible.</returns>
    private List<string> CollectFiles(string directory, string searchPattern, HashSet<string> allFiles)
    {
        List<string> files;
        try
        {
            files = System.IO.Directory.GetFiles(directory, searchPattern, SearchOption.TopDirectoryOnly).ToList();
        }
        catch (Exception e)
        {
            // Treat an inaccessible directory as empty and continue with the remaining directories.
            _logger?.LogWarning(e, "Failed to enumerate the files of a family source directory. Directory: {Directory}, Pattern: {Pattern}", directory,
                searchPattern);
            files = new List<string>();
        }

        foreach (var file in files)
        {
            allFiles.Add(file);
        }

        return files;
    }

    /// <summary>
    ///     Serves a file query from the cached per-folder map or, for recursive queries, from the cached file set.
    /// </summary>
    /// <param name="folderMap">The map of directories to their direct child files.</param>
    /// <param name="allFiles">The set of all cached files.</param>
    /// <param name="directory">The directory path to search.</param>
    /// <param name="includeSubfolders">If true, includes files in all subfolders; otherwise, only direct children.</param>
    /// <returns>An enumerable of file paths matching the criteria.</returns>
    private static IEnumerable<string> GetCachedFiles(Dictionary<string, List<string>>? folderMap, HashSet<string>? allFiles, string directory,
                                                      bool includeSubfolders)
    {
        if (folderMap == null || allFiles == null)
        {
            return [];
        }

        if (includeSubfolders)
        {
            // Only return files under the directory (recursively)
            var dirPrefix = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return allFiles.Where(f =>
                Path.GetFullPath(f).StartsWith(dirPrefix, StringComparison.OrdinalIgnoreCase));
        }

        return folderMap.TryGetValue(directory, out var files) ? files : Enumerable.Empty<string>();
    }
}
