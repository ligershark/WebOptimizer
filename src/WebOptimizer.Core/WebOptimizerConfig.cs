using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace WebOptimizer;

internal class WebOptimizerConfig(
    IConfiguration config,
    IOptionsMonitorCache<WebOptimizerOptions> options,
    IWebHostEnvironment hostingEnvironment)
    : IConfigureOptions<WebOptimizerOptions>,
    IDisposable
{
    private readonly IOptionsMonitorCache<WebOptimizerOptions> _options = options;
    private IDisposable? _callback;
    private bool _disposed;

    public void Configure(WebOptimizerOptions options)
    {
        _callback = config.GetReloadToken().RegisterChangeCallback(
            _ =>
            {
                _ = _options.TryRemove(Options.DefaultName);
            },
            null);

        config.GetSection("WebOptimizer").Bind(options);

        options.EnableCaching ??= !hostingEnvironment.IsDevelopment();
        options.EnableDiskCache ??= !hostingEnvironment.IsDevelopment();
        options.EnableMemoryCache ??= true;
        options.EnableTagHelperBundling ??= true;
        options.CacheDirectory = string.IsNullOrWhiteSpace(options.CacheDirectory)
            ? Path.Combine(hostingEnvironment.ContentRootPath, "obj", "WebOptimizerCache")
            : options.CacheDirectory;
        options.AllowEmptyBundle ??= false;
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _callback?.Dispose();
        }

        _disposed = true;
    }
}
