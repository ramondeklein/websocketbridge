using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace WebSocketBridge.Server.Authentication
{
    public class ApiKeyValidator : IApiKeyValidator
    {
        private readonly IOptionsMonitor<WebBridgeOptions> _options;

        public ApiKeyValidator(IOptionsMonitor<WebBridgeOptions> options)
        {
            _options = options;
        }

        public Task<bool> CheckApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_options.CurrentValue?.ApiKeys.Contains(apiKey) ?? false);
        }
    }
}