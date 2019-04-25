using System;
using System.Collections.Generic;
using MiniRpcLib.Extensions;
using RoR2;
using RoR2.Networking;
using UnityEngine.Networking;

namespace MiniRpcLib.RpcLayer
{
    public class UnityMessageHandler : RpcLayer, IDisposable
    {
        private const short MessageTypeC2S = 20001;
        private const short MessageTypeS2C = 10002;
        private static readonly int ChannelId = QosChannelIndex.defaultReliable.intVal;

        private static UnityMessageHandler _instance;

        public UnityMessageHandler()
        {
            if (_instance != null) throw new Exception("Only one instance of UnityMessageHandler is allowed. Dispose the existing instance before creating a new one.");
            _instance = this;
        }

        public override void Initialize()
        {
            var serverMessageHandlers = typeof(NetworkMessageHandlerAttribute).GetFieldValue<List<NetworkMessageHandlerAttribute>>("serverMessageHandlers");
            var clientMessageHandlers = typeof(NetworkMessageHandlerAttribute).GetFieldValue<List<NetworkMessageHandlerAttribute>>("clientMessageHandlers");

            var serverRpcHandler = new NetworkMessageHandlerAttribute { server = true, msgType = MessageTypeC2S };
            var clientRpcHandler = new NetworkMessageHandlerAttribute { client = true, msgType = MessageTypeS2C };

            var delegateC2S = Delegate.CreateDelegate(typeof(NetworkMessageDelegate), MethodHelper.GetMethodInfo<Action<NetworkMessage>>(HandleC2S));
            var delegateS2C = Delegate.CreateDelegate(typeof(NetworkMessageDelegate), MethodHelper.GetMethodInfo<Action<NetworkMessage>>(HandleS2C));

            serverRpcHandler.SetFieldValue("messageHandler", delegateC2S);
            clientRpcHandler.SetFieldValue("messageHandler", delegateS2C);

            serverMessageHandlers.Add(serverRpcHandler);
            clientMessageHandlers.Add(clientRpcHandler);
        }

        public override void SendC2S(string guid, int commandId, object argument)
        {
            if (!NetworkClient.active)
                throw new UnauthorizedAccessException(
                    "You can not invoke actions on the host as you do not have a client instance.");

            var networkWriter = new NetworkWriter();
            networkWriter.StartMessage(MessageTypeC2S);
            networkWriter.Write(guid);
            networkWriter.Write(commandId);

            if (argument is Action<NetworkWriter> anw)
                anw(networkWriter);
            else
                networkWriter.WriteObject(argument);

            networkWriter.FinishMessage();

            ClientScene.readyConnection.SendWriter(networkWriter, ChannelId);
        }

        public override void SendS2C(string guid, int commandId, object argument, NetworkUser target = null)
        {
            if (!NetworkServer.active)
                throw new UnauthorizedAccessException(
                    "You can not invoke actions on all clients as you are not the host.");

            var networkWriter = new NetworkWriter();
            networkWriter.StartMessage(MessageTypeS2C);
            networkWriter.Write(guid);
            networkWriter.Write(commandId);

            if (argument is Action<NetworkWriter> nw)
                nw(networkWriter);
            else
                networkWriter.WriteObject(argument);

            networkWriter.FinishMessage();

            if (target)
            {
                target.connectionToClient.SendWriter(networkWriter, ChannelId);
            }
            else
            {
                foreach (var networkConnection in NetworkServer.connections)
                    networkConnection?.SendWriter(networkWriter, ChannelId);
            }
        }

        public void HandleS2CInternal(NetworkMessage netMsg) => OnReceivedS2C(netMsg);
        public void HandleC2SInternal(NetworkMessage netMsg) => OnReceivedC2S(netMsg);

        [NetworkMessageHandler(msgType = MessageTypeS2C, client = true)]
        private static void HandleS2C(NetworkMessage netMsg) => _instance.HandleS2CInternal(netMsg);

        [NetworkMessageHandler(msgType = MessageTypeC2S, server = true)]
        private static void HandleC2S(NetworkMessage netMsg) => _instance.HandleC2SInternal(netMsg);

        public void Dispose() => _instance = null;
    }
}
