using System.Collections.Generic;
using System.Linq;
using NUglify.JavaScript;

namespace Bundler
{
    /// <summary>
    /// A bundle of text based files
    /// </summary>
    public class JsFile : RouteFile
    {
        /// <summary>
        /// Creates an array of JavaScript asset files.
        /// </summary>
        public static IEnumerable<IAsset> FromRoute(params string[] routes)
        {
            return FromRoute(new CodeSettings(), routes);
        }

        /// <summary>
        /// Creates an array of JavaScript asset files.
        /// </summary>
        public static IEnumerable<IAsset> FromRoute(IEnumerable<string> routes)
        {
            return FromRoute(new CodeSettings(), routes.ToArray());
        }

        /// <summary>
        /// Creates a JavaScript bundle
        /// </summary>
        public static IEnumerable<IAsset> FromRoute(CodeSettings settings, params string[] routes)
        {
            IEnumerable<IAsset> assets = FromRoute("application/javascript", routes);

            return assets.MinifyJavaScript(settings);
        }
    }
}
