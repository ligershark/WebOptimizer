using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebOptimizer
{
    [Serializable]
    internal class MemoryCachedResponse
    {
        public MemoryCachedResponse(byte[] body)
        {
            Body = body;
            Headers = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Headers { get; }

        public byte[] Body { get; set; }

        public static bool TryGetFromDiskCache(string route, string cacheKey, string cacheDir, out MemoryCachedResponse response)
        {
            response = null;
            string name = CleanRouteName(route);
            string filePath = GetPath(name, cacheKey, cacheDir);

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    response = JsonConvert.DeserializeObject<MemoryCachedResponse>(json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex);
                    File.Delete(filePath);
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
                File.Delete(oldFile);
            }

            // Then serialize to disk
            string filePath = GetPath(name, cacheKey, cacheDir);

            string json = JsonConvert.SerializeObject(this);
            using (var writer = new StreamWriter(filePath))
            {
                await writer.WriteAsync(json).ConfigureAwait(false);
            }
        }

        private static string CleanRouteName(string route)
        {
            return route.Replace('\\', '/').Replace('/', '\''); ;
        }

        private static string GetPath(string name, string cacheKey, string cacheDir)
        {
            return Path.Combine(cacheDir, $"{name}__{cacheKey}.cache");
        }
    }
}
