using Microsoft.AspNetCore.Http;

namespace WebOptimizer;

/// <summary>
/// Represents the context used to perform processing on <see cref="IAsset"/> instances. Implements
/// the <see cref="IAssetContext"/>
/// </summary>
/// <param name="httpContext">The HTTP context.</param>
/// <param name="asset">The asset.</param>
/// <param name="options">The options.</param>
/// <seealso cref="IAssetContext"/>
internal class AssetContext(HttpContext httpContext, IAsset asset, IWebOptimizerOptions options) : IAssetContext
{
    /// <summary>
    /// Gets the transform.
    /// </summary>
    /// <value>The asset.</value>
    public IAsset Asset { get; } = asset ?? throw new ArgumentNullException(nameof(asset));

    /// <summary>
    /// Gets or sets the content of the response.
    /// </summary>
    /// <value>The content.</value>
    public IDictionary<string, byte[]> Content { get; set; } = new Dictionary<string, byte[]>();

    /// <summary>
    /// Gets the HTTP context.
    /// </summary>
    /// <value>The HTTP context.</value>
    public HttpContext HttpContext { get; } = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

    /// <summary>
    /// Gets the global options for WebOptimizer.
    /// </summary>
    /// <value>The options.</value>
    public IWebOptimizerOptions Options { get; } = options ?? throw new ArgumentNullException(nameof(options));
}
