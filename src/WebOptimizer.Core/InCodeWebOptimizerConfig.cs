using Microsoft.Extensions.Options;

namespace WebOptimizer;

internal class InCodeWebOptimizerConfig(Action<WebOptimizerOptions> configure) : IConfigureOptions<WebOptimizerOptions>
{
    public void Configure(WebOptimizerOptions options)
    {
        configure(options);
    }
}
