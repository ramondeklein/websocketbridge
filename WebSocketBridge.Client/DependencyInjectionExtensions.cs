using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WebSocketBridge.Client
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddWebSocketBridgeCore(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .TryAddSingleton<IWebSocketToStreamConnector, WebSocketToStreamConnector>();
            return serviceCollection;
        }

        public static IServiceCollection AddWebSocketBridgeClient(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection
                .Configure<WebSocketBridgeClientOptions>(configuration)
                .AddWebSocketBridgeCore()
                .AddSingleton<IWebSocketBridgeClient, WebSocketBridgeClient>();
            return serviceCollection;
        }

        public static IServiceCollection AddWebSocketBridgeAcceptor(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddWebSocketBridgeCore()
                .AddSingleton<IWebSocketBridgeAcceptor, WebSocketBridgeAcceptor>();
            return serviceCollection;
        }
    }
}
