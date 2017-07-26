using NUglify;
using NUglify.JavaScript;

namespace Bundler.Processors
{
    /// <summary>
    /// A processor that minifies JavaScript
    /// </summary>
    public class JavaScriptMinifier 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptMinifier"/> class.
        /// </summary>
        public JavaScriptMinifier(CodeSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        public CodeSettings Settings { get; set; }

        /// <summary>
        /// Executes the processor on the specified configuration.
        /// </summary>
        public void Execute(BundlerProcess config)
        {
            UglifyResult minified = Uglify.Js(config.Content, Settings);

            if (!minified.HasErrors)
            {
                config.Content = minified.Code;
            }
        }
    }
}
