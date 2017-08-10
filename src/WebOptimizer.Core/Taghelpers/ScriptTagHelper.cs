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
    /// A TagHelper for hooking JavaScript bundles up to the HTML page.
    /// </summary>
    [HtmlTargetElement("script", Attributes = "src")]
    public class ScriptTagHelper : BaseTagHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptTagHelper"/> class.
        /// </summary>
        public ScriptTagHelper(IHostingEnvironment env, IMemoryCache cache, IAssetPipeline pipeline, IOptionsSnapshot<WebOptimizerOptions> options)
            : base(env, cache, pipeline, options)
        { }

        /// <summary>
        /// Synchronously executes the TagHelper
        /// </summary>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(output.TagName))
            {
                // output.SuppressOutput() was called by another TagHelper before this one
                return;
            }

            string src = context.AllAttributes["src"].Value.ToString();

            if (Pipeline.TryGetAssetFromRoute(src, out IAsset asset) && !output.Attributes.ContainsName("inline"))
            {
                Options.EnsureDefaults(HostingEnvironment);

                if (Options.EnableTagHelperBundling == true)
                {
                    src = GenerateHash(asset);
                    output.Attributes.SetAttribute("src", src);
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

            foreach (TagHelperAttribute item in output.Attributes.Where(a => !a.Name.Equals("src", StringComparison.OrdinalIgnoreCase)))
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
                string src = AddFileVersionToPath(file);
                output.PostElement.AppendHtml($"<script src=\"{src}\" {string.Join(" ", attrs)}></script>");
            }
        }
    }
}
