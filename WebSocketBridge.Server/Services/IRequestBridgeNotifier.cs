using System.Threading;
using System.Threading.Tasks;

namespace WebSocketBridge.Server.Services
{
    public interface IRequestBridgeNotifier
    {
        Task<bool> RequestBridgeAsync(string deviceId, string acceptUrl, string? requestData, CancellationToken cancellationToken = default);
    }
}