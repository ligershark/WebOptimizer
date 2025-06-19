﻿using Microsoft.AspNetCore.Http;

namespace WebOptimizer;

/// <summary>
/// The context used to perform processing to <see cref="IAsset"/> instances.
/// </summary>
public interface IAssetContext
{
    /// <summary>
    /// Gets the transform.
    /// </summary>
    IAsset Asset { get; }

    /// <summary>
    /// Gets or sets the content of the response.
    /// </summary>
    IDictionary<string, byte[]> Content { get; set; }

    /// <summary>
    /// Gets the HTTP context.
    /// </summary>
    HttpContext HttpContext { get; }

    /// <summary>
    /// Gets the global options for WebOptimizer.
    /// </summary>
    IWebOptimizerOptions Options { get; }
}
