using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketBridge.Server.MultiNode
{
    public interface INodeMapping
    {
        ValueTask<string> CreateNodeTokenAsync(CancellationToken cancellationToken);
        ValueTask<Uri> GetUriFromTokenAsync(string token, CancellationToken cancellationToken);
    }
}