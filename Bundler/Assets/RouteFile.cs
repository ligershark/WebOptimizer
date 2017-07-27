using System.Collections.Generic;
using System.Linq;

namespace Bundler
{
    /// <summary>
    /// A bundle of text based files
    /// </summary>
    public class RouteFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RouteFile"/> class.
        /// </summary>
        protected RouteFile()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bundle"/> class.
        /// </summary>
        public static IEnumerable<IAsset> FromRoute(string contentType, IEnumerable<string> routes)
        {
            return FromRoute(contentType, routes.ToArray());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bundle"/> class.
        /// </summary>
        public static IEnumerable<IAsset> FromRoute(string contentType, params string[] routes)
        {
            foreach (string route in routes)
            {
                yield return Bundle.Create(route, contentType, new[] { route });
            }
        }
    }
}
