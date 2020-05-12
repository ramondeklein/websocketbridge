using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebSocketBridge.Server.Services;
using WebSocketBridge.Server.SingleNode;

namespace WebSocketBridge.Server.MultiNode
{
    /// <summary>
    /// Multi-node stream bridge that can be used when running inside a cluster.
    /// </summary>
    /// <remarks>
    /// This implementation is almost as efficient as the single-node stream bridge,
    /// when used with a single node. Only token generation is a bit more expensive.
    /// 
    /// The multi-node stream bridge generates unique tokens that hold the node
    /// URI. The actual node cannot be determined by looking at the encoded token,
    /// but is only visible when decoded. If the token cannot be found on the local
    /// node, then the token is decoded and the stream is forwarded to another
    /// node. This adds an additional hop and additional streaming, so it's less
    /// efficient. When using a cluster behind a load-balancer, where the individual
    /// nodes cannot be directly addresses from the outside, then this is the only
    /// option.
    /// </remarks>
    public class MultiNodeStreamBridge : IStreamBridge
    {
        private class BridgeInfo
        {
            private readonly TaskCompletionSource<Stream> _tcs = new TaskCompletionSource<Stream>();
            private readonly Stream _requesterStream;

            public BridgeInfo(Stream requesterStream)
            {
                _requesterStream = requesterStream;
            }

            public Stream BridgeAccepted(Stream deviceStream)
            {
                _tcs.SetResult(deviceStream);
                return _requesterStream;
            }

            public async Task<Stream> WaitForDeviceStreamAsync(CancellationToken cancellationToken)
            {
                await using (cancellationToken.Register(() => _tcs.TrySetCanceled()))
                {
                    var deviceStream = await _tcs.Task.ConfigureAwait(false);
                    return deviceStream;
                }
            }

            public void Cancel()
            {
                _tcs.TrySetCanceled();
            }
        }

        private readonly INodeMapping _serverMapping;
        private readonly INodeUriProvider _nodeUriProvider;
        private readonly IStreamProxy _streamProxy;
        private readonly ConcurrentDictionary<string, BridgeInfo> _bridges = new ConcurrentDictionary<string, BridgeInfo>();

        public MultiNodeStreamBridge(INodeMapping serverMapping, INodeUriProvider nodeUriProvider, IStreamProxy streamProxy)
        {
            _serverMapping = serverMapping;
            _nodeUriProvider = nodeUriProvider;
            _streamProxy = streamProxy;
        }

        public async ValueTask<string> RequestBridgeAsync(Stream requesterStream, CancellationToken cancellationToken)
        {
            var bridgeToken = await _serverMapping.CreateNodeTokenAsync(cancellationToken).ConfigureAwait(false);
            var tcsBridge = new BridgeInfo(requesterStream);
            if (!_bridges.TryAdd(bridgeToken, tcsBridge))
                throw new InvalidOperationException("Duplicate bridge token");

            return bridgeToken;
        }

        public Task<Stream> WaitForDeviceStreamAsync(string bridgeToken, CancellationToken cancellationToken)
        {
            var bridgeInfo = GetBridgeInfo(bridgeToken);
            return bridgeInfo.WaitForDeviceStreamAsync(cancellationToken);
        }

        public async Task<Stream> AcceptAsync(string bridgeToken, Stream deviceStream, CancellationToken cancellationToken)
        {
            if (!_bridges.TryGetValue(bridgeToken, out var bridgeInfo))
            {
                Uri serverUri;
                try
                {
                    serverUri = await _serverMapping.GetUriFromTokenAsync(bridgeToken, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    throw new ArgumentException("Invalid bridge token", nameof(bridgeToken));
                }

                if (serverUri == _nodeUriProvider.GetCurrentNodeUri())
                    throw new ArgumentException("Invalid bridge token", nameof(bridgeToken));

                // Forward the accept to the other server
                return await _streamProxy.AcceptOnServer(bridgeToken, serverUri, deviceStream, cancellationToken).ConfigureAwait(false);
            }

            var requesterStream = bridgeInfo.BridgeAccepted(deviceStream);
            return requesterStream;
        }

        public Task CancelAsync(string bridgeToken)
        {
            if (_bridges.TryRemove(bridgeToken, out var bridgeInfo))
                bridgeInfo.Cancel();
            return Task.CompletedTask;
        }

        private BridgeInfo GetBridgeInfo(string bridgeToken)
        {
            if (!_bridges.TryGetValue(bridgeToken, out var bridgeInfo))
                throw new ArgumentException("Invalid bridge token", nameof(bridgeToken));
            return bridgeInfo;
        }
    }
}