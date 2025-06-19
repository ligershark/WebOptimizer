using WebOptimizer;

namespace WebOptimizer
{
    internal class ResponseHeader(string name, string value) : Processor
    {
        public override Task ExecuteAsync(IAssetContext config)
        {
            config.HttpContext.Response.Headers[name] = value;

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
        /// Adds a fingerprint to local url() references.
        /// NOTE: Make sure to call Concatenate() before this method
        /// </summary>
        /// <param name="bundle">The bundle.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>IAsset.</returns>
        public static IAsset AddResponseHeader(this IAsset bundle, string name, string value)
        {
            var minifier = new ResponseHeader(name, value);
            bundle.Processors.Add(minifier);

            return bundle;
        }

        /// <summary>
        /// Adds a fingerprint to local url() references.
        /// NOTE: Make sure to call Concatenate() before this method
        /// </summary>
        /// <param name="assets">The assets.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>IEnumerable&lt;IAsset&gt;.</returns>
        public static IEnumerable<IAsset> AddResponseHeader(this IEnumerable<IAsset> assets, string name, string value)
        {
            return assets.AddProcessor(asset => asset.AddResponseHeader(name, value));
        }
    }
}