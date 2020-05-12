using Microsoft.Azure.Devices;

namespace WebSocketBridge.Server.IotHub
{
    public interface IServiceClientProvider
    {
        ServiceClient Create();
    }
}