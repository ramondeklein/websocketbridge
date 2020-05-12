using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketBridge.Server.Services
{
    /// <summary>
    /// This interface is the core API that is used by the bridge web-controller to
    /// setup a bridge between two streams.
    /// </summary>
    /// <remarks>
    /// The bridge is setup by the "requester" and it calls <see cref="RequestBridgeAsync"/> to
    /// indicate it wants to setup a bridge. It receives a unique token that identifies the
    /// bridge. It should then signal the device and pass it the token. It should then call
    /// <see cref="WaitForDeviceStreamAsync"/> until the device actually connections. When
    /// the device receives the bridge token, it should call <see cref="AcceptAsync"/> to
    /// indicate that it accepts the bridge.
    ///
    /// The <see cref="WaitForDeviceStreamAsync"/> is called by the requester and returns the
    /// <seealso cref="Stream"/> of the device. The <see cref="AcceptAsync"/> is called by
    /// the device and it receives the <seealso cref="Stream"/> of the requester.
    /// </remarks>
    public interface IStreamBridge
    {
        /// <summary>
        /// Request a new bridge (called by the requester).
        /// </summary>
        /// <param name="requesterStream">Stream of the requesting peer</param>
        /// <param name="cancellationToken">Cancellation token to cancel the bridge</param>
        /// <returns>Task that holds the unique bridge token</returns>
        ValueTask<string> RequestBridgeAsync(Stream requesterStream, CancellationToken cancellationToken);

        /// <summary>
        /// Wait until the device accepts the bridge and returns the device stream
        /// (called by the requester).
        /// </summary>
        /// <param name="bridgeToken">Bridge token (from <see cref="RequestBridgeAsync"/></param>
        /// <param name="cancellationToken">
        /// Cancellation token to stop waiting for the bridge to complete. This doesn't cancel
        /// the bridge, so you need to call <see cref="CancelAsync"/> to cancel the bridge itself.
        /// </param>
        /// <returns>
        /// Task that holds the device stream upon completion of the bridge.
        /// </returns>
        Task<Stream> WaitForDeviceStreamAsync(string bridgeToken, CancellationToken cancellationToken);

        /// <summary>
        /// Accepts the bridge and returns the requester stream (called by the device).
        /// </summary>
        /// <param name="bridgeToken">Bridge token (received from the requesting party)</param>
        /// <param name="deviceStream">Stream of the device peer</param>
        /// <param name="cancellationToken">Cancellation token to cancel the acceptance of the bridge.</param>
        /// <returns>
        /// Task that holds the requester stream upon completion of the bridge.
        /// </returns>
        Task<Stream> AcceptAsync(string bridgeToken, Stream deviceStream, CancellationToken cancellationToken);

        /// <summary>
        /// Cancels the bridge.
        /// </summary>
        /// <param name="bridgeToken">Bridge token</param>
        /// <returns>Task that completes when the bridge has been cancelled.</returns>
        Task CancelAsync(string bridgeToken);
    }
}