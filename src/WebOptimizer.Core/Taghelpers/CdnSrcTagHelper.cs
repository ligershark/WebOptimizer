using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;

namespace WebOptimizer.Taghelpers
{
    /// <summary>Handles src attributes</summary>
    [HtmlTargetElement("*", Attributes = "src")]
    public class CdnSrcTagHelper : CdnBaseTagHelper
    {
        /// <summary>Initializes a new instance of the <see cref="CdnSrcTagHelper"/> class.</summary>
        public CdnSrcTagHelper(IConfiguration config)
            : base(config)
        { }

        /// <summary>Gets or sets the attribute.</summary>
        public string Src { get; set; }

        /// <summary>
        /// Handles the individual properties.
        /// </summary>
        protected override void HandleProperty(TagHelperOutput output)
        {
            PrependCdnUrl(output, "src", Src);
        }
    }
}
