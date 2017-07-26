using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;

namespace Bundler.Taghelpers
{
    /// <summary>
    /// A TagHelper for hooking JavaScript bundles up to the HTML page.
    /// </summary>
    [HtmlTargetElement("script", Attributes = "bundle")]
    public class ScriptTagHelper : BaseTagHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptTagHelper"/> class.
        /// </summary>
        public ScriptTagHelper(IHostingEnvironment env, IMemoryCache cache)
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
                if (Extensions.Options.Enabled)
                {
                    IBundle bundle = Extensions.Options.Bundles.FirstOrDefault(t => t.Route.Equals(Bundle));
                    string href = $"{Bundle}?v={GenerateHash(bundle)}";
                    output.Attributes.SetAttribute("src", href);
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
            IBundle bundle = Extensions.Options.Bundles.FirstOrDefault(t => t.Route.Equals(Bundle));
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

            foreach (string file in bundle.SourceFiles)
            {
                string src = AddFileVersionToPath(file);
                output.PostElement.AppendHtml($"<script src=\"{src}\" {string.Join(" ", attrs)}></script>");
            }
        }
    }
}
