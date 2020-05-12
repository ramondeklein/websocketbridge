using System.Text.Json.Serialization;

namespace WebSocketBridge.Client
{
    public class BridgeRequestData
    {
        [JsonPropertyName("acceptUrl")]
        public string AcceptUrl { get; set; }

        [JsonPropertyName("requestData")]
        public string? RequestData { get; set; }
    }
}