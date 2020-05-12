using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketBridge.Client
{
    public interface IWebSocketBridgeAcceptor
    {
        Task<WebSocket> AcceptAsync(string acceptUrl, CancellationToken cancellationToken = default);
    }
}