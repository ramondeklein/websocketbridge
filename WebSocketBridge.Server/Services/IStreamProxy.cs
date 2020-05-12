using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketBridge.Server.Services
{
    public interface IStreamProxy
    {
        Task<Stream> AcceptOnServer(string bridgeToken, Uri serverUri, Stream deviceStream, CancellationToken cancellationToken);
    }
}