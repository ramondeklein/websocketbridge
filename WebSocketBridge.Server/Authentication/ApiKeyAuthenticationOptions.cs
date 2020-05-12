using Microsoft.AspNetCore.Authentication;

namespace WebSocketBridge.Server.Authentication
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "API Key";
        
        public string Scheme => DefaultScheme;
        public string AuthenticationType = DefaultScheme;
    }
}