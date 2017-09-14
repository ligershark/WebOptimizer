using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebOptimizer
{
    /// <summary>
    /// A class for building a asset.
    /// </summary>
    public interface IAssetBuilder
    {
        /// <summary>
        /// Builds an asset by running it through all the processors.
        /// </summary>
        Task<IAssetResponse> BuildAsync(IAsset asset, HttpContext context, IWebOptimizerOptions options);
    }
}