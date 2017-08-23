using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;

namespace WebOptimizer
{
    /// <summary>
    /// TagHelper for creating CDN urls.
    /// </summary>
    [HtmlTargetElement("img")]
    [HtmlTargetElement("audio")]
    [HtmlTargetElement("video")]
    [HtmlTargetElement("track")]
    [HtmlTargetElement("source")]
    [HtmlTargetElement("link")]
    [HtmlTargetElement("script")]
    public class CdnTagHelper : TagHelper
    {
        private string _cdnUrl;

        private static readonly Dictionary<string, string[]> _attributes = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "audio", new[] { "src" } },
            { "embed", new[] { "src" } },
            { "img", new[] { "src", "srcset" } },
            { "input", new[] { "src" } },
            { "link", new[] { "href" } },
            { "menuitem", new[] { "icon" } },
            { "script", new[] { "src" } },
            { "source", new[] { "src", "srcset" } },
            { "track", new[] { "src" } },
            { "video", new[] { "poster", "src" } },
        };

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
        public override int Order => int.MaxValue;

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

            if (_attributes.TryGetValue(output.TagName, out var attributeNames))
            {
                foreach (string attrName in attributeNames)
                {
                    PrependCdnUrl(output, attrName);
                }
            }

            if (output.Attributes.TryGetAttribute("cdn-prop", out var prop))
            {
                PrependCdnUrl(output, "cdn-prop");
            }
        }

        /// <summary>
        /// Prepends the CDN URL.
        /// </summary>
        protected void PrependCdnUrl(TagHelperOutput output, string attrName)
        {
            string attrValue = GetValue(attrName, output);

            // Don't modify absolute paths
            if (string.IsNullOrWhiteSpace(attrName) || string.IsNullOrWhiteSpace(attrValue) || attrValue.Contains("://") || attrValue.StartsWith("//"))
            {
                return;
            }

            string[] values = attrValue.Split(',');
            string modifiedValue = null;

            foreach (string value in values)
            {
                string fullUrl = _cdnUrl.Trim().TrimEnd('/') + "/" + value.Trim().TrimStart('~', '/');
                modifiedValue += fullUrl + ", ";
            }

            var result = new HtmlString((modifiedValue ?? attrValue).Trim(',', ' '));

            output.Attributes.SetAttribute(attrName, result);
        }

        /// <summary>
        /// Gets the value from the attribute.
        /// </summary>
        public static string GetValue(string attrName, TagHelperOutput output)
        {
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
                if (content is HtmlString htmlString)
                {
                    return htmlString.ToString();
                }

                using (var writer = new StringWriter())
                {
                    content.WriteTo(writer, HtmlEncoder.Default);
                    return writer.ToString();
                }
            }

            return null;
        }
    }
}
