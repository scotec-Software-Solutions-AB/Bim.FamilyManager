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
    private readonly string _rootPath;
    private readonly ILogger? _logger;
    private Dictionary<string, List<string>>? _folderFileMap;
    private Dictionary<string, List<string>>? _descriptionFileMap;

    public DirectoryFileCache(string rootPath, ILogger? logger = null)
    {
        _rootPath = rootPath;
        _logger = logger;
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
        if (_folderFileMap != null && _descriptionFileMap != null)
        {
            return;
        }

        _folderFileMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        _descriptionFileMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

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
                    _logger?.LogWarning(e, "Failed to enumerate subdirectories. Root: {RootPath}", _rootPath);
                }
            }

            foreach (var dir in directories)
            {
                List<string> familyFiles;
                try
                {
                    familyFiles = System.IO.Directory.GetFiles(dir, "*.rfa", SearchOption.TopDirectoryOnly).ToList();
                }
                catch (Exception e)
                {
                    _logger?.LogWarning(e, "Failed to enumerate family files. Directory: {Directory}", dir);
                    familyFiles = [];
                }

                _folderFileMap[dir] = familyFiles;

                List<string> descriptionFiles;
                try
                {
                    descriptionFiles = System.IO.Directory.GetFiles(dir, "*.yaml", SearchOption.TopDirectoryOnly).ToList();
                }
                catch (Exception e)
                {
                    _logger?.LogWarning(e, "Failed to enumerate description files. Directory: {Directory}", dir);
                    descriptionFiles = [];
                }

                _descriptionFileMap[dir] = descriptionFiles;
            }
        }, cancellationToken);
    }

    /// <summary>
    ///     Resets the cache so that the next call to <see cref="InitializeAsync" /> performs a fresh scan.
    /// </summary>
    /// <remarks>
    ///     Call this method before re-initializing the cache, for example after a reload is triggered.
    /// </remarks>
    public void Reset()
    {
        _folderFileMap = null;
        _descriptionFileMap = null;
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

        // Derive subfolders from the cached directory keys — no disk access required.
        return _folderFileMap.Keys
                             .Where(k => string.Equals(
                                 Path.GetDirectoryName(k),
                                 directory,
                                 StringComparison.OrdinalIgnoreCase));
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
        if (_folderFileMap == null)
        {
            return [];
        }

        var dirPrefix = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (includeSubfolders)
        {
            // Match the directory itself and all descendants.
            return _folderFileMap
                   .Where(kvp => string.Equals(kvp.Key, directory, StringComparison.OrdinalIgnoreCase)
                                 || kvp.Key.StartsWith(dirPrefix, StringComparison.OrdinalIgnoreCase))
                   .SelectMany(kvp => kvp.Value);
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
    ///     This method serves description files from the in-memory cache populated during <see cref="InitializeAsync" />.
    ///     No additional disk access is performed after the cache has been initialized.
    /// </remarks>
    public IEnumerable<string> GetDescriptionFiles(string directory, bool includeSubfolders)
    {
        if (_descriptionFileMap == null)
        {
            return [];
        }

        var dirPrefix = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (includeSubfolders)
        {
            // Match the directory itself and all descendants.
            return _descriptionFileMap
                   .Where(kvp => string.Equals(kvp.Key, directory, StringComparison.OrdinalIgnoreCase)
                                 || kvp.Key.StartsWith(dirPrefix, StringComparison.OrdinalIgnoreCase))
                   .SelectMany(kvp => kvp.Value);
        }

        return _descriptionFileMap.TryGetValue(directory, out var files) ? files : Enumerable.Empty<string>();
    }
}
