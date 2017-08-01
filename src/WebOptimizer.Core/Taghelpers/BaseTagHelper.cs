using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers.Internal;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;

namespace WebOptimizer.Taghelpers
{
    /// <summary>
    /// A base class for TagHelpers
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Razor.TagHelpers.TagHelper" />
    public class BaseTagHelper : TagHelper
    {
        private FileVersionProvider _fileProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTagHelper"/> class.
        /// </summary>
        public BaseTagHelper(IHostingEnvironment env, IMemoryCache cache, IAssetPipeline pipeline)
        {
            HostingEnvironment = env;
            Cache = cache;
            Pipeline = pipeline;
        }

        /// <summary>
        /// Gets the hosting environment.
        /// </summary>
        protected IHostingEnvironment HostingEnvironment { get; }

        /// <summary>
        /// The cache object.
        /// </summary>
        protected IMemoryCache Cache { get; }

        /// <summary>
        /// Gets the pipeline.
        /// </summary>
        protected IAssetPipeline Pipeline { get; }

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
        protected string AddFileVersionToPath(string fileName)
        {
            if (_fileProvider == null)
            {
                Pipeline.EnsureDefaults(HostingEnvironment);
                _fileProvider = new FileVersionProvider(
                    Pipeline.FileProvider,
                    Cache,
                    ViewContext.HttpContext.Request.PathBase);
            }

            return _fileProvider.AddFileVersionToPath(fileName);
        }

        /// <summary>
        /// Generates a has of the files in the bundle.
        /// </summary>
        protected string GenerateHash(IAsset asset)
        {
            string hash = asset.GenerateCacheKey(ViewContext.HttpContext);

            return $"{asset.Route}?v={hash}";
        }
    }
}
