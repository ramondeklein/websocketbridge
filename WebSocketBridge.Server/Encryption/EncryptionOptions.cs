namespace WebSocketBridge.Server.Encryption
{
    public class EncryptionOptions
    {
        /// <summary>
        /// Base64 encoded 256-bit encryption key.
        /// </summary>
        /// <remarks>
        /// This encryption key is used to generate the 
        /// </remarks>
        public string EncryptionKey { get; set; }
    }
}