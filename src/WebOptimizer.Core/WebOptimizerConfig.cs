using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace WebOptimizer
{
    internal class WebOptimizerConfig : IConfigureOptions<WebOptimizerOptions>
    {
        private IConfiguration _config;
        private IOptionsMonitorCache<WebOptimizerOptions> _options;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public WebOptimizerConfig(IConfiguration config, IOptionsMonitorCache<WebOptimizerOptions> options, IWebHostEnvironment hostingEnvironment)
        {
            _config = config;
            _options = options;
            _hostingEnvironment = hostingEnvironment;
        }

        public void Configure(WebOptimizerOptions options)
        {
            _config.GetReloadToken().RegisterChangeCallback(_ =>
            {
                _options.TryRemove(Options.DefaultName);
            }, null);

            _config.GetSection("WebOptimizer").Bind(options);

            options.EnableCaching = options.EnableCaching ?? !_hostingEnvironment.IsDevelopment();
            options.EnableDiskCache = options.EnableDiskCache ?? !_hostingEnvironment.IsDevelopment();
            options.EnableMemoryCache = options.EnableMemoryCache ?? true;
            options.EnableTagHelperBundling = options.EnableTagHelperBundling ?? true;
            options.CacheDirectory = string.IsNullOrWhiteSpace(options.CacheDirectory)
                ? Path.Combine(_hostingEnvironment.ContentRootPath, "obj", "WebOptimizerCache")
                : options.CacheDirectory;
            options.AllowEmptyBundle = options.AllowEmptyBundle ?? false;
        }
    }
}
