using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebOptimizer;
using WebOptimizer.Utils;

namespace WebOptimizer
{
    internal class RelativePathAdjuster : Processor
    {
        private static readonly Regex _rxUrl = new Regex(@"(url\s*\(\s*)([""']?)([^:)]+)(\2\s*\))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly string _protocol = "file:///";

        public override Task ExecuteAsync(IAssetContext config)
        {
            var content = new Dictionary<string, byte[]>();

            foreach (string key in config.Content.Keys)
            {
                content[key] = Adjust(config, key);
            }

            config.Content = content;

            return Task.CompletedTask;
        }

        private static byte[] Adjust(IAssetContext config, string key)
        {
            string content = config.Content[key].AsString();

            return _rxUrl.Replace(content, match =>
            {
                // no change on inline data
                if (match.Value.StartsWith("data:"))
                    return match.Value;

                string urlValue = match.Groups[3].Value;

                // no change on absolute urls
                if (Uri.IsWellFormedUriString(urlValue, UriKind.Absolute))
                    return match.Value;

                // no change if other host
                if (urlValue.StartsWith("//"))
                    return match.Value;

                // no change, if absolute path
                if (UrlPathUtils.IsAbsolutePath(urlValue))
                    return match.Value;

                // get absolute path of content file
                string appPath = (config.HttpContext?.Request?.PathBase.HasValue ?? false)
                    ? config.HttpContext.Request.PathBase.Value
                    : "/";

                string routePath = UrlPathUtils.MakeAbsolute(appPath, config.Asset.Route.TrimStart('/'));

                // prevent query string from causing error
                string[] pathAndQuery = urlValue.Split(new[] { '?' }, 2, StringSplitOptions.RemoveEmptyEntries);
                string pathOnly = pathAndQuery[0];
                string queryOnly = pathAndQuery.Length == 2 ? ("?" + pathAndQuery[1]) : string.Empty;

                // get filepath of included file
                string filePath;
                if (pathOnly.StartsWith("~/"))
                {
                    filePath = UrlPathUtils.MakeAbsolute(appPath, pathOnly.Substring(2));
                }
                else
                {
                    if (!UrlPathUtils.TryMakeAbsolutePathFromInclude(appPath, key, pathOnly, out filePath))
                        // path to included file is invalid
                        return match.Value;
                }

                string relativePath = MakeRelative(routePath, filePath);

                string replaced =
                    match.Groups[1].Value +
                    match.Groups[2].Value +
                    relativePath + queryOnly +
                    match.Groups[4].Value;

                return replaced;

            }).AsByteArray();
        }

        private static string MakeRelative(string baseFile, string file)
        {
            if (string.IsNullOrEmpty(file))
                return file;

            // The file:// protocol is to make it work on Linux.
            // See https://github.com/madskristensen/BundlerMinifier/commit/01fe7a050eda073f8949caa90eedc4c23e04d0ce
            var baseUri = new Uri(_protocol + baseFile.TrimStart('/'), UriKind.RelativeOrAbsolute);
            var fileUri = new Uri(_protocol + file.TrimStart('/'), UriKind.RelativeOrAbsolute);

            if (baseUri.IsAbsoluteUri)
            {
                return Uri.UnescapeDataString(baseUri.MakeRelativeUri(fileUri).ToString());
            }
            else
            {
                return baseUri.ToString();
            }
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static partial class AssetPipelineExtensions
    {
        /// <summary>
        /// Adjusts the relative paths in CSS documents.
        /// </summary>
        public static IAsset AdjustRelativePaths(this IAsset bundle)
        {
            var minifier = new RelativePathAdjuster();
            bundle.Processors.Add(minifier);

            return bundle;
        }

        /// <summary>
        /// Adjusts the relative paths in CSS documents.
        /// </summary>
        public static IEnumerable<IAsset> AdjustRelativePaths(this IEnumerable<IAsset> assets)
        {
            return assets.AddProcessor(asset => asset.AdjustRelativePaths());
        }
    }
}
