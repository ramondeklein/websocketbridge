namespace WebSocketBridge.Server.SingleNode
{
    public interface IRandomTokenGenerator
    {
        string GenerateUrlSafeToken(int tokenSize);
    }
}