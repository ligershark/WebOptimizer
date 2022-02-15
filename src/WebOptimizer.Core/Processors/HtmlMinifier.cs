using System.Collections.Generic;
using System.Threading.Tasks;
using NUglify;
using NUglify.Html;
using WebOptimizer;

namespace WebOptimizer
{
    internal class HtmlMinifier : Processor
    {
        public HtmlMinifier(HtmlSettings settings)
        {
            Settings = settings;
        }

        public HtmlSettings Settings { get; set; }

        public override Task ExecuteAsync(IAssetContext config)
        {
            var content = new Dictionary<string, byte[]>();

            foreach (string key in config.Content.Keys)
            {
                if (key.EndsWith(".min") || key.EndsWith(".min.html"))
                {
                    content[key] = config.Content[key];
                    continue;
                }

                string input = config.Content[key].AsString();
                UglifyResult result = Uglify.Html(input, Settings);
                string minified = result.Code;

                if (result.HasErrors)
                {
                    minified = $"<!-- {string.Join("\r\n", result.Errors)} -->\r\n" + input;
                }

                content[key] = minified.AsByteArray();
            }

            config.Content = content;

            return Task.CompletedTask;
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static partial class AssetPipelineExtensions
    {
        /// <summary>
        /// Minifies any .html file requested.
        /// </summary>
        public static IEnumerable<IAsset> MinifyHtmlFiles(this IAssetPipeline pipeline)
        {
            return pipeline.MinifyHtmlFiles(new HtmlSettings());
        }

        /// <summary>
        /// Minifies any .html file requested.
        /// </summary>
        public static IEnumerable<IAsset> MinifyHtmlFiles(this IAssetPipeline pipeline, HtmlSettings settings)
        {
            return pipeline.AddFiles("text/html; charset=UTF-8", "**/*.html")
                           .MinifyHtml(settings);
        }


        /// <summary>
        /// Minifies the specified .html files.
        /// </summary>
        public static IEnumerable<IAsset> MinifyHtmlFiles(this IAssetPipeline pipeline, params string[] sourceFiles)
        {
            return pipeline.MinifyHtmlFiles(new HtmlSettings(), sourceFiles);
        }

        /// <summary>
        /// Minifies the specified .html files.
        /// </summary>
        public static IEnumerable<IAsset> MinifyHtmlFiles(this IAssetPipeline pipeline, HtmlSettings settings, params string[] sourceFiles)
        {
            return pipeline.AddFiles("text/html; charset=UTF-8", sourceFiles)
                           .MinifyHtml(settings);
        }

        /// <summary>
        /// Creates a HTML bundle on the specified route and minifies the output.
        /// </summary>
        public static IAsset AddHtmlBundle(this IAssetPipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.AddHtmlBundle(route, new HtmlSettings(), sourceFiles);
        }

        /// <summary>
        /// Creates a HTML bundle on the specified route and minifies the output.
        /// </summary>
        public static IAsset AddHtmlBundle(this IAssetPipeline pipeline, string route, HtmlSettings settings, params string[] sourceFiles)
        {
            return pipeline.AddBundle(route, "text/html; charset=UTF-8", sourceFiles)
                           .EnforceFileExtensions(".htm", ".html", ".xhtml", ".xhtm", ".shtml", ".shtm", ".js", ".nj", ".njk", ".njs", ".nunj", ".nunjs", ".nunjucks", ".smarty", ".svg", ".tpl", ".vue", ".vash", ".ejs", ".erb", ".liquid", ".lava", "..spark", ".cfm", ".kit", ".brail", ".twig", ".tag")
                           .Concatenate()
                           .MinifyHtml(settings);
        }

        /// <summary>
        /// Runs the HTML minifier on the content.
        /// </summary>
        public static IAsset MinifyHtml(this IAsset bundle)
        {
            return bundle.MinifyHtml(new HtmlSettings());
        }

        /// <summary>
        /// Runs the HTML minifier on the content.
        /// </summary>
        public static IAsset MinifyHtml(this IAsset bundle, HtmlSettings settings)
        {
            var minifier = new WebOptimizer.HtmlMinifier(settings);
            bundle.Processors.Add(minifier);

            return bundle;
        }

        /// <summary>
        /// Runs the HTML minifier on the content.
        /// </summary>
        public static IEnumerable<IAsset> MinifyHtml(this IEnumerable<IAsset> assets)
        {
            return assets.MinifyHtml(new HtmlSettings());
        }

        /// <summary>
        /// Runs the HTML minifier on the content.
        /// </summary>
        public static IEnumerable<IAsset> MinifyHtml(this IEnumerable<IAsset> assets, HtmlSettings settings)
        {
            return assets.AddProcessor(asset => asset.MinifyHtml(settings));
        }
    }
}
