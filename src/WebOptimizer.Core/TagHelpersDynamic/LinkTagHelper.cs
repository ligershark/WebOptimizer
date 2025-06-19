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
/// <see cref="ITagHelper"/> implementation targeting &lt;link&gt; elements that supports fallback
/// href paths.
/// </summary>
/// <remarks>The tag helper won't process for cases with just the 'href' attribute.</remarks>
/// <remarks>Creates a new <see cref="LinkTagHelper"/>.</remarks>
/// <param name="pipeline"></param>
/// <param name="hostingEnvironment">The <see cref="IWebHostEnvironment"/>.</param>
/// <param name="cache">The <see cref="IMemoryCache"/>.</param>
/// <param name="fileVersionProvider"></param>
/// <param name="htmlEncoder">The <see cref="HtmlEncoder"/>.</param>
/// <param name="javaScriptEncoder">The <see cref="JavaScriptEncoder"/>.</param>
/// <param name="urlHelperFactory">The <see cref="IUrlHelperFactory"/>.</param>
/// <param name="webOptimizerOptions"></param>
[HtmlTargetElement(TagName, Attributes = HrefIncludeAttributeName, TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement(TagName, Attributes = HrefExcludeAttributeName, TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement(TagName, Attributes = FallbackHrefAttributeName, TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement(TagName, Attributes = FallbackHrefIncludeAttributeName, TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement(TagName, Attributes = FallbackHrefExcludeAttributeName, TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement(TagName, Attributes = FallbackTestClassAttributeName, TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement(TagName, Attributes = FallbackTestPropertyAttributeName, TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement(TagName, Attributes = FallbackTestValueAttributeName, TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement(TagName, Attributes = AppendVersionAttributeName, TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement(TagName, Attributes = BundleKeyName)]
[HtmlTargetElement(TagName, Attributes = BundleDestinationKeyName)]
public class LinkTagHelper(
    IAssetPipeline pipeline,
    IWebHostEnvironment hostingEnvironment,
    TagHelperMemoryCacheProvider cache,
    IFileVersionProvider fileVersionProvider,
    HtmlEncoder htmlEncoder,
    JavaScriptEncoder javaScriptEncoder,
    IUrlHelperFactory urlHelperFactory,
    IOptionsSnapshot<WebOptimizerOptions> webOptimizerOptions) : Microsoft.AspNetCore.Mvc.TagHelpers.LinkTagHelper(hostingEnvironment, cache, fileVersionProvider, htmlEncoder, javaScriptEncoder, urlHelperFactory)
{
    private const string AppendVersionAttributeName = "asp-append-version";
    private const string BundleDestinationKeyName = "asp-bundle-dest-key";
    private const string BundleKeyName = "asp-bundle-key";
    private const string FallbackHrefAttributeName = "asp-fallback-href";
    private const string FallbackHrefExcludeAttributeName = "asp-fallback-href-exclude";
    private const string FallbackHrefIncludeAttributeName = "asp-fallback-href-include";
    private const string FallbackTestClassAttributeName = "asp-fallback-test-class";
    private const string FallbackTestPropertyAttributeName = "asp-fallback-test-property";
    private const string FallbackTestValueAttributeName = "asp-fallback-test-value";
    private const string HrefExcludeAttributeName = "asp-href-exclude";
    private const string HrefIncludeAttributeName = "asp-href-include";
    private const string TagName = "link";
    private readonly IAssetPipeline _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
    private readonly WebOptimizerOptions _webOptimizerOptions = webOptimizerOptions?.Value ?? throw new ArgumentNullException(nameof(webOptimizerOptions));

    /// <summary>
    /// Gets or sets the destination key for the bundle.
    /// </summary>
    [HtmlAttributeName(BundleDestinationKeyName)]
    public string BundleDestinationKey { get; set; } = default!;

    /// <summary>
    /// Gets or sets the key for the bundle.
    /// </summary>
    [HtmlAttributeName(BundleKeyName)]
    public string BundleKey { get; set; } = default!;

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        if (!output.HandleCssBundle(
                _pipeline,
                ViewContext,
                _webOptimizerOptions, Href, BundleKey, BundleDestinationKey))
        {
            base.Process(context, output);
        }
    }
}
