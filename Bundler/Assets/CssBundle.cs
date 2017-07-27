using System.Collections.Generic;
using NUglify.Css;

namespace Bundler
{
    /// <summary>
    /// A bundle of text based files
    /// </summary>
    public class CssBundle : Bundle
    {
        /// <summary>
        /// Creates a JavaScript bundle
        /// </summary>
        public static IAsset Create(string route, IEnumerable<string> sourceFiles)
        {
            return Create(route, new CssSettings(), sourceFiles);
        }

        /// <summary>
        /// Creates a JavaScript bundle
        /// </summary>
        public static IAsset Create(string route, CssSettings settings, IEnumerable<string> sourceFiles)
        {
            IAsset bundle = Create(route, "text/css", sourceFiles);
            bundle.MinifyCss(settings);

            return bundle;
        }
    }
}
