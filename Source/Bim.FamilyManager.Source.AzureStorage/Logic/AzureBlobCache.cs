using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Bim.FamilyManager.Source.AzureStorage.Logic;

public sealed class AzureBlobCache
{
    private readonly BlobContainerClient _blobContainerClient;
    private Dictionary<string, BlobItem>? _blobItemCache;
    private Dictionary<string, List<string>>? _folderBlobMap;

    public AzureBlobCache(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_blobItemCache != null && _folderBlobMap != null)
            return;

        _blobItemCache = new Dictionary<string, BlobItem>(StringComparer.OrdinalIgnoreCase);
        _folderBlobMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        await foreach (var blob in _blobContainerClient.GetBlobsAsync(cancellationToken: cancellationToken))
        {
            _blobItemCache[blob.Name] = blob;

            var folderPrefix = GetFolderPrefix(blob.Name);
            if (!_folderBlobMap.TryGetValue(folderPrefix, out var list))
            {
                list = new List<string>();
                _folderBlobMap[folderPrefix] = list;
            }
            list.Add(blob.Name);
        }
    }

    public IEnumerable<string> GetBlobNamesByPrefix(string prefix, bool includeSubfolders)
    {
        if (_blobItemCache == null || _folderBlobMap == null)
            return Enumerable.Empty<string>();

        if (includeSubfolders)
            return _blobItemCache.Keys.Where(name => name.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase));
        else
            return _folderBlobMap.TryGetValue(prefix, out var names) ? names : Enumerable.Empty<string>();
    }

    public BlobItem? GetBlobItem(string blobName)
    {
        if (_blobItemCache == null)
            return null;
        _blobItemCache.TryGetValue(blobName, out var item);
        return item;
    }

    public IEnumerable<string> GetImmediateSubfolders(string prefix)
    {
        if (_folderBlobMap == null)
            return Enumerable.Empty<string>();

        return _folderBlobMap.Keys
            .Where(k => k.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase) && k.Length > prefix.Length)
            .Select(k => GetImmediateSubfolder(prefix, k))
            .Where(p => p is not null)
            .Distinct()!;
    }

    private static string GetFolderPrefix(string blobName)
    {
        var idx = blobName.LastIndexOf('/');
        return idx >= 0 ? blobName.Substring(0, idx + 1) : string.Empty;
    }

    private static string? GetImmediateSubfolder(string prefix, string blobName)
    {
        var remainder = blobName.Substring(prefix.Length);
        var idx = remainder.IndexOf('/');
        if (idx > 0)
            return prefix + remainder.Substring(0, idx + 1);
        return null;
    }
}