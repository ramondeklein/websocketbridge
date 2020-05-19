using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebSocketBridge.Server.Helpers;

namespace WebSocketBridge.Server.Encryption
{
    internal class Sign : ISign
    {
        private readonly IEncryptionKeyProvider _encryptionKeyProvider;

        public Sign(IEncryptionKeyProvider encryptionKeyProvider)
        {
            _encryptionKeyProvider = encryptionKeyProvider;
        }

        public async Task<string> SignAsync(object obj, CancellationToken cancellationToken = default)
        {
            var key = await _encryptionKeyProvider.GetKeyAsync(cancellationToken).ConfigureAwait(false);
            using var hmac = new HMACSHA256(key);

            var json = JsonSerializer.Serialize(obj, obj.GetType());
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            var hash = hmac.ComputeHash(jsonBytes);
            return hash.ToUrlSafeBase64();
        }
    }
}