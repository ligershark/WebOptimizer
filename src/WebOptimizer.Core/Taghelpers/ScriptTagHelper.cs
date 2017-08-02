using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;

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
        public ScriptTagHelper(IHostingEnvironment env, IMemoryCache cache, IAssetPipeline pipeline)
            : base(env, cache, pipeline)
        { }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public string Src { get; set; }

        /// <summary>
        /// Synchronously executes the TagHelper
        /// </summary>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Pipeline.TryFromRoute(Src, out IAsset asset) && !output.Attributes.ContainsName("inline"))
            {
                Pipeline.EnsureDefaults(HostingEnvironment);

                if (Pipeline.EnableTagHelperBundling == true)
                {
                    string src = GenerateHash(asset);
                    output.Attributes.SetAttribute("src", src);
                }
                else
                {
                    WriteIndividualTags(output, asset);
                }
            }
            else
            {
                base.Process(context, output);
            }
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
                string src = AddFileVersionToPath(file);
                output.PostElement.AppendHtml($"<script src=\"{src}\" {string.Join(" ", attrs)}></script>");
            }
        }
    }
}
