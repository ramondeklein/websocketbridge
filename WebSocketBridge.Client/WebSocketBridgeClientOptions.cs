namespace WebSocketBridge.Client
{
    public class WebSocketBridgeClientOptions
    {
        /// <summary>
        /// The URL of the web-socket bridge server (only the basename).
        /// </summary>
        public string WebSocketBridgeUri { get; set; }

        /// <summary>
        /// The API key to allow access to the web-socket bridge.
        /// </summary>
        public string ApiKey { get; set; }
    }
}