using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

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
        public LinkTagHelper(IHostingEnvironment env, IMemoryCache cache, IAssetPipeline pipeline, IOptionsSnapshot<WebOptimizerOptions> options)
            : base(env, cache, pipeline, options)
        { }

        /// <summary>
        /// Synchronously executes the TagHelper
        /// </summary>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(output.TagName))
            {
                return;
            }

            string href = CdnTagHelper.GetValue("href", output);

            if (Pipeline.TryGetAssetFromRoute(href, out IAsset asset) && !output.Attributes.ContainsName("inline"))
            {
                Options.EnsureDefaults(HostingEnvironment);

                if (Options.EnableTagHelperBundling == true)
                {
                    href = GenerateHash(asset);
                    output.Attributes.SetAttribute("href", href);
                }
                else
                {
                    WriteIndividualTags(output, asset);
                }
            }
        }

        private void WriteIndividualTags(TagHelperOutput output, IAsset asset)
        {
            output.SuppressOutput();

            var attrs = new List<string>();

            foreach (TagHelperAttribute item in output.Attributes.Where(a => !a.Name.Equals("href", StringComparison.OrdinalIgnoreCase)))
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
                string href = AddFileVersionToPath(file, asset);
                output.PostElement.AppendHtml($"<link href=\"{href}\" {string.Join(" ", attrs)} />" + Environment.NewLine);
            }
        }
    }
}
