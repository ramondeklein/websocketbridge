using System.Threading;
using System.Threading.Tasks;

namespace WebSocketBridge.Server.Authentication
{
    public interface IApiKeyValidator
    {
        Task<bool> CheckApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
    }
}