using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketBridge.Server.Helpers;

namespace WebSocketBridge.Server.MultiNode
{
    public class NodeMapping : INodeMapping
    {
        private readonly IEncryptionKeyProvider _encryptionKeyProvider;
        private static readonly Encoding Encoding = Encoding.UTF8;

        private readonly byte[] _urlBytes;

        public NodeMapping(IEncryptionKeyProvider encryptionKeyProvider, INodeUriProvider nodeUriProvider)
        {
            _encryptionKeyProvider = encryptionKeyProvider ?? throw new ArgumentNullException(nameof(encryptionKeyProvider));
            _urlBytes = Encoding.GetBytes(nodeUriProvider.GetCurrentNodeUri().ToString());
        }

        public async ValueTask<string> CreateNodeTokenAsync(CancellationToken cancellationToken)
        {
            // An IV (initialization vector) is used to make sure that if we encode the same text over
            // and over again, the resulting value will never be the same. This is good for security,
            // because if tokens leak, then the attacker cannot determine to which server traffic is
            // routed (or if it's the same server). Another advantage of this uniqueness is that the
            // bride token is nog also unique and can be used to differentiate between different bridges
            using var aes = Aes.Create();
            aes.Key = await _encryptionKeyProvider.GetKeyAsync(cancellationToken).ConfigureAwait(false);
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var encryptedBytes = encryptor.TransformFinalBlock(_urlBytes, 0, _urlBytes.Length);

            var bytes = new byte[aes.IV.Length + encryptedBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, bytes, 0, aes.IV.Length);
            Buffer.BlockCopy(encryptedBytes, 0, bytes, aes.IV.Length, encryptedBytes.Length);

            return bytes.ToUrlSafeBase64();
        }

        public async ValueTask<Uri> GetUriFromTokenAsync(string token, CancellationToken cancellationToken)
        {
            var bytes = token.FromUrlSafeBase64();
            
            using var aes = Aes.Create();
            var iv = new byte[aes.BlockSize / 8];
            Buffer.BlockCopy(bytes, 0, iv, 0, iv.Length);
            aes.Key = await _encryptionKeyProvider.GetKeyAsync(cancellationToken).ConfigureAwait(false);
            aes.IV = iv;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var decodedBytes = decryptor.TransformFinalBlock(bytes, iv.Length, bytes.Length - iv.Length);
            return new Uri(Encoding.GetString(decodedBytes));
        }
    }
}