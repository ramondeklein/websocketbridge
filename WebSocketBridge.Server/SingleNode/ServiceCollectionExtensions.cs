using Microsoft.Extensions.DependencyInjection;
using WebSocketBridge.Server.Services;

namespace WebSocketBridge.Server.SingleNode
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSingleNode(this IServiceCollection services)
        {
            services
                .AddSingleton<IRandomTokenGenerator, RandomTokenGenerator>()
                .AddSingleton<IStreamBridge, SingleNodeStreamBridge>();
            return services;
        }
    }
}