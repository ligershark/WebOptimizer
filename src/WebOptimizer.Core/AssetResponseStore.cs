using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace WebOptimizer
{
    internal class AssetResponseStore : IAssetResponseStore
    {
        private readonly ILogger<AssetResponseStore> _logger;
        private readonly IHostingEnvironment _env;
        private readonly WebOptimizerOptions _options = new WebOptimizerOptions();

        public AssetResponseStore(ILogger<AssetResponseStore> logger, IHostingEnvironment env, IConfigureOptions<WebOptimizerOptions> options)
        {
            _logger = logger;
            _env = env;
            options.Configure(_options);
        }

        public async Task AddAsync(string bucket, string cachekey, AssetResponse assetResponse)
        {
            string name = CleanName(bucket);
            Directory.CreateDirectory(_options.CacheDirectory);

            // First delete old cached files
            IEnumerable<string> oldCachedFiles = Directory.EnumerateFiles(_options.CacheDirectory, name + "__*.cache");
            foreach (string oldFile in oldCachedFiles)
            {
                await DeleteFileAsync(oldFile).ConfigureAwait(false);
            }

            // Then serialize to disk
            string json = JsonConvert.SerializeObject(assetResponse);
            string filePath = GetPath(bucket, cachekey);
            await WriteFileAsync(filePath, json).ConfigureAwait(false);
        }

        public async Task RemoveAsync(string bucket, string cachekey)
        {
            string filePath = GetPath(bucket, cachekey);
            await DeleteFileAsync(filePath);
        }

        public bool TryGet(string bucket, string cachekey, out AssetResponse assetResponse)
        {
            assetResponse = null;
            string filePath = GetPath(bucket, cachekey);

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    assetResponse = JsonConvert.DeserializeObject<AssetResponse>(json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex);
                    DeleteFileAsync(filePath).GetAwaiter().GetResult();
                }
            }

            return assetResponse != null;
        }

        private string CleanName(string route)
        {
            return route.Replace('\\', '/').Replace('/', '\'');
        }

        private string GetPath(string bucket, string cachekey)
        {
            // cachekey is Base64 encoded which uses / as one of the characters.  So for Linux 
            // we need to clean both the bucket and the cachekey.
            return Path.Combine(_options.CacheDirectory, CleanName($"{bucket}__{cachekey}.cache"));
        }

        private async Task DeleteFileAsync(string filePath, int attempts = 5)
        {
            await TryAsync(attempts, () =>
            {
                File.Delete(filePath);
                return Task.CompletedTask;
            }).ConfigureAwait(false);
        }

        private async Task WriteFileAsync(string filePath, string content, int attempts = 5)
        {
            await TryAsync(attempts, async () =>
            {
                using (var writer = new StreamWriter(filePath))
                {
                    await writer.WriteAsync(content).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }

        private async Task TryAsync(int attempts, Func<Task> callback)
        {
            try
            {
                await callback().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
                if (attempts > 0)
                {
                    await Task.Delay(10).ConfigureAwait(false);
                    attempts--;
                    await TryAsync(attempts, callback).ConfigureAwait(false);
                }
            }
        }
    }
}
