using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace WebOptimizer.Taghelpers;

/// <summary>
/// Tag helper for inlining CSS
/// </summary>
/// <param name="env">The env.</param>
/// <param name="cache">The cache.</param>
/// <param name="pipeline">The pipeline.</param>
/// <param name="options">The options.</param>
/// <param name="builder">The builder.</param>
/// <remarks>Tag helper for inlining content</remarks>
[HtmlTargetElement("link", Attributes = "inline, href")]
public class LinkInlineHrefTagHelper(
    IWebHostEnvironment env,
    IMemoryCache cache,
    IAssetPipeline pipeline,
    IOptionsMonitor<WebOptimizerOptions> options,
    IAssetBuilder builder)
    : BaseTagHelper(env, cache, pipeline, options)
{
    /// <summary>
    /// Makes sure this taghelper runs before the built in ones.
    /// </summary>
    public override int Order => base.Order + 1;

    /// <summary>
    /// Gets or sets the href attribute
    /// </summary>
    public string? Href { get; set; }

    /// <summary>
    /// Creates a tag helper for inlining content
    /// </summary>
    /// <param name="context">Contains information associated with the current HTML tag.</param>
    /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    /// .
    /// <remarks>By default this calls into <see cref="M:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper.Process(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext,Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput)" />.</remarks>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(output.TagName))
        {
            return;
        }

        if (string.IsNullOrEmpty(Href))
        {
            return;
        }

        TagHelperAttribute? scoped = GetScoped(output);
        output.TagName = "style";
        output.Attributes.Clear();

        // Make sure to use add the scope attribute if the user specified it
        if (scoped is not null)
        {
            output.Attributes.Add(scoped);
        }

        string route = AssetPipeline.NormalizeRoute(Href);
        string? content = await GetFileContentAsync(route);

        output.Content.SetHtmlContent(content);
        output.TagMode = TagMode.StartTagAndEndTag;
    }

    private static TagHelperAttribute? GetScoped(TagHelperOutput output) =>
        output.Attributes.TryGetAttribute("scoped", out TagHelperAttribute? attr) ? attr : null;

    private async Task<string?> GetFileContentAsync(string route)
    {
        if (Pipeline.TryGetAssetFromRoute(route, out IAsset asset))
        {
            IAssetResponse response = await builder.BuildAsync(asset, ViewContext.HttpContext, Options);
            return response.Body.AsString();
        }

        string cacheKey = $"_WO_{route}";

        if (Cache.TryGetValue(cacheKey, out string? content))
        {
            return content;
        }

        string cleanRoute = route.TrimStart('~');
        string? file = HostingEnvironment.WebRootFileProvider.GetFileInfo(cleanRoute).PhysicalPath;

        if (file is not null && File.Exists(file))
        {
            using StreamReader reader = File.OpenText(file);
            content = await reader.ReadToEndAsync();
            AddToCache(cacheKey, content, HostingEnvironment.WebRootFileProvider, cleanRoute);

            return content;
        }

        throw new FileNotFoundException("File or bundle doesn't exist", route);
    }
}
