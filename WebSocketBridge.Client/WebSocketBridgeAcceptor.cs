using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketBridge.Client
{
    internal class WebSocketBridgeAcceptor : IWebSocketBridgeAcceptor
    {
        public async Task<WebSocket> AcceptAsync(string acceptUrl, CancellationToken cancellationToken = default)
        {
            var clientWebSocket = new ClientWebSocket();
            try
            {
                await clientWebSocket.ConnectAsync(new Uri(acceptUrl), cancellationToken).ConfigureAwait(false);
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