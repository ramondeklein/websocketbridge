using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketBridge.Client
{
    public interface IWebSocketBridgeClient
    {
        Task<WebSocket> TunnelAsync(string deviceId, string requestData, CancellationToken cancellationToken = default);
    }
}
