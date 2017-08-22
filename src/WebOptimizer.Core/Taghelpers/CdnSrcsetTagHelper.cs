using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;

namespace WebOptimizer.Taghelpers
{
    /// <summary>Handles src attributes</summary>
    [HtmlTargetElement("*", Attributes = "srcset")]
    public class CdnSrcsetTagHelper : CdnBaseTagHelper
    {
        /// <summary>Initializes a new instance of the <see cref="CdnSrcTagHelper"/> class.</summary>
        public CdnSrcsetTagHelper(IConfiguration config)
            : base(config)
        { }

        /// <summary>Gets or sets the attribute.</summary>
        public string SrcSet { get; set; }

        /// <summary>
        /// Handles the individual properties.
        /// </summary>
        protected override void HandleProperty(TagHelperOutput output)
        {
            PrependCdnUrl(output, "srcset", SrcSet);
        }
    }
}
