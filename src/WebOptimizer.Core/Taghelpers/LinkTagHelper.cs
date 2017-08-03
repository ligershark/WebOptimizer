using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;

namespace WebOptimizer.Taghelpers
{
    /// <summary>
    /// A TagHelper for hooking CSS bundles up to the HTML page.
    /// </summary>
    [HtmlTargetElement("link", Attributes = "href, [rel=stylesheet]")]
    [HtmlTargetElement("link", Attributes = "href, [rel=preload]")]
    [HtmlTargetElement("link", Attributes = "href, [rel=prefetch]")]
    public class LinkTagHelper : BaseTagHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkTagHelper"/> class.
        /// </summary>
        public LinkTagHelper(IHostingEnvironment env, IMemoryCache cache, IAssetPipeline pipeline)
            : base(env, cache, pipeline)
        { }

        /// <summary>
        /// Gets or sets the href attribute.
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// Synchronously executes the TagHelper
        /// </summary>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Pipeline.TryFromRoute(Href, out IAsset asset) && !output.Attributes.ContainsName("inline"))
            {
                Pipeline.EnsureDefaults(HostingEnvironment);

                if (Pipeline.EnableTagHelperBundling == true)
                {
                    string href = GenerateHash(asset);
                    output.Attributes.SetAttribute("href", href);
                }
                else
                {
                    WriteIndividualTags(output, asset);
                }
            }

            base.Process(context, output);
        }

        private void WriteIndividualTags(TagHelperOutput output, IAsset asset)
        {
            output.SuppressOutput();

            var attrs = new List<string>();

            foreach (TagHelperAttribute item in output.Attributes)
            {
                string attr = item.Name;

                if (item.ValueStyle != HtmlAttributeValueStyle.Minimized)
                {
                    string quote = GetQuote(item.ValueStyle);
                    attr += "=" + quote + item.Value + quote;
                }

                attrs.Add(attr);
            }

            foreach (string file in asset.SourceFiles)
            {
                string href = AddFileVersionToPath(file);
                output.PostElement.AppendHtml($"<link href=\"{href}\" {string.Join(" ", attrs)} />");
            }
        }
    }
}
