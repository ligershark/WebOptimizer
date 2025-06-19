using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace WebOptimizer.Taghelpers;

/// <summary>
/// A base class for TagHelpers
/// </summary>
/// <param name="env">The env.</param>
/// <param name="cache">The cache.</param>
/// <param name="pipeline">The pipeline.</param>
/// <param name="options">The options.</param>
/// <seealso cref="TagHelper" />
/// <remarks>Initializes a new instance of the <see cref="BaseTagHelper" /> class.</remarks>
public abstract class BaseTagHelper(IWebHostEnvironment env, IMemoryCache cache, IAssetPipeline pipeline, IOptionsMonitor<WebOptimizerOptions> options) : TagHelper
{
    private FileVersionProvider? _fileProvider;

    /// <summary>
    /// Gets the hosting environment.
    /// </summary>
    protected IWebHostEnvironment HostingEnvironment { get; } = env;

    /// <summary>
    /// The cache object.
    /// </summary>
    protected IMemoryCache Cache { get; } = cache;

    /// <summary>
    /// Gets the pipeline.
    /// </summary>
    protected IAssetPipeline Pipeline { get; } = pipeline;

    /// <summary>
    /// Gets the options.
    /// </summary>
    protected IWebOptimizerOptions Options { get; } = options.CurrentValue;

    /// <summary>
    /// Makes sure this taghelper runs before the built in ones.
    /// </summary>
    public override int Order => 10;

    /// <summary>
    /// Gets or sets the view context.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = default!;

    /// <summary>
    /// Gets the quote character.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <returns>System.String.</returns>
    protected static string GetQuote(HtmlAttributeValueStyle style) =>
        style switch
        {
            HtmlAttributeValueStyle.DoubleQuotes => "\"",
            HtmlAttributeValueStyle.SingleQuotes => "'",
            _ => string.Empty,
        };

    /// <summary>
    /// Generates a has of the file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="asset">The asset.</param>
    /// <returns>System.String.</returns>
    protected string AddFileVersionToPath(string fileName, IAsset asset)
    {
        _fileProvider ??= new FileVersionProvider(
                asset.GetFileProvider(HostingEnvironment),
                Cache,
                ViewContext!.HttpContext.Request.PathBase);

        return _fileProvider.AddFileVersionToPath(fileName);
    }

    /// <summary>
    /// Adds current the PathBase to a Url
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>System.String.</returns>
    protected string AddPathBase(string url)
    {
        Microsoft.AspNetCore.Http.PathString pathBase = ViewContext.HttpContext.Request.PathBase;
        return string.IsNullOrEmpty(pathBase) ? url : pathBase + (url.StartsWith('/') ? url : ($"/{url}"));
    }

    /// <summary>
    /// Generates a has of the files in the bundle.
    /// </summary>
    /// <param name="asset">The asset.</param>
    /// <returns>System.String.</returns>
    protected string GenerateHash(IAsset asset)
    {
        string hash = asset.GenerateCacheKey(ViewContext.HttpContext, Options);

        return $"{asset.Route}?v={hash}";
    }

    /// <summary>
    /// Adds string value to memory cache.
    /// </summary>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="value">The value.</param>
    /// <param name="fileProvider">The file provider.</param>
    /// <param name="files">The files.</param>
    protected void AddToCache(string cacheKey, string value, IFileProvider fileProvider, params string[] files)
    {
        var cacheOptions = new MemoryCacheEntryOptions();

        foreach (string file in files)
        {
            cacheOptions.AddExpirationToken(fileProvider.Watch(file));
        }

        Cache.Set(cacheKey, value, cacheOptions);
    }

    /// <summary>
    /// Adds the CdnUrl defined in Options to a Url
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>System.String.</returns>
    protected string AddCdn(string url) =>
        string.IsNullOrEmpty(Options.CdnUrl) ? url : $"{Options.CdnUrl}{(url.StartsWith('/') ? url : $"/{url}")}";
}
