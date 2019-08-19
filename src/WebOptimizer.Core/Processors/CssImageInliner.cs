using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using WebOptimizer;

namespace WebOptimizer
{
    internal class CssImageInliner : Processor
    {
        private static readonly Regex _rxUrl = new Regex(@"url\s*\(\s*([""']?)([^:)]+)\1\s*\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static int _maxFileSize;

        public CssImageInliner(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        public override async Task ExecuteAsync(IAssetContext config)
        {
            var content = new Dictionary<string, byte[]>();
            var env = (IWebHostEnvironment)config.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment));
            var pipeline = (IAssetPipeline)config.HttpContext.RequestServices.GetService(typeof(IAssetPipeline));
            IFileProvider fileProvider = config.Asset.GetFileProvider(env);

            foreach (string key in config.Content.Keys)
            {
                IFileInfo input = fileProvider.GetFileInfo(key);

                content[key] = await InlineAsync(config.Content[key].AsString(), input, env);
            }

            config.Content = content;
        }

        private static async Task<byte[]> InlineAsync(string content, IFileInfo input, IWebHostEnvironment env)
        {
            MatchCollection matches = _rxUrl.Matches(content);
            string inputDir = Path.GetDirectoryName(input.PhysicalPath);

            foreach (Match match in matches)
            {
                string urlValue = match.Groups[2].Value;
                string dir = inputDir;

                if (urlValue.Contains("://") || urlValue.StartsWith("//"))
                {
                    continue;
                }

                string[] pathAndQuery = urlValue.Split(new[] { '?' }, 2, StringSplitOptions.RemoveEmptyEntries);
                string pathOnly = pathAndQuery[0];
                string queryOnly = pathAndQuery.Length == 2 ? pathAndQuery[1] : string.Empty;

                if (pathOnly.StartsWith("/", StringComparison.Ordinal))
                {
                    dir = env.WebRootPath;
                }

                var info = new FileInfo(Path.Combine(dir, pathOnly.TrimStart('/')));

                if (!info.Exists)
                {
                    continue;
                }

                if (info.Length > _maxFileSize && (!queryOnly.Contains("&inline") && !queryOnly.Contains("?inline")))
                    continue;

                string mimeType = GetMimeTypeFromFileExtension(info.Name);

                if (!string.IsNullOrEmpty(mimeType))
                {
                    using (Stream fs = info.OpenRead())
                    {
                        string base64 = Convert.ToBase64String(await fs.AsBytesAsync());
                        string dataUri = $"url('data:{mimeType};base64,{base64}')";
                        content = content.Replace(match.Value, dataUri);
                    }
                }
            }

            return content.AsByteArray();
        }

        static string GetMimeTypeFromFileExtension(string file)
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
