using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace WebOptimizer
{
    internal class WebOptimizerConfig : IConfigureOptions<WebOptimizerOptions>
    {
        private IConfiguration _config;
        private IOptionsMonitorCache<WebOptimizerOptions> _options;

        public WebOptimizerConfig(IConfiguration config, IOptionsMonitorCache<WebOptimizerOptions> options)
        {
            _config = config;
            _options = options;
        }
        public void Configure(WebOptimizerOptions options)
        {
            _config.GetReloadToken().RegisterChangeCallback(_ =>
            {
                _options.TryRemove(Options.DefaultName);
            }, null);

            ConfigurationBinder.Bind(_config.GetSection("WebOptimizer"), options);
        }
    }
}
