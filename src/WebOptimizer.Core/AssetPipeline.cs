using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;

namespace WebOptimizer
{
    internal class AssetPipeline : IAssetPipeline
    {
        internal ConcurrentDictionary<string, IAsset> _assets = new ConcurrentDictionary<string, IAsset>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<IAsset> Assets => _assets.Values.ToList();

        public bool TryGetAssetFromRoute(string route, out IAsset asset)
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
                IReadOnlyList<IAsset> list = Assets;
                foreach (IAsset existing in list)
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
                        asset = new Asset(cleanRoute, existing.ContentType, this, new[]
                        {
                            cleanRoute
                        });

                        foreach (IProcessor processor in existing.Processors)
                        {
                            asset.Processors.Add(processor);
                        }

                        foreach (KeyValuePair<string, object> items in existing.Items)
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

        public IAsset AddBundle(IAsset asset)
        {
            return AddBundle(asset.Route, asset.ContentType, asset.SourceFiles.ToArray());
        }

        public IEnumerable<IAsset> AddBundle(IEnumerable<IAsset> assets)
        {
            var list = new List<IAsset>();

            foreach (IAsset asset in assets)
            {
                IAsset ass = AddBundle(asset.Route, asset.ContentType, asset.SourceFiles.ToArray());
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

            if (route.Contains("*") || route.Contains("[") && route.Contains("?"))
            {
                throw new ArgumentException($"The route \"{route}\" appears to be a globbing pattern which isn't supported for bundle routes.", nameof(route));
            }

            route = NormalizeRoute(route);

            if (Assets.Any(a => a.Route.Equals(route, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"The route \"{route}\" was already specified", nameof(route));
            }

            IAsset asset = new Asset(route, contentType, this, sourceFiles);
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
                IAsset asset = new Asset(NormalizeRoute(file), contentType, this, new[] { file });
                list.Add(asset);
                _assets.TryAdd(asset.Route, asset);
            }

            return list;
        }

        public static string NormalizeRoute(string route)
        {
            string trimmedRoute = route.Trim();
            string cleanRoute = trimmedRoute.StartsWith( '/') || trimmedRoute.StartsWith("~")
                ? "/" + route.Trim().TrimStart('~', '/')
                : trimmedRoute;

            int index = cleanRoute.IndexOfAny(new[] { '?', '#' });

            if (index > -1)
            {
                cleanRoute = cleanRoute.Substring(0, index);
            }

            return cleanRoute;
        }
    }
}
