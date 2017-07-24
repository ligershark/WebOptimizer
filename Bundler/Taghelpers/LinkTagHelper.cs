using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;

namespace Bundler.Taghelpers
{
    [HtmlTargetElement("link", Attributes = "asp-bundle")]
    public class LinkTagHelper : TagHelper
    {
        private IHostingEnvironment _env;

        public LinkTagHelper(IHostingEnvironment env)
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
                    output.Attributes.SetAttribute("href", Bundle);
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
                output.PostElement.AppendHtml($"<link href=\"{file}\" {string.Join(" ", attrs)} />");
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
