using System;
using System.IO;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace WebOptimizer
{
    internal class WebOptimizerConfig : IConfigureOptions<WebOptimizerOptions>, IDisposable
    {
        private readonly IConfiguration _config;
        private readonly IOptionsMonitorCache<WebOptimizerOptions> _options;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private IDisposable _callback;
        private bool _disposedValue;

        public WebOptimizerConfig(IConfiguration config, IOptionsMonitorCache<WebOptimizerOptions> options, IWebHostEnvironment hostingEnvironment)
        {
            _config = config;
            _options = options;
            _hostingEnvironment = hostingEnvironment;
        }

        public void Configure(WebOptimizerOptions options)
        {
            _callback = _config.GetReloadToken().RegisterChangeCallback(_ =>
            {
                _options.TryRemove(Options.DefaultName);
            }, null);

            _config.GetSection("WebOptimizer").Bind(options);

            options.EnableCaching ??= !_hostingEnvironment.IsDevelopment();
            options.EnableDiskCache ??= !_hostingEnvironment.IsDevelopment();
            options.EnableMemoryCache ??= true;
            options.EnableTagHelperBundling ??= true;
            options.CacheDirectory = string.IsNullOrWhiteSpace(options.CacheDirectory)
                ? Path.Combine(_hostingEnvironment.ContentRootPath, "obj", "WebOptimizerCache")
                : options.CacheDirectory;
            options.AllowEmptyBundle ??= false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _callback?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
