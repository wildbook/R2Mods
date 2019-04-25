using RoR2;
using System;
using UnityEngine.Networking;

namespace MiniRpcLib.RpcLayer
{
    public abstract class RpcLayer
    {
        public abstract void Initialize();
        public event Action<NetworkMessage> ReceivedC2S;
        public event Action<NetworkMessage> ReceivedS2C;

        public abstract void SendC2S(string guid, int commandId, object argument);
        public abstract void SendS2C(string guid, int commandId, object argument, NetworkUser target = null);

        protected void OnReceivedC2S(NetworkMessage x) => ReceivedC2S?.Invoke(x);
        protected void OnReceivedS2C(NetworkMessage x) => ReceivedS2C?.Invoke(x);
    }
}
