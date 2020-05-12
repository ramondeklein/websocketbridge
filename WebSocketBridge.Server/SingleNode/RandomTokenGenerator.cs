using System;
using System.Security.Cryptography;
using WebSocketBridge.Server.Helpers;

namespace WebSocketBridge.Server.SingleNode
{
    public class RandomTokenGenerator : IRandomTokenGenerator, IDisposable
    {
        private readonly RNGCryptoServiceProvider _cryptoServiceProvider = new RNGCryptoServiceProvider();

        public void Dispose()
        {
            _cryptoServiceProvider.Dispose();
            GC.SuppressFinalize(this);
        } 

        public string GenerateUrlSafeToken(int tokenSize)
        {
            if (tokenSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(tokenSize));

            var bytes = new byte[tokenSize];
            _cryptoServiceProvider.GetBytes(bytes);
            return bytes.ToUrlSafeBase64();
        }
    }
}
