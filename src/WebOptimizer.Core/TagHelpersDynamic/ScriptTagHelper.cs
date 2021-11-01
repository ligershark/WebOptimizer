using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WebOptimizer.Extensions;

namespace WebOptimizer.TagHelpersDynamic
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;script&gt; elements that supports fallback src paths.
    /// </summary>
    /// <remarks>
    /// The tag helper won't process for cases with just the 'src' attribute.
    /// </remarks>
    [HtmlTargetElement(TagName, Attributes = SrcIncludeAttributeName)]
    [HtmlTargetElement(TagName, Attributes = SrcExcludeAttributeName)]
    [HtmlTargetElement(TagName, Attributes = FallbackSrcAttributeName)]
    [HtmlTargetElement(TagName, Attributes = FallbackSrcIncludeAttributeName)]
    [HtmlTargetElement(TagName, Attributes = FallbackSrcExcludeAttributeName)]
    [HtmlTargetElement(TagName, Attributes = FallbackTestExpressionAttributeName)]
    [HtmlTargetElement(TagName, Attributes = AppendVersionAttributeName)]
    [HtmlTargetElement(TagName, Attributes = BundleKeyName)]
    [HtmlTargetElement(TagName, Attributes = BundleDestinationKeyName)]
    public class ScriptTagHelper : Microsoft.AspNetCore.Mvc.TagHelpers.ScriptTagHelper
    {
        private const string TagName = "script";
        private const string SrcIncludeAttributeName = "asp-src-include";
        private const string SrcExcludeAttributeName = "asp-src-exclude";
        private const string FallbackSrcAttributeName = "asp-fallback-src";
        private const string FallbackSrcIncludeAttributeName = "asp-fallback-src-include";
        private const string FallbackSrcExcludeAttributeName = "asp-fallback-src-exclude";
        private const string FallbackTestExpressionAttributeName = "asp-fallback-test";
        private const string AppendVersionAttributeName = "asp-append-version";

        private const string BundleDestinationKeyName = "asp-bundle-dest-key";
        private const string BundleKeyName = "asp-bundle-key";

        private readonly WebOptimizerOptions _webOptimizerOptions;
        private readonly IAssetPipeline _pipeline;

        /// <summary>
        ///
        /// </summary>
        [HtmlAttributeName(BundleKeyName)]
        public string BundleKey { get; set; }

        /// <summary>
        ///
        /// </summary>
        [HtmlAttributeName(BundleDestinationKeyName)]
        public string BundleDestinationKey { get; set; }

        /// <summary>
        /// Creates a new <see cref="ScriptTagHelper"/>.
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="hostingEnvironment">The <see cref="IWebHostEnvironment"/>.</param>
        /// <param name="cache">The <see cref="IMemoryCache"/>.</param>
        /// <param name="fileVersionProvider"></param>
        /// <param name="htmlEncoder">The <see cref="HtmlEncoder"/>.</param>
        /// <param name="javaScriptEncoder">The <see cref="JavaScriptEncoder"/>.</param>
        /// <param name="urlHelperFactory">The <see cref="IUrlHelperFactory"/>.</param>
        /// <param name="webOptimizerOptions"></param>
        public ScriptTagHelper(
            IAssetPipeline pipeline,
            IWebHostEnvironment hostingEnvironment,
            TagHelperMemoryCacheProvider cache,
            IFileVersionProvider fileVersionProvider,
            HtmlEncoder htmlEncoder,
            JavaScriptEncoder javaScriptEncoder,
            IUrlHelperFactory urlHelperFactory,
            IOptionsSnapshot<WebOptimizerOptions> webOptimizerOptions)
            : base(hostingEnvironment, cache, fileVersionProvider, htmlEncoder, javaScriptEncoder, urlHelperFactory)
        {
            _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            _webOptimizerOptions = webOptimizerOptions?.Value ?? throw new ArgumentNullException(nameof(webOptimizerOptions));
        }

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (output == null) throw new ArgumentNullException(nameof(output));

            if (!output.HandleJsBundle(
                    _pipeline,
                    ViewContext,
                    _webOptimizerOptions, Src, BundleKey, BundleDestinationKey))
            {
                base.Process(context, output);
            }
        }
    }
}