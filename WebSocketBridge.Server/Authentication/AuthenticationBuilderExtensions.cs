using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace WebSocketBridge.Server.Authentication
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddApiKeySupport(this AuthenticationBuilder authenticationBuilder)
            => authenticationBuilder.AddApiKeySupport(options => { });

        public static AuthenticationBuilder AddApiKeySupport(this AuthenticationBuilder authenticationBuilder, Action<ApiKeyAuthenticationOptions> configureOptions)
        {
            if (authenticationBuilder is null)
                throw new ArgumentNullException(nameof(authenticationBuilder));
            authenticationBuilder.Services.AddSingleton<IApiKeyValidator, ApiKeyValidator>();
            return authenticationBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, configureOptions);
        }
    }
}