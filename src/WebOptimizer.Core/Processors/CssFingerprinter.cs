using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.FileProviders;
using WebOptimizer;
using WebOptimizer.Utils;

namespace WebOptimizer
{
    internal class CssFingerprinter : Processor
    {
        private static readonly Regex _rxUrl = new Regex(@"(url\s*\(\s*)([""']?)([^:)]+)(\2\s*\))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override Task ExecuteAsync(IAssetContext config)
        {
            var content = new Dictionary<string, byte[]>();
            var env = (IWebHostEnvironment)config.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment));

            IFileProvider fileProvider = config.Asset.GetAssetFileProvider(env);

            foreach (string key in config.Content.Keys)
            {
                content[key] = Adjust(config, key, fileProvider);
            }

            config.Content = content;

            return Task.CompletedTask;
        }

        private static byte[] Adjust(IAssetContext config, string key, IFileProvider fileProvider)
        {
            string content = config.Content[key].AsString();

            return _rxUrl.Replace(content, match =>
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

                string hash = GenerateHash(linkedFileInfo.LastModified.Ticks.ToString());
                string withHash = pathOnly + $"?v={hash}";

                if (!string.IsNullOrEmpty(queryOnly))
                {
                    withHash += $"&{queryOnly}";
                }

                string replaced =
                    match.Groups[1].Value +
                    match.Groups[2].Value +
                    withHash +
                    match.Groups[4].Value;

                return replaced;
            }).AsByteArray();
        }

        private static string GenerateHash(string content)
        {
            using (var algo = SHA1.Create())
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
                byte[] hash = algo.ComputeHash(buffer);
                return WebEncoders.Base64UrlEncode(hash);
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
        /// Adds a fingerprint to local url() references.
        /// NOTE: Make sure to call this method before Concatinate()
        /// </summary>
        public static IAsset FingerprintUrls(this IAsset bundle)
        {
            var minifier = new CssFingerprinter();
            bundle.Processors.Add(minifier);

            return bundle;
        }

        /// <summary>
        /// Adds a fingerprint to local url() references.
        /// NOTE: Make sure to call this method before Concatinate()
        /// </summary>
        public static IEnumerable<IAsset> FingerprintUrls(this IEnumerable<IAsset> assets)
        {
            return assets.AddProcessor(asset => asset.FingerprintUrls());
        }
    }
}
