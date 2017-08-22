using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;

namespace WebOptimizer.Taghelpers
{
    /// <summary>
    /// TagHelper for creating CDN urls.
    /// </summary>
    public abstract class CdnBaseTagHelper : TagHelper
    {
        private string _cdnUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="CdnBaseTagHelper"/> class.
        /// </summary>
        public CdnBaseTagHelper(IConfiguration config)
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

            HandleProperty(output);
        }

        /// <summary>
        /// Handles the individual properties.
        /// </summary>
        protected abstract void HandleProperty(TagHelperOutput output);

        /// <summary>
        /// Prepends the CDN URL.
        /// </summary>
        protected void PrependCdnUrl(TagHelperOutput output, string attrName, string attrValue)
        {
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
    }
}
