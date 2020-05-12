using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebSocketBridge.Server.Services;

namespace WebSocketBridge.Server.SingleNode
{
    /// <summary>
    /// Lightweight stream bridge that can be used when running on a single node.
    /// </summary>
    /// <remarks>
    /// This implementation works very efficient when only a single node is used to
    /// hold all the bridges. It's often net suitable when it's scaled-out, because
    /// it assumes that the requester and device always connect to the same node.
    /// 
    /// This could work in a multi-node setup, where each node is reachable from
    /// the internet on its own unique endpoint. This requires that all nodes should
    /// have unique IP addresses and/or ports. Using a port range is often not desirable,
    /// because this complicates firewall setups for the devices. It's best to keep
    /// traffic on the standard ports, so each server should have its own external IP
    /// address and should notify the device that it should connect to that specific
    /// node.
    /// </remarks>
    internal class SingleNodeStreamBridge : IStreamBridge
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

        private const int TokenSize = 20;
        private readonly ConcurrentDictionary<string, BridgeInfo> _bridges = new ConcurrentDictionary<string, BridgeInfo>();
        private readonly IRandomTokenGenerator _randomTokenGenerator;
        private readonly ILogger<SingleNodeStreamBridge> _logger;

        public SingleNodeStreamBridge(IRandomTokenGenerator randomTokenGenerator, ILogger<SingleNodeStreamBridge> logger)
        {
            _randomTokenGenerator = randomTokenGenerator ?? throw new ArgumentNullException(nameof(randomTokenGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ValueTask<string> RequestBridgeAsync(Stream requesterStream, CancellationToken cancellationToken)
        {
            var bridgeToken = _randomTokenGenerator.GenerateUrlSafeToken(TokenSize);
            var tcsBridge = new BridgeInfo(requesterStream);
            if (!_bridges.TryAdd(bridgeToken, tcsBridge))
                throw new InvalidOperationException("Duplicate bridge token");

            return new ValueTask<string>(bridgeToken);
        }

        public Task<Stream> WaitForDeviceStreamAsync(string bridgeToken, CancellationToken cancellationToken)
        {
            var bridgeInfo = GetBridgeInfo(bridgeToken);
            return bridgeInfo.WaitForDeviceStreamAsync(cancellationToken);
        }

        public Task<Stream> AcceptAsync(string bridgeToken, Stream deviceStream, CancellationToken cancellationToken)
        {
            var bridgeInfo = GetBridgeInfo(bridgeToken);
            var requesterStream = bridgeInfo.BridgeAccepted(deviceStream);
            return Task.FromResult(requesterStream);
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