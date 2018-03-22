using System;
using Microsoft.AspNetCore.Hosting;

namespace WebOptimizer
{
    internal static class OptionsExtensions
    {
        /// <summary>
        /// Ensures that defaults are set
        /// </summary>
        public static void EnsureDefaults(this IWebOptimizerOptions options, IHostingEnvironment env)
        {
            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            options.EnableCaching = options.EnableCaching ?? !env.IsDevelopment();
            options.EnableDiskCache = options.EnableDiskCache ?? !env.IsDevelopment();
            options.EnableMemoryCache = options.EnableMemoryCache ?? true;
            options.EnableTagHelperBundling = options.EnableTagHelperBundling ?? true;
        }
    }
}
