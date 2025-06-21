using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using WebOptimizer;
using WebOptimizer.Utils;

namespace WebOptimizer
{
    internal partial class CssImageInliner : Processor
    {
        [GeneratedRegex(@"(url\s*\(\s*)([""']?)([^:)]+)(\2\s*\))", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex UrlRegex();

        private static readonly Regex _rxUrl = UrlRegex();
        private static int _maxFileSize;

        public CssImageInliner(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        public override async Task ExecuteAsync(IAssetContext config)
        {
            var content = new Dictionary<string, byte[]>();
            var env = (IWebHostEnvironment)config.HttpContext.RequestServices.GetRequiredService(typeof(IWebHostEnvironment));
            IFileProvider fileProvider = config.Asset.GetAssetFileProvider(env);

            foreach (string key in config.Content.Keys)
            {
                _ = fileProvider.GetFileInfo(key);

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
                sb.Append(await ReplaceMatchAsync(config, fileProvider, match));
                lastIndex = match.Index + match.Length;
            }

            sb.Append(content, lastIndex, content.Length - lastIndex);

            return sb.ToString().AsByteArray();
        }

        private static async Task<string> ReplaceMatchAsync(IAssetContext config, IFileProvider fileProvider, Match match)
        {
            // no fingerprint on inline data
            if (match.Value.StartsWith("data:"))
            {
                return match.Value;
            }

            string urlValue = match.Groups[3].Value;

            // no fingerprint on absolute urls
            if (Uri.IsWellFormedUriString(urlValue, UriKind.Absolute))
            {
                return match.Value;
            }

            // no fingerprint if other host
            if (urlValue.StartsWith("//"))
            {
                return match.Value;
            }

            string routeBasePath = UrlPathUtils.GetDirectory(config.Asset.Route);

            // prevent query string from causing error
            string[] pathAndQuery = urlValue.Split(['?'], 2, StringSplitOptions.RemoveEmptyEntries);
            string pathOnly = pathAndQuery[0];
            string queryOnly = pathAndQuery.Length == 2 ? pathAndQuery[1] : string.Empty;

            // get filepath of included file
            if (!UrlPathUtils.TryMakeAbsolute(routeBasePath, pathOnly, out string? filePath))
            {
                // path to included file is invalid
                return match.Value;
            }

            // get FileInfo of included file
            var linkedFileInfo = filePath is null ? null : fileProvider.GetFileInfo(filePath);

            // no fingerprint if file is not found
            if (linkedFileInfo is null || !linkedFileInfo.Exists)
            {
                return match.Value;
            }

            if (linkedFileInfo.Length > _maxFileSize &&
                (!queryOnly.Contains("&inline") && !queryOnly.Contains("?inline")))
            {
                return match.Value;
            }

            string? mimeType = GetMimeTypeFromFileExtension(linkedFileInfo.Name);

            if (string.IsNullOrEmpty(mimeType))
            {
                return match.Value;
            }

            using var fs = linkedFileInfo.CreateReadStream();
            string base64 = Convert.ToBase64String(await fs.AsBytesAsync());
            string? dataUri = $"data:{mimeType};base64,{base64}";

            string replaced =
                match.Groups[1].Value +
                match.Groups[2].Value +
                dataUri +
                match.Groups[4].Value;

            return replaced;
        }

        private static string? GetMimeTypeFromFileExtension(string file)
        {
            string ext = Path.GetExtension(file).TrimStart('.');

            return ext switch
            {
                "jpg" or "jpeg" => "image/jpeg",
                "gif" => "image/gif",
                "png" => "image/png",
                "webp" => "image/webp",
                "svg" => "image/svg+xml",
                "ttf" => "application/x-font-ttf",
                "otf" => "application/x-font-opentype",
                "woff" => "application/font-woff",
                "woff2" => "application/font-woff2",
                "eot" => "application/vnd.ms-fontobject",
                "sfnt" => "application/font-sfnt",
                _ => null,
            };
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
