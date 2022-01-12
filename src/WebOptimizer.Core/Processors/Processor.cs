using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace WebOptimizer
{
    /// <summary>
    /// A base class for processors
    /// </summary>
    /// <seealso cref="WebOptimizer.IProcessor" />
    public abstract class Processor : IProcessor
    {
        /// <summary>
        /// Gets the custom key that should be used when calculating the memory cache key.
        /// </summary>
        public virtual string CacheKey(HttpContext context, IAssetContext config) => string.Empty;

        /// <summary>
        /// Executes the processor on the specified configuration.
        /// </summary>
        public abstract Task ExecuteAsync(IAssetContext context);
    }
}
