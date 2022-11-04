using System;
using Microsoft.Extensions.Options;

namespace WebOptimizer
{
    internal class InCodeWebOptimizerConfig : IConfigureOptions<WebOptimizerOptions>
    {
        private readonly Action<WebOptimizerOptions> _configure;

        public InCodeWebOptimizerConfig(Action<WebOptimizerOptions> configure)
        {
            _configure = configure;
        }
        
        public void Configure(WebOptimizerOptions options)
        {
            _configure(options);
        }
    }
}