using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebOptimizer
{
    [Serializable]
    internal class AssetResponse : IAssetResponse
    {
        public AssetResponse(byte[] body, string cacheKey)
        {
            Body = body;
            CacheKey = cacheKey;
            Headers = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Headers { get; }

        public byte[] Body { get; set; }

        public string CacheKey { get; }

        public static bool TryGetFromDiskCache(string route, string cacheKey, string cacheDir, out AssetResponse response)
        {
            response = null;
            string name = CleanRouteName(route);
            string filePath = GetPath(name, cacheKey, cacheDir);

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    response = JsonConvert.DeserializeObject<AssetResponse>(json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex);
                    DeleteFileAsync(filePath).GetAwaiter().GetResult();
                }
            }

            return response != null;
        }

        public async Task CacheToDiskAsync(string route, string cacheKey, string cacheDir)
        {
            string name = CleanRouteName(route);
            Directory.CreateDirectory(cacheDir);

            // First delete old cached files
            IEnumerable<string> oldCachedFiles = Directory.EnumerateFiles(cacheDir, name + "__*.cache");

            foreach (string oldFile in oldCachedFiles)
            {
                await DeleteFileAsync(oldFile).ConfigureAwait(false);
            }

            // Then serialize to disk
            string filePath = GetPath(name, cacheKey, cacheDir);
            string json = JsonConvert.SerializeObject(this);

            await WriteFileAsync(filePath, json);
        }

        private static string CleanRouteName(string route)
        {
            return route.Replace('\\', '/').Replace('/', '\''); ;
        }

        private static string GetPath(string name, string cacheKey, string cacheDir)
        {
            return Path.Combine(cacheDir, $"{name}__{cacheKey}.cache");
        }

        private static async Task DeleteFileAsync(string filePath, int attempts = 5)
        {
            await TryAsync(attempts, () =>
            {
                File.Delete(filePath);
                return Task.CompletedTask;
            }).ConfigureAwait(false);
        }

        private static async Task WriteFileAsync(string filePath, string content, int attempts = 5)
        {
            await TryAsync(attempts, async () =>
            {
                using (var writer = new StreamWriter(filePath))
                {
                    await writer.WriteAsync(content).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }

        private static async Task TryAsync(int attempts, Func<Task> callback)
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
                    await Task.Delay(10);
                    attempts--;
                    await TryAsync(attempts, callback).ConfigureAwait(false);
                }
            }
        }
    }
}
