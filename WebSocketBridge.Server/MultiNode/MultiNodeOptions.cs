namespace WebSocketBridge.Server.MultiNode
{
    public class MultiNodeOptions
    {
        /// <summary>
        /// Specifies the environment that holds the local IP address of the bridge server
        /// (only used in multi-server stream bridge).
        /// </summary>
        /// <remarks>
        /// This is the hostname that is used between two bridge servers for internal
        /// communication. Each server should have its own hostname.
        ///
        /// If the connection between the servers is trusted, then this communication
        /// doesn't need to be secured.
        /// </remarks>
        /// <example>
        /// LOCAL_IP   (where LOCAL_IP contains the IP address, such as 10.1.2.3)
        /// </example>
        public string LocalIpVariable { get; set; } = "LOCAL_IP";

        /// <summary>
        /// Base64 encoded 256-bit encryption key.
        /// </summary>
        /// <remarks>
        /// This encryption key is used to generate the 
        /// </remarks>
        public string EncryptionKey { get; set; }
    }
}