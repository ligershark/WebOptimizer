using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;

namespace Bundler.Taghelpers
{
    [HtmlTargetElement("script", Attributes = "asp-bundle")]
    public class ScriptTagHelper : TagHelper
    {
        private IHostingEnvironment _env;

        public ScriptTagHelper(IHostingEnvironment env)
        {
            _env = env;
        }

        [HtmlAttributeName("asp-bundle")]
        public string Bundle { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!string.IsNullOrEmpty(Bundle))
            {
                if (Exensions.Options.Enabled)
                {
                    output.Attributes.SetAttribute("src", Bundle);
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
            var transform = Exensions.Options.Transforms.FirstOrDefault(t => t.Path.Equals(Bundle));
            output.SuppressOutput();

            var attrs = new List<string>();

            foreach (var item in output.Attributes)
            {
                string attr = item.Name;

                if (item.ValueStyle != HtmlAttributeValueStyle.Minimized)
                {
                    var quote = GetQuote(item.ValueStyle);
                    attr += "=" + quote + item.Value + quote;
                }

                attrs.Add(attr);
            }

            foreach (string file in transform.SourceFiles)
            {
                output.PostElement.AppendHtml($"<script src=\"{file}\" {string.Join(" ", attrs)}></script>");
            }
        }

        private static string GetQuote(HtmlAttributeValueStyle style)
        {
            switch (style)
            {
                case HtmlAttributeValueStyle.DoubleQuotes:
                    return "\"";
                case HtmlAttributeValueStyle.SingleQuotes:
                    return "'";
            }

            return string.Empty;
        }
    }
}
