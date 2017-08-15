using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace WebOptimizer.Taghelpers
{
    /// <summary>
    /// TagHelper for creating CDN urls.
    /// </summary>
    [HtmlTargetElement("link", Attributes = "href")]
    [HtmlTargetElement("img", Attributes = "src")]
    [HtmlTargetElement("script", Attributes = "src")]
    [HtmlTargetElement("source", Attributes = "src")]
    [HtmlTargetElement("audio", Attributes = "src")]
    [HtmlTargetElement("video", Attributes = "src")]
    public class CdnTagHelper: TagHelper
    {
        private WebOptimizerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CdnTagHelper"/> class.
        /// </summary>
        public CdnTagHelper(IOptionsSnapshot<WebOptimizerOptions> options)
        {
            _options = options.Value;
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
            if (string.IsNullOrWhiteSpace(_options.CdnUrl) || string.IsNullOrEmpty(output.TagName))
            {
                return;
            }

            TagHelperAttribute attr = context.AllAttributes.FirstOrDefault(a => a.Name == "src" || a.Name == "href");

            // Only add CDN url if attribute is present at render time
            if (!output.Attributes.ContainsName(attr.Name))
            {
                return;
            }

            string value = attr?.Value?.ToString();

            // Don't modify absolute paths
            if (string.IsNullOrWhiteSpace(value) || value.Contains("://") || value.StartsWith("//"))
            {
                return;
            }

            string cdnUrl = _options.CdnUrl.Trim().TrimEnd('/') + "/" + value.Trim().TrimStart('~', '/');

            output.Attributes.SetAttribute(attr.Name, cdnUrl);
        }
    }
}
