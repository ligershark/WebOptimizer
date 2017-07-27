using System.Collections.Generic;
using System.Linq;
using NUglify.Css;

namespace Bundler
{
    /// <summary>
    /// A bundle of text based files
    /// </summary>
    public class CssFile : RouteFile
    {
        /// <summary>
        /// Creates an array of CSS asset files.
        /// </summary>
        public static IEnumerable<IAsset> FromRoute(params string[] routes)
        {
            return FromRoute(new CssSettings(), routes);
        }

        /// <summary>
        /// Creates an array of CSS asset files.
        /// </summary>
        public static IEnumerable<IAsset> FromRoute(IEnumerable<string> routes)
        {
            return FromRoute(new CssSettings(), routes.ToArray());
        }

        /// <summary>
        /// Creates a CSS bundle
        /// </summary>
        public static IEnumerable<IAsset> FromRoute(CssSettings settings, params string[] routes)
        {
            IEnumerable<IAsset> assets = FromRoute("application/javascript", routes);

            return assets.MinifyCss(settings);
        }
    }
}
