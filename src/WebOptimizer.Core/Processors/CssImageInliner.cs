using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using WebOptimizer;
using WebOptimizer.Utils;

namespace WebOptimizer
{
    internal class CssImageInliner : Processor
    {
        private static readonly Regex _rxUrl = new Regex(@"(url\s*\(\s*)([""']?)([^:)]+)(\2\s*\))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static int _maxFileSize;

        public CssImageInliner(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        public override async Task ExecuteAsync(IAssetContext config)
        {
            var content = new Dictionary<string, byte[]>();
            var env = (IWebHostEnvironment)config.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment));
            IFileProvider fileProvider = config.Asset.GetAssetFileProvider(env);

            foreach (string key in config.Content.Keys)
            {
                IFileInfo input = fileProvider.GetFileInfo(key);

                content[key] = await InlineAsync(config, key, fileProvider);
            }

            config.Content = content;
        }

        private static async Task<byte[]> InlineAsync(IAssetContext config, string key, IFileProvider fileProvider)
        {
            string content = config.Content[key].AsString();

            var sb = new StringBuilder();
            int lastIndex = 0;

            foreach (Match match in _rxUrl.Matches(content))
            {
                sb.Append(content, lastIndex, match.Index - lastIndex);
                sb.Append(await ReplaceMatch(config, key, fileProvider, match));
                lastIndex = match.Index + match.Length;
            }

            sb.Append(content, lastIndex, content.Length - lastIndex);

            return sb.ToString().AsByteArray();
        }

        private static async Task<string> ReplaceMatch(IAssetContext config, string key, IFileProvider fileProvider, Match match)
        {
            // no fingerprint on inline data
            if (match.Value.StartsWith("data:"))
                return match.Value;

            string urlValue = match.Groups[3].Value;

            // no fingerprint on absolute urls
            if (Uri.IsWellFormedUriString(urlValue, UriKind.Absolute))
                return match.Value;

            // no fingerprint if other host
            if (urlValue.StartsWith("//"))
                return match.Value;

            string routeBasePath = UrlPathUtils.GetDirectory(config.Asset.Route);

            // prevent query string from causing error
            string[] pathAndQuery = urlValue.Split(new[] { '?' }, 2, StringSplitOptions.RemoveEmptyEntries);
            string pathOnly = pathAndQuery[0];
            string queryOnly = pathAndQuery.Length == 2 ? pathAndQuery[1] : string.Empty;

            // get filepath of included file
            if (!UrlPathUtils.TryMakeAbsolute(routeBasePath, pathOnly, out string filePath))
                // path to included file is invalid
                return match.Value;

            // get FileInfo of included file
            IFileInfo linkedFileInfo = fileProvider.GetFileInfo(filePath);

            // no fingerprint if file is not found
            if (!linkedFileInfo.Exists)
                return match.Value;

            if (linkedFileInfo.Length > _maxFileSize &&
                (!queryOnly.Contains("&inline") && !queryOnly.Contains("?inline")))
                return match.Value;

            string mimeType = GetMimeTypeFromFileExtension(linkedFileInfo.Name);

            if (string.IsNullOrEmpty(mimeType))
                return match.Value;

            using (Stream fs = linkedFileInfo.CreateReadStream())
            {
                string base64 = Convert.ToBase64String(await fs.AsBytesAsync());
                string dataUri = $"data:{mimeType};base64,{base64}";

                string replaced =
                    match.Groups[1].Value +
                    match.Groups[2].Value +
                    dataUri +
                    match.Groups[4].Value;

                return replaced;
            }
        }

        private static string GetMimeTypeFromFileExtension(string file)
        {
            string ext = Path.GetExtension(file).TrimStart('.');

            switch (ext)
            {
                case "jpg":
                case "jpeg":
                    return "image/jpeg";
                case "gif":
                    return "image/gif";
                case "png":
                    return "image/png";
                case "webp":
                    return "image/webp";
                case "svg":
                    return "image/svg+xml";
                case "ttf":
                    return "application/x-font-ttf";
                case "otf":
                    return "application/x-font-opentype";
                case "woff":
                    return "application/font-woff";
                case "woff2":
                    return "application/font-woff2";
                case "eot":
                    return "application/vnd.ms-fontobject";
                case "sfnt":
                    return "application/font-sfnt";
            }

            return null;
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
        /// Inlines url() references as base64 encoded strings if the image size is below <paramref name="maxFileSize"/>.
        /// </summary>
        public static IAsset InlineImages(this IAsset bundle, int maxFileSize = 5120)
        {
            var minifier = new CssImageInliner(maxFileSize);
            bundle.Processors.Add(minifier);

            return bundle;
        }

        /// <summary>
        /// Adds a fingerprint to local url() references.
        /// NOTE: Make sure to call Concatinate() before this method
        /// </summary>
        public static IEnumerable<IAsset> InlineImages(this IEnumerable<IAsset> assets, int maxFileSize = 5120)
        {
            return assets.AddProcessor(asset => asset.InlineImages(maxFileSize));
        }
    }
}
