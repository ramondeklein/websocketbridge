using Microsoft.Azure.Devices;
using Microsoft.Extensions.Options;

namespace WebSocketBridge.Server.IotHub
{
    public class ServiceClientProvider : IServiceClientProvider
    {
        private readonly IOptionsMonitor<IotSettings> _settings;

        public ServiceClientProvider(IOptionsMonitor<IotSettings> settings)
        {
            _settings = settings;
        }
        
        public ServiceClient Create()
        {
            var settings = _settings.CurrentValue;
            var serviceClient = ServiceClient.CreateFromConnectionString(settings.ConnectionString, TransportType.Amqp);
            return serviceClient;
        }
    }
}