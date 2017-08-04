using System.Threading.Tasks;
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
    public abstract class BaseTagHelper : TagHelper
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
        /// Synchronously executes the <see cref="T:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper" /> with the given <paramref name="context" /> and
        /// <paramref name="output" />.
        /// </summary>
        /// <param name="context">Contains information associated with the current HTML tag.</param>
        /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
        public sealed override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(output.TagName))
            {
                // output.SuppressOutput() was called by another TagHelper before this one
                return;
            }

            ProcessSafe(context, output);
        }

        /// <summary>
        /// Asynchronously executes the <see cref="T:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper" /> with the given <paramref name="context" /> and
        /// <paramref name="output" />.
        /// </summary>
        /// <param name="context">Contains information associated with the current HTML tag.</param>
        /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task" /> that on completion updates the <paramref name="output" />.
        /// </returns>
        /// <remarks>
        /// By default this calls into <see cref="M:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper.Process(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext,Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput)" />.
        /// </remarks>
        /// .
        public sealed override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(output.TagName))
            {
                // output.SuppressOutput() was called by another TagHelper before this one
                return Task.CompletedTask;
            }

            return ProcessSafeAsync(context, output);
        }

        /// <summary>
        /// Synchronously executes the <see cref="T:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper" /> with the given <paramref name="context" /> and
        /// <paramref name="output" />.
        /// </summary>
        /// <param name="context">Contains information associated with the current HTML tag.</param>
        /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
        public virtual void ProcessSafe(TagHelperContext context, TagHelperOutput output)
        {
            // nothing
        }

        /// <summary>
        /// Synchronously executes the <see cref="T:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper" /> with the given <paramref name="context" /> and
        /// <paramref name="output" />.
        /// </summary>
        /// <param name="context">Contains information associated with the current HTML tag.</param>
        /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
        public virtual Task ProcessSafeAsync(TagHelperContext context, TagHelperOutput output)
        {
            return Task.CompletedTask;
        }

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
