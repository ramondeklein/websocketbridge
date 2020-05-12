using System.Threading;
using System.Threading.Tasks;

namespace WebSocketBridge.Server.MultiNode
{
    public interface IEncryptionKeyProvider
    {
        /// <summary>
        /// Get the 256-bit key.
        /// </summary>
        /// <remarks>
        /// The 256-bit encryption key is often used for cryptography. This operation
        /// is called often, so it should return the key fast. If the key is fetched
        /// from external sources (i.e. Azure KeyVault) then make sure you use some
        /// form of caching.
        /// </remarks>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>256 byte key</returns>
        ValueTask<byte[]> GetKeyAsync(CancellationToken cancellationToken);
    }
}