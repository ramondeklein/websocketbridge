using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using WebSocketBridge.Client;
using WebSocketBridge.Server.Services;
using WebSocketBridge.Server.SingleNode;

namespace WebSocketBridge.Server.IotHub
{
    public class IotHubRequestBridgeNotifier : IRequestBridgeNotifier
    {
        private const string BridgeMethodName = "Bridge";
        private readonly IServiceClientProvider _serviceClientProvider;

        public IotHubRequestBridgeNotifier(IServiceClientProvider serviceClientProvider)
        {
            _serviceClientProvider = serviceClientProvider ?? throw new ArgumentNullException(nameof(serviceClientProvider));
        }

        public async Task<bool> RequestBridgeAsync(string deviceId, string acceptUrl, string? requestData, CancellationToken cancellationToken = default)
        {
            if (deviceId is null)
                throw new ArgumentNullException(nameof(deviceId));

            // Request a new bridge
            var bridgeRequest = new BridgeRequestData
            {
                AcceptUrl = acceptUrl,
                RequestData = requestData
            };

            using var serviceClient = _serviceClientProvider.Create();
            var requestJson = JsonSerializer.Serialize(bridgeRequest);
            var ctdMethod = new CloudToDeviceMethod(BridgeMethodName);
            ctdMethod.SetPayloadJson(requestJson);
            var ctdResult = await serviceClient.InvokeDeviceMethodAsync(deviceId, ctdMethod, cancellationToken).ConfigureAwait(false);
            return ctdResult.Status == 0;
        }
    }
}