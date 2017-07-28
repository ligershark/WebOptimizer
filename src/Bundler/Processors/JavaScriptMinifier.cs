using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NUglify;
using NUglify.JavaScript;

namespace Bundler.Processors
{
    /// <summary>
    /// A processor that minifies JavaScript
    /// </summary>
    public class JavaScriptMinifier : IProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptMinifier"/> class.
        /// </summary>
        public JavaScriptMinifier(CodeSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// Gets the custom key that should be used when calculating the memory cache key.
        /// </summary>
        public string CacheKey(HttpContext context) => string.Empty;

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        public CodeSettings Settings { get; set; }

        /// <summary>
        /// Executes the processor on the specified configuration.
        /// </summary>
        public Task ExecuteAsync(IAssetContext config)
        {
            return Task.Run(() =>
            {
                UglifyResult minified = Uglify.Js(config.Content, Settings);

                if (!minified.HasErrors)
                {
                    config.Content = minified.Code;
                }
            });
        }
    }
}
