using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace WebOptimizer
{
    internal class WebOptimizerConfig : IConfigureOptions<Options>
    {
        private IConfiguration _config;

        public WebOptimizerConfig(IConfiguration config)
        {
            _config = config;
        }
        public void Configure(Options options)
        {
            ConfigurationBinder.Bind(_config.GetSection("WebOptimizer"), options);
        }
    }
}
