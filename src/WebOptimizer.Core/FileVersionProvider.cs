using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;

namespace WebOptimizer;

/// <summary>
/// Creates a new instance of <see cref="FileVersionProvider"/>.
/// </summary>
/// <param name="fileProvider">The file provider to get and watch files.</param>
/// <param name="cache"><see cref="IMemoryCache"/> where versioned urls of files are cached.</param>
/// <param name="requestPathBase">The base path for the current HTTP request.</param>
public class FileVersionProvider(
    IFileProvider fileProvider,
    IMemoryCache cache,
    PathString requestPathBase)
{
    private const string VersionKey = "v";
    private static readonly char[] _queryStringAndFragmentTokens = ['?', '#'];
    private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly IFileProvider _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));

    /// <summary>
    /// Adds version query parameter to the specified file path.
    /// </summary>
    /// <param name="path">The path of the file to which version should be added.</param>
    /// <returns>Path containing the version query string.</returns>
    /// <remarks>The version query string is appended with the key "v".</remarks>
    public string? AddFileVersionToPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        string resolvedPath = path;

        int queryStringOrFragmentStartIndex = path.IndexOfAny(_queryStringAndFragmentTokens);
        if (queryStringOrFragmentStartIndex != -1)
        {
            resolvedPath = path[..queryStringOrFragmentStartIndex];
        }

        if (Uri.TryCreate(resolvedPath, UriKind.Absolute, out var uri) && !uri.IsFile)
        {
            // Don't append version if the path is absolute.
            return path;
        }

        if (!_cache.TryGetValue(path, out string? value))
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions();
            _ = cacheEntryOptions.AddExpirationToken(_fileProvider.Watch(resolvedPath));
            var fileInfo = _fileProvider.GetFileInfo(resolvedPath);

            if (!fileInfo.Exists &&
                requestPathBase.HasValue &&
                resolvedPath.StartsWith(requestPathBase.Value, StringComparison.OrdinalIgnoreCase))
            {
                string requestPathBaseRelativePath = resolvedPath[requestPathBase.Value.Length..];
                _ = cacheEntryOptions.AddExpirationToken(_fileProvider.Watch(requestPathBaseRelativePath));
                fileInfo = _fileProvider.GetFileInfo(requestPathBaseRelativePath);
            }

            if (fileInfo.Exists)
            {
                value = QueryHelpers.AddQueryString(path, VersionKey, GetHashForFile(fileInfo));
            }
            else
            {
                // if the file is not in the current server.
                value = path;
            }

            value = _cache.Set(path, value, cacheEntryOptions);
        }

        return value;
    }

    private static string GetHashForFile(IFileInfo fileInfo)
    {
        using var sha256 = SHA256.Create();
        using var readStream = fileInfo.CreateReadStream();
        byte[] hash = sha256.ComputeHash(readStream);
        return WebEncoders.Base64UrlEncode(hash);
    }
}
