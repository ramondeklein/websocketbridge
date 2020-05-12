using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WebSocketBridge.Client
{
    /// <summary>
    /// Default implementation of the <see cref="IWebSocketToStreamConnector"/> interface.
    /// </summary>
    internal class WebSocketToStreamConnector : IWebSocketToStreamConnector
    {
        private readonly ILogger<WebSocketToStreamConnector> _logger;
        private const int BufferSize = 4096;

        public WebSocketToStreamConnector(ILogger<WebSocketToStreamConnector> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task BridgeAsync(WebSocket webSocket, Stream stream, CancellationToken cancellationToken = default)
        {
            if (webSocket is null)
                throw new ArgumentNullException(nameof(webSocket));
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            try
            {
                await Task.WhenAny(
                    CopyWebSocketToStreamAsync(webSocket, stream, cancellationToken),
                    CopyStreamToWebSocketAsync(stream, webSocket, cancellationToken)).ConfigureAwait(false);

                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "ok", cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "exception", cancellationToken).ConfigureAwait(false);
                throw;
            }
            finally
            {
                await stream.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task CopyWebSocketToStreamAsync(WebSocket webSocket, Stream stream, CancellationToken cancellationToken)
        {
            var receiveBuffer = new byte[BufferSize];
            while (webSocket.State == WebSocketState.Open)
            {
                var receiveResult = await webSocket.ReceiveAsync(receiveBuffer, cancellationToken).ConfigureAwait(false);
                if (_logger.IsEnabled(LogLevel.Trace))
                    _logger.LogTrace("Read {Count} bytes from the websocket", receiveResult.Count);

                await stream.WriteAsync(receiveBuffer, 0, receiveResult.Count, cancellationToken).ConfigureAwait(false);
                if (_logger.IsEnabled(LogLevel.Trace))
                    _logger.LogTrace("Wrote {Count} bytes to the stream", receiveResult.Count);
            }
        }

        private async Task CopyStreamToWebSocketAsync(Stream stream, WebSocket webSocket, CancellationToken cancellationToken)
        {
            var buffer = new byte[BufferSize];
            while (true)
            {
                var readBytes = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                if (_logger.IsEnabled(LogLevel.Trace))
                    _logger.LogTrace("Read {Count} bytes from the stream", readBytes);
                if (readBytes == 0)
                    return;

                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, readBytes), WebSocketMessageType.Binary, true, cancellationToken).ConfigureAwait(false);
                if (_logger.IsEnabled(LogLevel.Trace))
                    _logger.LogTrace("Wrote {Count} bytes to the websocket", readBytes);
            }
        }
    }
}
