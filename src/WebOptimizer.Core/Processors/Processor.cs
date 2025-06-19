using Microsoft.AspNetCore.Http;

namespace WebOptimizer;

/// <summary>
/// A base class for processors
/// </summary>
/// <seealso cref="IProcessor" />
public abstract class Processor : IProcessor
{
    /// <summary>
    /// Gets the custom key that should be used when calculating the memory cache key.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="config">The configuration.</param>
    /// <returns>System.String.</returns>
    public virtual string CacheKey(HttpContext context, IAssetContext config) => string.Empty;

    /// <summary>
    /// Executes the processor on the specified configuration.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>Task.</returns>
    public abstract Task ExecuteAsync(IAssetContext context);
}
