using System;
using System.IO;
using System.Net.WebSockets;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using WebSocketBridge.Client;

namespace WebSocketBridge.Server.Services
{
    public class StreamProxy : IStreamProxy
    {
        private readonly UrlEncoder _urlEncoder;
        private readonly IWebSocketToStreamConnector _webSocketToStreamConnector;

        public StreamProxy(UrlEncoder urlEncoder, IWebSocketToStreamConnector webSocketToStreamConnector)
        {
            _urlEncoder = urlEncoder ?? throw new ArgumentNullException(nameof(urlEncoder));
            _webSocketToStreamConnector = webSocketToStreamConnector ?? throw new ArgumentNullException(nameof(webSocketToStreamConnector));
        }

        public async Task<Stream> AcceptOnServer(string bridgeToken, Uri serverUri, Stream deviceStream, CancellationToken cancellationToken)
        {
            var clientWebSocket = new ClientWebSocket();    // The web-socket is disposed via the websocket stream
            var acceptUri = new Uri(serverUri, $"/bridge/accept?bridgeToken={_urlEncoder.Encode(bridgeToken)}");
            await clientWebSocket.ConnectAsync(acceptUri, cancellationToken).ConfigureAwait(false);

            await _webSocketToStreamConnector.BridgeAsync(clientWebSocket, deviceStream, cancellationToken).ConfigureAwait(false);
            return new WebSocketStream(clientWebSocket, true);
        }
    }
}