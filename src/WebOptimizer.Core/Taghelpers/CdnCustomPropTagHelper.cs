using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;

namespace WebOptimizer.Taghelpers
{

    /// <summary>Handles src attributes</summary>
    [HtmlTargetElement("*", Attributes = "cdn-prop")]
    public class CdnCustomPropTagHelper : CdnBaseTagHelper
    {
        /// <summary>Initializes a new instance of the <see cref="CdnSrcTagHelper"/> class.</summary>
        public CdnCustomPropTagHelper(IConfiguration config)
            : base(config)
        { }

        /// <summary>Gets or sets the attribute.</summary>
        public string CdnProp { get; set; }

        /// <summary>
        /// Handles the individual properties.
        /// </summary>
        protected override void HandleProperty(TagHelperOutput output)
        {
            if (!string.IsNullOrEmpty(CdnProp) && output.Attributes.TryGetAttribute(CdnProp, out var prop))
            {
                PrependCdnUrl(output, CdnProp, prop.Value.ToString());
            }
        }
    }
}
