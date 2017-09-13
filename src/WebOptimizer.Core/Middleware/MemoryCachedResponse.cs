using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace WebOptimizer
{
    [Serializable]
    internal class MemoryCachedResponse
    {
        public MemoryCachedResponse(byte[] body)
        {
            Body = body;
        }

        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        public byte[] Body { get; set; }

        public static bool TryGetFromDiskCache(string route, string cacheKey, string cacheDir, out MemoryCachedResponse response)
        {
            response = null;
            string name = CleanRouteName(route);
            string filePath = GetPath(name, cacheKey, cacheDir);

            if (File.Exists(filePath))
            {
                var formatter = new BinaryFormatter();

                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    try
                    {
                        response = formatter.Deserialize(fs) as MemoryCachedResponse;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Write(ex);
                        File.Delete(filePath);
                    }
                }
            }

            return response != null;
        }

        public void CacheToDisk(string route, string cacheKey, string cacheDir)
        {
            var formatter = new BinaryFormatter();
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

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                try
                {
                    formatter.Serialize(fs, this);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex);
                }
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
