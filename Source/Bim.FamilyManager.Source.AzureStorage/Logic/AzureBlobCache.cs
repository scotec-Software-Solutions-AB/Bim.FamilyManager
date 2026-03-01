using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Bim.FamilyManager.Source.AzureStorage.Logic;

/// <summary>
/// Provides an in-memory cache for Azure Blob Storage items and their folder structure.
/// </summary>
/// <remarks>
/// This class efficiently caches and organizes blobs from an Azure Blob container for fast, in-memory lookup and enumeration.
/// The cache is initialized once via <see cref="InitializeAsync"/>, after which all queries are performed without additional network calls.
/// Folder prefixes are determined by the position of '/' in blob names, emulating a hierarchical structure.
/// This approach is highly performant for large containers and minimizes Azure API usage.
/// </remarks>
public sealed class AzureBlobCache
{
    private readonly BlobContainerClient _blobContainerClient;
    private Dictionary<string, BlobItem>? _blobItemCache;
    private Dictionary<string, List<string>>? _folderBlobMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobCache"/> class for the specified blob container.
    /// </summary>
    /// <param name="blobContainerClient">The Azure <see cref="BlobContainerClient"/> to cache blobs from.</param>
    /// <remarks>
    /// The cache is not populated until <see cref="InitializeAsync"/> is called.
    /// </remarks>
    public AzureBlobCache(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    /// <summary>
    /// Asynchronously initializes the cache by loading all blobs and organizing them by folder prefix.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method enumerates all blobs in the container and builds the internal dictionaries for fast lookup.
    /// It should be called once before using any other methods.
    /// For large containers, this operation may take time proportional to the number of blobs.
    /// </remarks>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_blobItemCache != null && _folderBlobMap != null)
        {
            return;
        }

        _blobItemCache = new Dictionary<string, BlobItem>(StringComparer.OrdinalIgnoreCase);
        _folderBlobMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        await foreach (var blob in _blobContainerClient.GetBlobsAsync(cancellationToken: cancellationToken))
        {
            _blobItemCache[blob.Name] = blob;

            var folderPrefix = GetFolderPrefix(blob.Name);
            if (!_folderBlobMap.TryGetValue(folderPrefix, out var list))
            {
                list = [];
                _folderBlobMap[folderPrefix] = list;
            }

            list.Add(blob.Name);
        }
    }

    /// <summary>
    /// Gets the names of blobs under the specified prefix.
    /// </summary>
    /// <param name="prefix">The blob prefix to filter by (e.g., folder path).</param>
    /// <param name="includeSubfolders">If true, includes blobs in all subfolders; otherwise, only direct children.</param>
    /// <returns>An enumerable of blob names matching the criteria.</returns>
    /// <remarks>
    /// When <paramref name="includeSubfolders"/> is <c>true</c>, all blobs whose names start with the given prefix are returned.
    /// When <c>false</c>, only blobs directly under the specified folder (not in subfolders) are returned.
    /// This method relies on the internal cache and does not make any network calls.
    /// </remarks>
    public IEnumerable<string> GetBlobNamesByPrefix(string prefix, bool includeSubfolders)
    {
        if (_blobItemCache == null || _folderBlobMap == null)
        {
            return [];
        }

        if (includeSubfolders)
        {
            return _blobItemCache.Keys.Where(name => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        return _folderBlobMap.TryGetValue(prefix, out var names) ? names : Enumerable.Empty<string>();
    }

    /// <summary>
    /// Gets the <see cref="BlobItem"/> for the specified blob name.
    /// </summary>
    /// <param name="blobName">The name of the blob.</param>
    /// <returns>The <see cref="BlobItem"/> if found; otherwise, null.</returns>
    /// <remarks>
    /// This method provides fast, in-memory lookup of blob metadata.
    /// Returns <c>null</c> if the blob name does not exist in the cache.
    /// </remarks>
    public BlobItem? GetBlobItem(string blobName)
    {
        if (_blobItemCache == null)
        {
            return null;
        }

        _blobItemCache.TryGetValue(blobName, out var item);
        return item;
    }

    /// <summary>
    /// Gets the immediate subfolder prefixes under the specified prefix.
    /// </summary>
    /// <param name="prefix">The parent folder prefix.</param>
    /// <returns>An enumerable of immediate subfolder prefixes.</returns>
    /// <remarks>
    /// This method analyzes the cached folder structure and returns only the next-level subfolders under the given prefix.
    /// The returned prefixes can be used for recursive folder enumeration.
    /// </remarks>
    public IEnumerable<string> GetImmediateSubfolders(string prefix)
    {
        if (_folderBlobMap == null)
        {
            return [];
        }

        return _folderBlobMap.Keys
                             .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && k.Length > prefix.Length)
                             .Select(k => GetImmediateSubfolder(prefix, k))
                             .Where(p => p is not null)
                             .Distinct()!;
    }

    /// <summary>
    /// Gets the folder prefix for a given blob name.
    /// </summary>
    /// <param name="blobName">The blob name to extract the folder prefix from.</param>
    /// <returns>The folder prefix, or an empty string if none exists.</returns>
    /// <remarks>
    /// The folder prefix is defined as the substring up to and including the last '/' in the blob name.
    /// If the blob is at the root, an empty string is returned.
    /// </remarks>
    private static string GetFolderPrefix(string blobName)
    {
        var index = blobName.LastIndexOf('/');
        return index >= 0 ? blobName.Substring(0, index + 1) : string.Empty;
    }

    /// <summary>
    /// Gets the immediate subfolder prefix under a parent prefix from a blob name.
    /// </summary>
    /// <param name="prefix">The parent folder prefix.</param>
    /// <param name="blobName">The blob name to analyze.</param>
    /// <returns>The immediate subfolder prefix, or null if not found.</returns>
    /// <remarks>
    /// This method extracts the next-level subfolder from a blob name, relative to the given parent prefix.
    /// For example, for prefix "A/" and blob name "A/B/C/file.rfa", it returns "A/B/" as the immediate subfolder.
    /// </remarks>
    private static string? GetImmediateSubfolder(string prefix, string blobName)
    {
        var remainder = blobName.Substring(prefix.Length);
        var idx = remainder.IndexOf('/');
        if (idx > 0)
        {
            return prefix + remainder.Substring(0, idx + 1);
        }

        return null;
    }
}
