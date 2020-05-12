using System;

namespace WebSocketBridge.Server
{
    public class WebBridgeOptions
    {
        /// <summary>
        /// Specifies the external name of the bridge server.
        /// </summary>
        /// <remarks>
        /// This is the hostname that is used to send to the devices, so they can accept
        /// the bridge. This server should be externally available.
        ///
        /// When the multi-server stream bridge is used, then this is often the external
        /// hostname of the load balancer.
        ///
        /// When the single-server stream bridge is used, then this is the external name
        /// of the server that generates the bridge token. If multiple servers are used,
        /// then each server should have its own unique address.
        ///
        /// This is external traffic, so it should always be secured (wss scheme).
        /// </remarks>
        /// <example>
        /// wss://stream-bridge.example.com
        /// </example>
        public string? ExternalHostname { get; set; }

        /// <summary>
        /// API keys are used to determine which services can request bridges. One of the API keys
        /// must match to allow the /bridge/connect websocket request.
        /// </summary>
        public string[] ApiKeys { get; set; } = Array.Empty<string>();
    }
}
