using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace WebOptimizer
{
    internal class WebOptimizerConfig : IConfigureOptions<WebOptimizerOptions>
    {
        private IConfiguration _config;
        private IOptionsCache<WebOptimizerOptions> _options;

        public WebOptimizerConfig(IConfiguration config, IOptionsCache<WebOptimizerOptions> options)
        {
            _config = config;
            _options = options;
        }
        public void Configure(WebOptimizerOptions options)
        {
            _config.GetReloadToken().RegisterChangeCallback(a => { _options.TryRemove(Options.DefaultName); }, null);
            ConfigurationBinder.Bind(_config.GetSection("WebOptimizer"), options);
        }
    }
}
