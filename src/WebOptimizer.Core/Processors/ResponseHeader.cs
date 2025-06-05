using System.Collections.Generic;
using System.Threading.Tasks;
using WebOptimizer;

namespace WebOptimizer
{
    internal class ResponseHeader : Processor
    {
        private readonly string _name;
        private readonly string _value;

        public ResponseHeader(string name, string value)
        {
            _name = name;
            _value = value;
        }

        public override Task ExecuteAsync(IAssetContext config)
        {
            config.HttpContext.Response.Headers[_name] = _value;

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
        public static IEnumerable<IAsset> AddResponseHeader(this IEnumerable<IAsset> assets, string name, string value)
        {
            return assets.AddProcessor(asset => asset.AddResponseHeader(name, value));
        }
    }
}