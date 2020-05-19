using System;
using System.Net.WebSockets;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using WebSocketBridge.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebSocketBridge.Server.Encryption;
using WebSocketBridge.Server.Services;

namespace WebSocketBridge.Server.Controllers
{
    [ApiController]
    [Route("bridge")]
    public class BridgeController : ControllerBase
    {
        private const int BufferSize = 4 * 1024;

        private readonly WebBridgeOptions _options;
        private readonly IStreamBridge _streamBridge;
        private readonly UrlEncoder _urlEncoder;
        private readonly ISign _sign;
        private readonly IRequestBridgeNotifier _requestBridgeNotifier;
        private readonly ILogger<BridgeController> _logger;

        public BridgeController(IOptions<WebBridgeOptions> options, IStreamBridge streamBridge, UrlEncoder urlEncoder, ISign sign, IRequestBridgeNotifier requestBridgeNotifier, ILogger<BridgeController> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _streamBridge = streamBridge ?? throw new ArgumentNullException(nameof(streamBridge));
            _sign = sign ?? throw new ArgumentNullException(nameof(sign));
            _urlEncoder = urlEncoder ?? throw new ArgumentNullException(nameof(urlEncoder));
            _requestBridgeNotifier = requestBridgeNotifier ?? throw new ArgumentNullException(nameof(requestBridgeNotifier));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize]
        [HttpGet("getSignedUrl")]
        public async Task<string> ConnectAsync([FromQuery] string deviceId, [FromQuery] string? requestData, [FromQuery] DateTime expirationUtc)
        {
            var signature = await _sign.SignAsync(new {deviceId, requestData, ExpirationUtc = expirationUtc.ToUniversalTime() });
            var connectSignedUrl = $"{_options.ExternalHostname}/bridge/connectSigned?deviceId={_urlEncoder.Encode(deviceId)}&requestData={_urlEncoder.Encode(requestData)}&expirationUtc={expirationUtc.ToUniversalTime():O}&signature={_urlEncoder.Encode(signature)}";
            return connectSignedUrl;
        }

        [HttpGet("connectSigned")]
        public async Task<IActionResult> ConnectAsync([FromQuery] string deviceId, [FromQuery] string? requestData, [FromQuery] DateTime expirationUtc, [FromQuery] string signature)
        {
            var expectedSignature = await _sign.SignAsync(new { deviceId, requestData, ExpirationUtc = expirationUtc.ToUniversalTime() });
            if (signature != expectedSignature)
                return BadRequest();

            if (DateTime.UtcNow > expirationUtc.ToUniversalTime())
                return BadRequest();

            return await ConnectAsync(deviceId, requestData).ConfigureAwait(false);
        }

        [Authorize]
        [HttpGet("connect")]
        public async Task<IActionResult> ConnectAsync([FromQuery] string deviceId, [FromQuery] string? requestData)
        {
            // TODO: Track the events in AppInsights

            var cancellationToken = HttpContext.RequestAborted;

            using var requesterSocket = await HttpContext.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
            string? bridgeToken = null;
            try
            {
                await using var requesterStream = new WebSocketStream(requesterSocket);

                bridgeToken = await _streamBridge.RequestBridgeAsync(requesterStream, cancellationToken).ConfigureAwait(false);
                _logger.LogDebug("Bridge to device '{DeviceId}' has bridge-token '{BridgeToken}'", deviceId, bridgeToken);

                // Send the bridge request to the device
                var acceptUrl = $"{_options.ExternalHostname}/bridge/accept?bridgeToken={_urlEncoder.Encode(bridgeToken)}";
                var accepted = await _requestBridgeNotifier.RequestBridgeAsync(deviceId, acceptUrl, requestData, cancellationToken).ConfigureAwait(false);
                if (!accepted)
                {
                    _logger.LogWarning("Bridge to device '{DeviceId}' with bridge-token '{BridgeToken}' was not accepted.", deviceId, bridgeToken);
                    return Conflict();
                }
                _logger.LogDebug("Bridge to device '{DeviceId}' with bridge-token '{BridgeToken}' is accepted by the device (waiting for bridge to establish).", deviceId, bridgeToken);

                // Wait until the device has connected to the bridge too
                await using var deviceStream = await _streamBridge.WaitForDeviceStreamAsync(bridgeToken, cancellationToken).ConfigureAwait(false);
                _logger.LogDebug("Bridge to device '{DeviceId}' with bridge-token '{BridgeToken}' has been established.", deviceId, bridgeToken);

                // Copy all data from the requester to the device (the other side of the bridge copies data the other way)
                await requesterStream.CopyToAsync(deviceStream, BufferSize, cancellationToken).ConfigureAwait(false);

                // Close our socket
                await requesterSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "ok", cancellationToken).ConfigureAwait(false);
                _logger.LogDebug("Bridge to device '{DeviceId}' with bridge-token '{BridgeToken}' is closed.", deviceId, bridgeToken);

                return Ok();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Bridge to device '{DeviceId}' with bridge-token '{BridgeToken}' encountered an exception and will be terminated.", deviceId, bridgeToken);
                if (requesterSocket != null)
                    await requesterSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "exception", cancellationToken).ConfigureAwait(false);
                throw;
            }
            finally
            {
                await _streamBridge.CancelAsync(bridgeToken).ConfigureAwait(false);
            }
        }

        [HttpGet("accept")]
        public async Task AcceptAsync([FromQuery] string bridgeToken)
        {
            var cancellationToken = HttpContext.RequestAborted;

            var deviceSocket = await HttpContext.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
            try
            {
                await using var deviceStream = new WebSocketStream(deviceSocket);
                _logger.LogDebug("Bridge token '{BridgeToken}' received from device.", bridgeToken);

                await using var requesterStream = await _streamBridge.AcceptAsync(bridgeToken, deviceStream, cancellationToken).ConfigureAwait(false);
                _logger.LogDebug("Bridge token has been accepted by the device.", bridgeToken);

                // Copy all data from the device to the requester (the other side of the bridge copies data the other way)
                await deviceStream.CopyToAsync(requesterStream, BufferSize, cancellationToken).ConfigureAwait(false);

                await deviceSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "ok", cancellationToken).ConfigureAwait(false);
                _logger.LogDebug("Bridge with bridge-token '{BridgeToken}' is closed.", bridgeToken);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Bridge with bridge-token '{BridgeToken}' encountered an exception and will be terminated.", bridgeToken);
                if (deviceSocket != null)
                    await deviceSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "exception", cancellationToken).ConfigureAwait(false);
                throw;
            }
            finally
            {
                await _streamBridge.CancelAsync(bridgeToken).ConfigureAwait(false);
            }
        }
    }
}
