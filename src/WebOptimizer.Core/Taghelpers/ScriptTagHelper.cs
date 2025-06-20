using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace WebOptimizer.Taghelpers;

/// <summary>
/// A TagHelper for hooking JavaScript bundles up to the HTML page.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ScriptTagHelper"/> class.
/// </remarks>
[HtmlTargetElement("script")]
public class ScriptTagHelper(
    IWebHostEnvironment env,
    IMemoryCache cache,
    IAssetPipeline pipeline,
    IOptionsMonitor<WebOptimizerOptions> options)
    : BaseTagHelper(env, cache, pipeline, options)
{

    /// <summary>
    /// For HttpContext Access
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext CurrentViewContext { get; set; } = default!;

    /// <summary>
    /// Synchronously executes the TagHelper
    /// </summary>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(output.TagName))
        {
            return;
        }

        string? src = LinkTagHelper.GetValue("src", output, out bool encoded );

        if (string.IsNullOrEmpty(src))
        {
            return;
        }

        string? pathBase = null;

        if (CurrentViewContext.HttpContext.Request.PathBase.HasValue)
        {
            pathBase = CurrentViewContext.HttpContext.Request.PathBase.Value;
        }

        if (pathBase is not null && src.StartsWith(pathBase))
        {
            src = src[pathBase.Length..];
        }

        if (Pipeline.TryGetAssetFromRoute(src, out var asset) && !output.Attributes.ContainsName("inline"))
        {
            if (Options.EnableTagHelperBundling == true)
            {
                src = AddCdn(AddPathBase(GenerateHash(asset)));
                output.Attributes.SetAttribute("src", src);
            }
            else
            {
                WriteIndividualTags(output, asset);
            }
        }
        else
        {
            if (!Uri.TryCreate(src, UriKind.Absolute, out var _))
            {
                src = AddCdn(AddPathBase(src));
                object? value = encoded ? new HtmlString(src) : src;
                output.Attributes.SetAttribute("src", value);
            }
        }
    }

    private void WriteIndividualTags(TagHelperOutput output, IAsset asset)
    {
        output.SuppressOutput();

        var attrs = new List<string>();

        foreach (var item in output.Attributes.Where(a => !a.Name.Equals("src", StringComparison.OrdinalIgnoreCase)))
        {
            string attr = item.Name;

            if (item.ValueStyle != HtmlAttributeValueStyle.Minimized)
            {
                string quote = GetQuote(item.ValueStyle);
                attr += $"={quote}{item.Value}{quote}";
            }

            attrs.Add(attr);
        }

        var sourceFiles = Asset.ExpandGlobs(asset, HostingEnvironment);

        foreach (string file in sourceFiles)
        {
            string? src = AddCdn(AddPathBase(AddFileVersionToPath(file, asset)));
            output.PostElement.AppendHtml($"<script src=\"{src}\" {string.Join(" ", attrs)}></script>{Environment.NewLine}");
        }
    }
}
