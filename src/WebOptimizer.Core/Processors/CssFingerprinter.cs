using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.FileProviders;
using WebOptimizer;

namespace WebOptimizer
{
    internal class CssFingerprinter : Processor
    {
        private static readonly Regex _rxUrl = new Regex(@"(url\s*\(\s*)([""']?)([^:)]+)(\2\s*\))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override Task ExecuteAsync(IAssetContext config)
        {
            var content = new Dictionary<string, byte[]>();
            var env = (IWebHostEnvironment)config.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment));
            var pipeline = (IAssetPipeline)config.HttpContext.RequestServices.GetService(typeof(IAssetPipeline));
            IFileProvider fileProvider = config.Asset.GetFileProvider(env);

            foreach (string key in config.Content.Keys)
            {
                IFileInfo input = fileProvider.GetFileInfo(key);
                IFileInfo output = fileProvider.GetFileInfo(config.Asset.Route);

                content[key] = Adjust(config.Content[key].AsString(), input, output, env);
            }

            config.Content = content;

            return Task.CompletedTask;
        }

        private static byte[] Adjust(string content, IFileInfo input, IFileInfo output, IWebHostEnvironment env)
        {
            string inputDir = Path.GetDirectoryName(input.PhysicalPath);

            Match match = _rxUrl.Match(content);

            while (match.Success)
            {
                string urlValue = match.Groups[3].Value;
                string dir = inputDir;

                // Ignore references with protocols
                if (urlValue.Contains("://") || urlValue.StartsWith("//") || urlValue.StartsWith("data:"))
                    continue;

                //prevent query string from causing error
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
                    match = _rxUrl.Match(content, match.Index + match.Length);
                    continue;
                }

                string hash = GenerateHash(info.LastWriteTime.Ticks.ToString());
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

                string preMatchContent = content.Substring(0, match.Index);
                string postMatchContent = content.Substring(match.Index + match.Length);

                content = preMatchContent + replaced + postMatchContent;

                //search next match from end of one just found (and replaced)
                int startIndex = (preMatchContent + replaced).Length;
                match = _rxUrl.Match(content, startIndex);
            }

            return content.AsByteArray();
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
        /// NOTE: Make sure to call Concatinate() before this method
        /// </summary>
        public static IAsset FingerprintUrls(this IAsset bundle)
        {
            var minifier = new CssFingerprinter();
            bundle.Processors.Add(minifier);

            return bundle;
        }

        /// <summary>
        /// Adds a fingerprint to local url() references.
        /// NOTE: Make sure to call Concatinate() before this method
        /// </summary>
        public static IEnumerable<IAsset> FingerprintUrls(this IEnumerable<IAsset> assets)
        {
            return assets.AddProcessor(asset => asset.FingerprintUrls());
        }
    }
}
