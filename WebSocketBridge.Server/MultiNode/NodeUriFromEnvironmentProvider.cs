using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebSocketBridge.Server.MultiNode
{
    public class NodeUriFromEnvironmentProvider : INodeUriProvider
    {
        private readonly Uri _serverUri;

        public NodeUriFromEnvironmentProvider(IOptions<MultiNodeOptions> options, ILogger<NodeUriFromEnvironmentProvider> logger)
        {
            var multiNodeOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
            var localIpAddress = Environment.GetEnvironmentVariable(multiNodeOptions.LocalIpVariable) ?? "127.0.0.1";
            _serverUri = new Uri($"ws://{localIpAddress}");

            logger.LogInformation("Using local service URI: {ServerUri}", _serverUri);
        }
        
        public Uri GetCurrentNodeUri() => _serverUri;
    }
}
