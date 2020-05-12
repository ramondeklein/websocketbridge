using System;

namespace WebSocketBridge.Server.MultiNode
{
    public interface INodeUriProvider
    {
        Uri GetCurrentNodeUri();
    }
}