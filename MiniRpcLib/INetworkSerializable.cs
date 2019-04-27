using UnityEngine.Networking;

namespace MiniRpcLib
{
    /// <summary>
    /// Allow your custom object to be sent over MiniRPC.
    /// 
    /// Note:
    ///     Your constructor will ONLY be called if it does NOT take any arguments.
    ///     If your object does not implement a default constructor, the object will be initialized without calling the constructor.
    /// </summary>
    public interface INetworkSerializable
    {
        void Serialize(NetworkWriter writer);
        void Deserialize(NetworkReader reader);
    }
}
