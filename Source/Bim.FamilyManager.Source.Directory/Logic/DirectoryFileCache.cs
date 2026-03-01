using System.IO;

namespace Bim.FamilyManager.Source.Directory.Logic;

public sealed class DirectoryFileCache
{
    private readonly string _rootPath;
    private HashSet<string>? _allFiles;
    private Dictionary<string, List<string>>? _folderFileMap;

    public DirectoryFileCache(string rootPath)
    {
        _rootPath = rootPath;
    }

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
                    /* ignore errors for inaccessible subdirs */
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

    public IEnumerable<string> GetImmediateSubfolders(string directory)
    {
        if (_folderFileMap == null)
        {
            return Enumerable.Empty<string>();
        }

        try
        {
            return System.IO.Directory.Exists(directory)
                ? System.IO.Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly)
                : Enumerable.Empty<string>();
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
    }

    public IEnumerable<string> GetFamilyFiles(string directory, bool includeSubfolders)
    {
        if (_folderFileMap == null || _allFiles == null)
        {
            return Enumerable.Empty<string>();
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
            return Enumerable.Empty<string>();
        }
    }
}
