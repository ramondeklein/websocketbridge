using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketBridge.Client
{
    /// <summary>
    /// Service that allows bridging all data between a websocket and a .NET stream.
    /// </summary>
    /// <remarks>
    /// This service is typically used by tunnels after the websocket has been connected
    /// and a network-stream has been obtained from the TCP client. This connector then
    /// takes care of sending all the data between the both peers.
    /// </remarks>
    public interface IWebSocketToStreamConnector
    {
        Task BridgeAsync(WebSocket webSocket, Stream stream, CancellationToken cancellationToken = default);
    }
}