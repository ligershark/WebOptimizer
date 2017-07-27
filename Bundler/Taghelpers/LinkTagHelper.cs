using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;

namespace Bundler.Taghelpers
{
    /// <summary>
    /// A TagHelper for hooking CSS bundles up to the HTML page.
    /// </summary>
    [HtmlTargetElement("link", Attributes = "bundle")]
    public class LinkTagHelper : BaseTagHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkTagHelper"/> class.
        /// </summary>
        public LinkTagHelper(IHostingEnvironment env, IMemoryCache cache)
            : base(env, cache)
        { }


        /// <summary>
        /// The route to the bundle file name.
        /// </summary>
        [HtmlAttributeName("bundle")]
        public string Bundle { get; set; }

        /// <summary>
        /// Synchronously executes the TagHelper
        /// </summary>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!string.IsNullOrEmpty(Bundle))
            {
                if (Extensions.Pipeline.Enabled)
                {
                    IAsset asset = Extensions.Pipeline.Assets.FirstOrDefault(t => t.Route.Equals(Bundle));
                    string href = $"{Bundle}?v={GenerateHash(asset)}";
                    output.Attributes.SetAttribute("href", href);
                }
                else
                {
                    WriteIndividualTags(output);
                }
            }

            base.Process(context, output);
        }

        private void WriteIndividualTags(TagHelperOutput output)
        {
            IAsset asset = Extensions.Pipeline.Assets.FirstOrDefault(t => t.Route.Equals(Bundle));
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
