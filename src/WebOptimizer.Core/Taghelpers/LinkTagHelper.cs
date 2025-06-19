using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace WebOptimizer.Taghelpers;

/// <summary>
/// A TagHelper for hooking CSS bundles up to the HTML page.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="LinkTagHelper"/> class.
/// </remarks>
[HtmlTargetElement("link", Attributes = "href, [rel=stylesheet]")]
[HtmlTargetElement("link", Attributes = "href, [rel=preload]")]
[HtmlTargetElement("link", Attributes = "href, [rel=prefetch]")]
public class LinkTagHelper(
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
    /// <param name="context">Contains information associated with the current HTML tag.</param>
    /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(output.TagName))
        {
            return;
        }

        if (output.Attributes.ContainsName("inline"))
        {
            return;
        }

        string href = GetValue("href", output, out bool encoded);
        string? pathBase = CurrentViewContext.HttpContext?.Request?.PathBase.Value;

        if (!string.IsNullOrEmpty(pathBase) && href.StartsWith(pathBase))
        {
            href = href[pathBase.Length..];
        }

        if (Pipeline.TryGetAssetFromRoute(href, out IAsset asset))
        {
            if (Options.EnableTagHelperBundling == true)
            {
                href = AddCdn(AddPathBase(GenerateHash(asset)));
                output.Attributes.SetAttribute("href", href);
            }
            else
            {
                WriteIndividualTags(output, asset);
            }
        }
        else
        {
            if (!Uri.TryCreate(href, UriKind.Absolute, out var _))
            {
                string unmodifiedHref = href;
                href = AddCdn(AddPathBase(href));
                if (href != unmodifiedHref)
                {
                    object value = encoded ? new HtmlString(href) : href;
                    output.Attributes.SetAttribute("href", value);
                }
            }
        }
    }

    private void WriteIndividualTags(TagHelperOutput output, IAsset asset)
    {
        output.SuppressOutput();

        var attrs = new List<string>();

        foreach (TagHelperAttribute item in output.Attributes.Where(a => !a.Name.Equals("href", StringComparison.OrdinalIgnoreCase)))
        {
            StringBuilder sb = new();
            sb.Append(item.Name);

            if (item.ValueStyle != HtmlAttributeValueStyle.Minimized)
            {
                string quote = GetQuote(item.ValueStyle);
                sb.Append($"={quote}{item.Value}{quote}");
            }

            attrs.Add(sb.ToString());
        }

        IEnumerable<string> sourceFiles = Asset.ExpandGlobs(asset, HostingEnvironment);

        foreach (string file in sourceFiles)
        {
            string fileToAdd = file;
            if (Path.GetExtension(file) == ".scss")
            {
                if (Path.GetFileName(file).StartsWith('_')) continue;

                fileToAdd = Path.ChangeExtension(file, "css");
            }

            string href = AddCdn(AddPathBase(AddFileVersionToPath(fileToAdd, asset)));
            output.PostElement.AppendHtml($"<link href=\"{href}\" {string.Join(" ", attrs)} />{Environment.NewLine}");
        }
    }

    internal static string? GetValue(string attrName, TagHelperOutput output, out bool encoded)
    {
        encoded = false;
        if (string.IsNullOrEmpty(attrName) || !output.Attributes.TryGetAttribute(attrName, out var attr))
        {
            return null;
        }

        if (attr.Value is string stringValue)
        {
            return stringValue;
        }
        else if (attr.Value is IHtmlContent content)
        {
            encoded = true;
            if (content is HtmlString htmlString)
            {
                return htmlString.ToString();
            }

            using var writer = new StringWriter();
            content.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }

        return null;
    }
}
