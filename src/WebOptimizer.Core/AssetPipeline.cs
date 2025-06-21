using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;

namespace WebOptimizer;

internal class AssetPipeline(ILogger<Asset> assetLogger) : IAssetPipeline
{
    internal ConcurrentDictionary<string, IAsset> _assets = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// For use by the Asset constructor only. Do not use for logging messages inside <see cref="AssetPipeline"/>.
    /// </summary>
    internal ILogger<Asset> _assetLogger = assetLogger ?? throw new ArgumentNullException(nameof(assetLogger));

    public IReadOnlyList<IAsset> Assets => [.. _assets.Values];

    /// <summary>
    /// Gets the <see cref="IAsset" /> from the specified route.
    /// </summary>
    /// <param name="route">The route to find the asset by.</param>
    /// <param name="asset">The asset matching the route.</param>
    /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
    public bool TryGetAssetFromRoute(string? route, [NotNullWhen(true)] out IAsset? asset)
    {
        asset = null;

        // Bail if this is an absolute path
        if (string.IsNullOrEmpty(route) || route.StartsWith("//") || route.Contains("://"))
        {
            return false;
        }

        string cleanRoute = NormalizeRoute(route);

        // First check direct matches
        if (_assets.TryGetValue(cleanRoute, out asset))
        {
            return true;
        }

        // Then check globbing matches
        if (route != "/")
        {
            var list = Assets;
            foreach (var existing in list)
            {
                PatternMatchingResult result;
                try
                {
                    var matcher = new Matcher();
                    matcher.AddInclude(existing.Route);
                    result = matcher.Match(cleanRoute.TrimStart('/'));
                }
                catch (Exception ex)
                {
                    // Some paths may be invalid and the call to matcher.Match will fail
                    System.Diagnostics.Debug.Write(ex);
                    continue;
                }

                if (result.HasMatches)
                {
                    asset = new Asset(
                        cleanRoute,
                        existing.ContentType,
                        [cleanRoute],
                        _assetLogger);

                    foreach (var processor in existing.Processors)
                    {
                        asset.Processors.Add(processor);
                    }

                    foreach (var items in existing.Items)
                    {
                        if (items.Key == Asset.PhysicalFilesKey)
                        {
                            continue;
                        }

                        asset.Items.Add(items);
                    }

                    _assets.TryAdd(cleanRoute, asset);
                    return true;
                }
            }
        }

        return false;
    }

    public IAsset AddBundle(IAsset asset) =>
        AddBundle(asset.Route, asset.ContentType, [.. asset.SourceFiles]);

    public IEnumerable<IAsset> AddBundle(IEnumerable<IAsset> assets)
    {
        var list = new List<IAsset>();

        foreach (var asset in assets)
        {
            var ass = AddBundle(asset.Route, asset.ContentType, [.. asset.SourceFiles]);
            list.Add(ass);
        }

        return list;
    }

    public IAsset AddBundle(string route, string contentType, params string[] sourceFiles)
    {
        if (sourceFiles.Length == 0)
        {
            throw new ArgumentException("At least one source file has to be specified", nameof(sourceFiles));
        }

        if (route.Contains('*') || route.Contains('[') && route.Contains('?'))
        {
            throw new ArgumentException($"The route \"{route}\" appears to be a globbing pattern which isn't supported for bundle routes.", nameof(route));
        }

        route = NormalizeRoute(route);

        IAsset asset = new Asset(route, contentType, sourceFiles, _assetLogger);
        _assets.TryAdd(route, asset);

        return asset;
    }


    public IAsset AddAsset(string route, string contentType)
    {
        route = NormalizeRoute(route);

        IAsset asset = new Asset(route, contentType, [], _assetLogger);
        _assets.TryAdd(route, asset);

        return asset;
    }

    public IEnumerable<IAsset> AddFiles(string contentType, params string[] sourceFiles)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            throw new ArgumentException("A valid content type must be specified", nameof(contentType));
        }

        if (sourceFiles.Length == 0)
        {
            throw new ArgumentException("At least one source file has to be specified", nameof(sourceFiles));
        }

        var list = new List<IAsset>();

        foreach (string file in sourceFiles)
        {
            var asset = new Asset(NormalizeRoute(file), contentType, [file], _assetLogger);
            list.Add(asset);
            _assets.TryAdd(asset.Route, asset);
        }

        return list;
    }

    public static string NormalizeRoute(string route)
    {
        string trimmedRoute = route.Trim();
        string cleanRoute = trimmedRoute.StartsWith('/') || trimmedRoute.StartsWith('~')
            ? $"/{route.Trim().TrimStart('~', '/')}"
            : trimmedRoute;

        int index = cleanRoute.IndexOfAny(['?', '#']);

        if (index > -1)
        {
            cleanRoute = cleanRoute[..index];
        }

        if (!cleanRoute.StartsWith('/'))
        {
            cleanRoute = $"/{cleanRoute}";
        }

        return cleanRoute;
    }
}
