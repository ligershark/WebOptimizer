using Microsoft.AspNetCore.Http;

namespace Bundler.Processors
{
    /// <summary>
    /// An interface for post processors.
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// Executes the processor on the specified configuration.
        /// </summary>
        void Execute(AssetContext context);

        /// <summary>
        /// Gets the custom key that should be used when calculating the memory cache key.
        /// </summary>
        string CacheKey(HttpContext context);
    }
}