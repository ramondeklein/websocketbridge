using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebSocketBridge.Client;
using WebSocketBridge.Server.Services;

namespace WebSocketBridge.Server.MultiNode
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMultiNode(this IServiceCollection services, IConfiguration multiNodeSettings)
        {
            services
                .Configure<MultiNodeOptions>(multiNodeSettings)
                .AddWebSocketBridgeCore()
                .AddSingleton<IEncryptionKeyProvider, EncryptionKeyFromConfigurationProvider>()
                .AddSingleton<INodeMapping, NodeMapping>()
                .AddSingleton<IStreamProxy, StreamProxy>()
                .AddSingleton<INodeUriProvider, NodeUriFromEnvironmentProvider>()
                .AddSingleton<IStreamBridge, MultiNodeStreamBridge>();
            return services;
        }
    }
}
