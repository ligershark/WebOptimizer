using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace WebOptimizer.Taghelpers
{
    /// <summary>
    /// A base class for TagHelpers
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Razor.TagHelpers.TagHelper" />
    public abstract class BaseTagHelper : TagHelper
    {
        private FileVersionProvider _fileProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTagHelper"/> class.
        /// </summary>
        public BaseTagHelper(IWebHostEnvironment env, IMemoryCache cache, IAssetPipeline pipeline, IOptionsMonitor<WebOptimizerOptions> options)
        {
            HostingEnvironment = env;
            Cache = cache;
            Pipeline = pipeline;
            Options = options.CurrentValue;
        }

        /// <summary>
        /// Gets the hosting environment.
        /// </summary>
        protected IWebHostEnvironment HostingEnvironment { get; }

        /// <summary>
        /// The cache object.
        /// </summary>
        protected IMemoryCache Cache { get; }

        /// <summary>
        /// Gets the pipeline.
        /// </summary>
        protected IAssetPipeline Pipeline { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        protected IWebOptimizerOptions Options { get; }

        /// <summary>
        /// Makes sure this taghelper runs before the built in ones.
        /// </summary>
        public override int Order => 10;

        /// <summary>
        /// Gets or sets the view context.
        /// </summary>
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Gets the quote character.
        /// </summary>
        protected static string GetQuote(HtmlAttributeValueStyle style)
        {
            switch (style)
            {
                case HtmlAttributeValueStyle.DoubleQuotes:
                    return "\"";
                case HtmlAttributeValueStyle.SingleQuotes:
                    return "'";
            }

            return string.Empty;
        }

        /// <summary>
        /// Generates a has of the file.
        /// </summary>
        protected string AddFileVersionToPath(string fileName, IAsset asset)
        {
            if (_fileProvider == null)
            {
                _fileProvider = new FileVersionProvider(
                    asset.GetFileProvider(HostingEnvironment),
                    Cache,
                    ViewContext.HttpContext.Request.PathBase);
            }

            return _fileProvider.AddFileVersionToPath(fileName);
        }

        /// <summary>
        /// Adds current the PathBase to a Url
        /// </summary>
        protected string AddPathBase(string url)
        {
            var pathBase = ViewContext.HttpContext.Request.PathBase;
            if (string.IsNullOrEmpty(pathBase))
                return url;

            return pathBase + (url.StartsWith("/") ? url : ("/" + url));
        }

        /// <summary>
        /// Generates a has of the files in the bundle.
        /// </summary>
        protected string GenerateHash(IAsset asset)
        {
            string hash = asset.GenerateCacheKey(ViewContext.HttpContext, Options);

            return $"{asset.Route}?v={hash}";
        }

        /// <summary>
        /// Adds string value to memory cache.
        /// </summary>
        protected void AddToCache(string cacheKey, string value, IFileProvider fileProvider, params string[] files)
        {
            var cacheOptions = new MemoryCacheEntryOptions();

            foreach (string file in files)
            {
                cacheOptions.AddExpirationToken(fileProvider.Watch(file));
            }

            Cache.Set(cacheKey, value, cacheOptions);
        }
    }
}
