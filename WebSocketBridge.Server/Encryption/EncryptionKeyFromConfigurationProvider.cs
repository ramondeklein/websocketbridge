using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace WebSocketBridge.Server.Encryption
{
    public class EncryptionKeyFromConfigurationProvider : IEncryptionKeyProvider
    {
        private readonly IOptionsMonitor<EncryptionOptions> _options;
        
        public EncryptionKeyFromConfigurationProvider(IOptionsMonitor<EncryptionOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public ValueTask<byte[]> GetKeyAsync(CancellationToken cancellationToken)
        {
            var key = Convert.FromBase64String(_options.CurrentValue.EncryptionKey);
            return new ValueTask<byte[]>(key);
        }
    }
}