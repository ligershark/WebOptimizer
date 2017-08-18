using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;

namespace WebOptimizer.Taghelpers
{
    /// <summary>
    /// TagHelper for creating CDN urls.
    /// </summary>
    [HtmlTargetElement("link")]
    [HtmlTargetElement("img")]
    [HtmlTargetElement("script")]
    [HtmlTargetElement("source")]
    [HtmlTargetElement("audio")]
    [HtmlTargetElement("video")]
    public class CdnTagHelper : TagHelper
    {
        private string _cdnUrl;
        private static string[] _attributeNames = { "src", "srcset", "href" };

        /// <summary>
        /// Initializes a new instance of the <see cref="CdnTagHelper"/> class.
        /// </summary>
        public CdnTagHelper(IConfiguration config)
        {
            _cdnUrl = config["WebOptimizer:CdnUrl"];
        }

        /// <summary>
        /// When a set of <see cref="T:Microsoft.AspNetCore.Razor.TagHelpers.ITagHelper" />s are executed, their <see cref="M:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper.Init(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext)" />'s
        /// are first invoked in the specified <see cref="P:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper.Order" />; then their
        /// <see cref="M:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper.ProcessAsync(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext,Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput)" />'s are invoked in the specified
        /// <see cref="P:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper.Order" />. Lower values are executed first.
        /// </summary>
        public override int Order => 100;

        /// <summary>
        /// Synchronously executes the <see cref="T:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper" /> with the given <paramref name="context" /> and
        /// <paramref name="output" />.
        /// </summary>
        /// <param name="context">Contains information associated with the current HTML tag.</param>
        /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(_cdnUrl) || string.IsNullOrEmpty(output.TagName))
            {
                return;
            }

            TagHelperAttribute CdnProp = context.AllAttributes.FirstOrDefault(a => a.Name == "cdn-prop") ??
                                         output.Attributes.FirstOrDefault(a => a.Name == "cdn-prop");

            string[] attributes = _attributeNames;

            if (CdnProp != null)
            {
                attributes = attributes.Union(new[] { CdnProp.Value.ToString() }).ToArray();

                if (output.Attributes.ContainsName(CdnProp.Name))
                {
                    output.Attributes.Remove(CdnProp);
                }
            }

            foreach (string name in attributes)
            {
                TagHelperAttribute attr = context.AllAttributes.FirstOrDefault(a => a.Name == name) ??
                                          output.Attributes.FirstOrDefault(a => a.Name == name);

                // Only add CDN url if attribute is present at render time
                if (attr == null || !output.Attributes.ContainsName(attr.Name))
                {
                    continue;
                }

                string attrValue = output.Attributes[attr.Name]?.Value as string ?? attr.Value.ToString();

                // Don't modify absolute paths
                if (string.IsNullOrWhiteSpace(attrValue) || attrValue.Contains("://") || attrValue.StartsWith("//"))
                {
                    continue;
                }

                string[] values = attrValue.Split(',');
                string modifiedValue = null;

                foreach (string value in values)
                {
                    string fullUrl = _cdnUrl.Trim().TrimEnd('/') + "/" + value.Trim().TrimStart('~', '/');
                    modifiedValue += fullUrl + ", ";
                }

                output.Attributes.SetAttribute(attr.Name, (modifiedValue ?? attrValue).Trim(',', ' '));
            }
        }
    }
}
