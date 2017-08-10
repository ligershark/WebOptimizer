using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.FileProviders;

namespace WebOptimizer
{
    internal class CssFingerprinter : Processor
    {
        private static readonly Regex _rxUrl = new Regex(@"url\s*\(\s*([""']?)([^:)]+)\1\s*\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override Task ExecuteAsync(IAssetContext config)
        {
            var content = new Dictionary<string, byte[]>();
            var pipeline = (IAssetPipeline)config.HttpContext.RequestServices.GetService(typeof(IAssetPipeline));

            foreach (string key in config.Content.Keys)
            {
                IFileInfo input = config.Options.FileProvider.GetFileInfo(key);
                IFileInfo output = config.Options.FileProvider.GetFileInfo(config.Asset.Route);

                content[key] = Adjust(config.Content[key].AsString(), input, output);
            }

            config.Content = content;

            return Task.CompletedTask;
        }

        private static byte[] Adjust(string content, IFileInfo input, IFileInfo output)
        {
            MatchCollection matches = _rxUrl.Matches(content);

            // Ignore the file if no match
            if (matches.Count > 0)
            {
                string inputDir = Path.GetDirectoryName(input.PhysicalPath);

                foreach (Match match in matches)
                {
                    string urlValue = match.Groups[2].Value;

                    // Ignore references with protocols
                    if (urlValue.Contains("://") || urlValue.StartsWith("//") || urlValue.StartsWith("data:"))
                        continue;

                    //prevent query string from causing error
                    string[] pathAndQuery = urlValue.Split(new[] { '?' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    string pathOnly = pathAndQuery[0];
                    string queryOnly = pathAndQuery.Length == 2 ? pathAndQuery[1] : string.Empty;

                    var info = new FileInfo(Path.Combine(inputDir, pathOnly.TrimStart('/')));

                    if (!info.Exists)
                    {
                        continue;
                    }

                    string hash = GenerateHash(info.LastWriteTime.Ticks.ToString());
                    string withHash = pathOnly + $"?v={hash}";

                    if (!string.IsNullOrEmpty(queryOnly))
                    {
                        withHash += $"&{queryOnly}";
                    }

                    content = content.Replace(match.Groups[2].Value, withHash);
                }
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

    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static partial class PipelineExtensions
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
