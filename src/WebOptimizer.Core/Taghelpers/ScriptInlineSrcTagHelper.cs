using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace WebOptimizer.Taghelpers;

/// <summary>
/// Tag helper for inlining CSS
/// </summary>
/// <remarks>
/// Tag helper for inlining content
/// </remarks>
/// <param name="env">The env.</param>
/// <param name="cache">The cache.</param>
/// <param name="pipeline">The pipeline.</param>
/// <param name="options">The options.</param>
/// <param name="builder">The builder.</param>
[HtmlTargetElement("script", Attributes = "inline")]
public class ScriptInlineSrcTagHelper(
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
    /// Gets or sets the src attribute
    /// </summary>
    public string? Src { get; set; }

    /// <summary>
    /// Creates a tag helper for inlining content
    /// </summary>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(output.TagName))
        {
            // output.SuppressOutput() was called by another TagHelper before this one
            return;
        }

        if (!string.IsNullOrEmpty(Src))
        {
            output.Attributes.RemoveAll("inline");
            output.Attributes.RemoveAll("integrity");
            output.Attributes.RemoveAll("language");
            output.Attributes.RemoveAll("src");
            output.Attributes.RemoveAll("async");
            output.Attributes.RemoveAll("defer");

            string route = AssetPipeline.NormalizeRoute(Src);
            string? content = await GetFileContentAsync(route);

            output.Content.SetHtmlContent(content);
            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }

    /// <summary>
    /// Gets the file content asynchronous.
    /// </summary>
    /// <exception cref="FileNotFoundException">File or bundle doesn't exist</exception>
    protected async Task<string?> GetFileContentAsync(string route)
    {
        if (Pipeline.TryGetAssetFromRoute(route, out var asset))
        {
            var response = await builder.BuildAsync(asset, ViewContext.HttpContext, Options);
            return response.Body.AsString();
        }

        string cacheKey = $"_WO_{route}";

        if (Cache.TryGetValue(cacheKey, out string? content))
        {
            return content;
        }

        string cleanRoute = route.TrimStart('~');
        string? file = HostingEnvironment.WebRootFileProvider.GetFileInfo(cleanRoute).PhysicalPath;

        if (File.Exists(file))
        {
            using var reader = File.OpenText(file);
            content = await reader.ReadToEndAsync();
            AddToCache(cacheKey, content, HostingEnvironment.WebRootFileProvider, cleanRoute);

            return content;
        }

        throw new FileNotFoundException("File or bundle doesn't exist", route);
    }
}
