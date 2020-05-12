using System;
using System.Net.WebSockets;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebSocketBridge.Client
{
    internal class WebSocketBridgeClient : IWebSocketBridgeClient
    {
        private readonly IOptionsMonitor<WebSocketBridgeClientOptions> _options;
        private readonly UrlEncoder _urlEncoder;
        private readonly ILogger<WebSocketBridgeClient> _logger;

        public WebSocketBridgeClient(IOptionsMonitor<WebSocketBridgeClientOptions> options, UrlEncoder urlEncoder, ILogger<WebSocketBridgeClient> logger)
        {
            _options = options;
            _urlEncoder = urlEncoder;
            _logger = logger;
        }

        public async Task<WebSocket> TunnelAsync(string deviceId, string requestData, CancellationToken cancellationToken = default)
        {
            var options = _options.CurrentValue;

            var encodedDeviceId = _urlEncoder.Encode(deviceId);
            var encodedRequestData = _urlEncoder.Encode(requestData);
            var url = new Uri($"{options.WebSocketBridgeUri}/bridge/connect?deviceId={encodedDeviceId}&requestData={encodedRequestData}");

            var clientWebSocket = new ClientWebSocket();
            try
            {
                if (!string.IsNullOrEmpty(options.ApiKey))
                    clientWebSocket.Options.SetRequestHeader("X-Api-Key", options.ApiKey);

                _logger.LogTrace("Requesting bridge from URL {Url}", url);
                await clientWebSocket.ConnectAsync(url, cancellationToken).ConfigureAwait(false);
                _logger.LogDebug("Bridge established to device {DeviceId}", deviceId);

                return clientWebSocket;
            }
            catch
            {
                clientWebSocket.Dispose();
                throw;
            }
        }
    }
}