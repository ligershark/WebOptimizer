using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WebOptimizer.Extensions;

namespace WebOptimizer.TagHelpersDynamic;

/// <summary>
/// <see cref="ITagHelper"/> implementation targeting &lt;script&gt; elements that supports fallback
/// src paths.
/// </summary>
/// <remarks>The tag helper won't process for cases with just the 'src' attribute.</remarks>
/// <remarks>Creates a new <see cref="ScriptTagHelper"/>.</remarks>
/// <param name="pipeline"></param>
/// <param name="hostingEnvironment">The <see cref="IWebHostEnvironment"/>.</param>
/// <param name="cache">The <see cref="IMemoryCache"/>.</param>
/// <param name="fileVersionProvider"></param>
/// <param name="htmlEncoder">The <see cref="HtmlEncoder"/>.</param>
/// <param name="javaScriptEncoder">The <see cref="JavaScriptEncoder"/>.</param>
/// <param name="urlHelperFactory">The <see cref="IUrlHelperFactory"/>.</param>
/// <param name="webOptimizerOptions">The web optimizer options.</param>
[HtmlTargetElement(TagName, Attributes = SrcIncludeAttributeName)]
[HtmlTargetElement(TagName, Attributes = SrcExcludeAttributeName)]
[HtmlTargetElement(TagName, Attributes = FallbackSrcAttributeName)]
[HtmlTargetElement(TagName, Attributes = FallbackSrcIncludeAttributeName)]
[HtmlTargetElement(TagName, Attributes = FallbackSrcExcludeAttributeName)]
[HtmlTargetElement(TagName, Attributes = FallbackTestExpressionAttributeName)]
[HtmlTargetElement(TagName, Attributes = AppendVersionAttributeName)]
[HtmlTargetElement(TagName, Attributes = BundleKeyName)]
[HtmlTargetElement(TagName, Attributes = BundleDestinationKeyName)]
public class ScriptTagHelper(
    IAssetPipeline pipeline,
    IWebHostEnvironment hostingEnvironment,
    TagHelperMemoryCacheProvider cache,
    IFileVersionProvider fileVersionProvider,
    HtmlEncoder htmlEncoder,
    JavaScriptEncoder javaScriptEncoder,
    IUrlHelperFactory urlHelperFactory,
    IOptionsSnapshot<WebOptimizerOptions> webOptimizerOptions)
    : Microsoft.AspNetCore.Mvc.TagHelpers.ScriptTagHelper(
        hostingEnvironment,
        cache,
        fileVersionProvider,
        htmlEncoder,
        javaScriptEncoder,
        urlHelperFactory)
{
    private const string AppendVersionAttributeName = "asp-append-version";
    private const string BundleDestinationKeyName = "asp-bundle-dest-key";
    private const string BundleKeyName = "asp-bundle-key";
    private const string FallbackSrcAttributeName = "asp-fallback-src";
    private const string FallbackSrcExcludeAttributeName = "asp-fallback-src-exclude";
    private const string FallbackSrcIncludeAttributeName = "asp-fallback-src-include";
    private const string FallbackTestExpressionAttributeName = "asp-fallback-test";
    private const string SrcExcludeAttributeName = "asp-src-exclude";
    private const string SrcIncludeAttributeName = "asp-src-include";
    private const string TagName = "script";
    private readonly IAssetPipeline _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
    private readonly WebOptimizerOptions _webOptimizerOptions = webOptimizerOptions?.Value ?? throw new ArgumentNullException(nameof(webOptimizerOptions));

    /// <summary>
    /// Gets or sets the destination key for the bundle.
    /// </summary>
    [HtmlAttributeName(BundleDestinationKeyName)]
    public string BundleDestinationKey { get; set; } = default!;

    /// <summary>
    /// Gets or sets the key of the bundle.
    /// </summary>
    [HtmlAttributeName(BundleKeyName)]
    public string BundleKey { get; set; } = default!;

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (!output.HandleJsBundle(
                _pipeline,
                ViewContext,
                _webOptimizerOptions, Src, BundleKey, BundleDestinationKey))
        {
            base.Process(context, output);
        }
    }
}
