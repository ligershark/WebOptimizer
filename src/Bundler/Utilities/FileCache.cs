using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;

namespace Bundler.Utilities
{
    /// <summary>
    /// A helper class for working with the memory cache.
    /// </summary>
    internal class FileCache
    {
        private IMemoryCache _cache;

        /// <summary>
        /// Instantiates a new instance of the class.
        /// </summary>
        public FileCache(IFileProvider fileProvider, IMemoryCache cache)
        {
            _cache = cache;
            FileProvider = fileProvider;
        }

        /// <summary>
        /// The FileProvider used for cache invalidations
        /// </summary>
        public IFileProvider FileProvider { get; }

        private void AddExpirationToken(MemoryCacheEntryOptions cacheOptions, string file)
        {
            cacheOptions.AddExpirationToken(FileProvider.Watch(file));
        }

        /// <summary>
        /// Adds a value to the cache and invalidates it based on the specified files
        /// </summary>
        public void Add(string cacheKey, string value, params string[] files)
        {
            Add(cacheKey, value, files);
        }

        /// <summary>
        /// Adds a value to the cache and invalidates it based on the specified files
        /// </summary>
        public void Add(string cacheKey, string value, IEnumerable<string> files)
        {
            var cacheOptions = new MemoryCacheEntryOptions();

            foreach (string file in files)
            {
                AddExpirationToken(cacheOptions, file);
            }

            _cache.Set(cacheKey, value, cacheOptions);
        }

        /// <summary>
        /// Tries to get the value from the cache.
        /// </summary>
        public bool TryGetValue(string cacheKey, out string value)
        {
            return _cache.TryGetValue(cacheKey, out value);
        }
    }
}
