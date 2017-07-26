using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;

namespace Bundler.Utilities
{
    public class FileCacheHelper
    {
        private MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions();
        private IMemoryCache _cache;
        public IFileProvider FileProvider { get; private set; }
        

        public FileCacheHelper(IFileProvider fileProvider, IMemoryCache cache)
        {
            _cache = cache;
            FileProvider = fileProvider;
        }

        private void AddExpirationToken(string file)
        {
            _cacheOptions.AddExpirationToken(FileProvider.Watch(file));
        }

        public void AddItemToCache(string cacheKey, string result, string file)
        {
            _cacheOptions.AddExpirationToken(FileProvider.Watch(file));

            _cache.Set(cacheKey, result, _cacheOptions);
        }

        public void AddFileToCache(string cacheKey, string value, string file)
        {
            AddExpirationToken(file);
            _cache.Set(cacheKey, value, _cacheOptions);
        }

        public void AddFileBundleToCache(string cacheKey, string value, IEnumerable<string> files)
        {
            foreach (string file in files)
            {
                AddExpirationToken(file);
            }

            _cache.Set(cacheKey, value, _cacheOptions);
        }

        public bool TryGetValue(string cacheKey, out string value)
        {
            return _cache.TryGetValue(cacheKey, out value);
        }
    }
}
