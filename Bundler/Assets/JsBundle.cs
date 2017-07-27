using System.Collections.Generic;
using NUglify.JavaScript;

namespace Bundler
{
    /// <summary>
    /// A bundle of text based files
    /// </summary>
    public class JsBundle : Bundle
    {
        /// <summary>
        /// Creates a JavaScript bundle
        /// </summary>
        public static IAsset Create(string route, IEnumerable<string> sourceFiles)
        {
            return Create(route, new CodeSettings(), sourceFiles);
        }

        /// <summary>
        /// Creates a JavaScript bundle
        /// </summary>
        public static IAsset Create(string route, CodeSettings settings, IEnumerable<string> sourceFiles)
        {
            IAsset bundle = Create(route, "application/javascript", sourceFiles);
            bundle.MinifyJavaScript(settings);

            return bundle;
        }
    }
}
