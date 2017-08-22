using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;

namespace WebOptimizer.Taghelpers
{
    /// <summary>Handles src attributes</summary>
    [HtmlTargetElement("*", Attributes = "href")]
    public class CdnHrefTagHelper : CdnBaseTagHelper
    {
        /// <summary>Initializes a new instance of the <see cref="CdnHrefTagHelper"/> class.</summary>
        public CdnHrefTagHelper(IConfiguration config)
            : base(config)
        { }

        /// <summary>Gets or sets the attribute.</summary>
        public string Href { get; set; }

        /// <summary>
        /// Handles the individual properties.
        /// </summary>
        protected override void HandleProperty(TagHelperOutput output)
        {
            PrependCdnUrl(output, "href", Href);
        }
    }
}
