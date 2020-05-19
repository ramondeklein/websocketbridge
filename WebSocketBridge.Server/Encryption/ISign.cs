using System.Threading;
using System.Threading.Tasks;

namespace WebSocketBridge.Server.Encryption
{
    public interface ISign
    {
        /// <summary>
        /// Create a signature for the specified object.
        /// </summary>
        /// <param name="obj">Object that needs to be signed</param>
        /// <returns>URL-safe BASE-64 signature of the object.</returns>
        Task<string> SignAsync(object obj, CancellationToken cancellationToken = default);
    }
}