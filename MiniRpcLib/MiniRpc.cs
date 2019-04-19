using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RoR2;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

namespace MiniRpcLib
{
    public static class MiniRpc
    {
        private const short MessageTypeC2S = 20001;
        private const short MessageTypeS2C = 10002;
        private const string FuncChannelGuid = "mrpc_func";

        private static readonly int ChannelId = QosChannelIndex.defaultReliable.intVal;

        private static readonly Dictionary<string, Dictionary<int, IRpcAction>> Actions =
            new Dictionary<string, Dictionary<int, IRpcAction>>();

        private static readonly Dictionary<string, Dictionary<int, IRpcFunc>> Functions =
            new Dictionary<string, Dictionary<int, IRpcFunc>>();

        private static readonly Dictionary<int, Action<NetworkReader>> AwaitingResponse =
            new Dictionary<int, Action<NetworkReader>>();

        private static IRpcAction<Action<NetworkWriter>> _funcRequest;
        private static IRpcAction<Action<NetworkWriter>> _funcResponse;
        private static readonly Random Random = new Random();

        private static readonly Logger Logger = new Logger {Tag = "Koi.MiniRpc", Level = LogLevel.Info};
        private static void Log(string x) => Logger.Log(x);
        private static void LogError(string x) => Logger.LogError(x);

        public static void Initialize()
        {
            On.RoR2.Networking.NetworkMessageHandlerAttribute.CollectHandlers += orig =>
            {
                orig();
                RegisterNetworkMessage();
            };

            Reflection.InvokeMethod<NetworkMessageHandlerAttribute>("CollectHandlers");

            var instance = CreateInstance(FuncChannelGuid);

            _funcRequest = instance.RegisterAction(ExecuteOn.Any, HandleFunctionRequest);
            _funcResponse = instance.RegisterAction(ExecuteOn.Any, HandleFunctionResponse);
        }

        private static void HandleFunctionRequest(NetworkUser nu, NetworkReader reader)
        {
            Log("HandleFunctionRequest");
            var guid = reader.ReadString();
            var funcId = reader.ReadInt32();
            var invokeId = reader.ReadInt32();
            Log($"Received function: {guid}[{funcId}] - {invokeId}");
            var func = Functions[guid][funcId];
            var result = func.Function.Invoke(nu,
                func.RequestReceiveType == typeof(NetworkReader)
                    ? reader
                    : ReadObject(reader, func.RequestReceiveType));

            _funcResponse.Invoke(writer =>
            {
                writer.Write(invokeId);
                WriteObject(writer, result);
            });
        }

        private static void HandleFunctionResponse(NetworkUser nu, NetworkReader reader)
        {
            Log("HandleFunctionResponse");
            var invokeId = reader.ReadInt32();
            Log($"Received function return: {invokeId}");
            AwaitingResponse[invokeId].Invoke(reader);
        }

        public static MiniRpcInstance CreateInstance(string guid)
        {
            Actions[guid] = new Dictionary<int, IRpcAction>();
            Functions[guid] = new Dictionary<int, IRpcFunc>();
            return new MiniRpcInstance(guid);
        }

        private static void RegisterNetworkMessage()
        {
            Log("Registering network handlers.");

            var serverMessageHandlers = typeof(NetworkMessageHandlerAttribute).GetFieldValue<List<NetworkMessageHandlerAttribute>>("serverMessageHandlers");
            var clientMessageHandlers = typeof(NetworkMessageHandlerAttribute).GetFieldValue<List<NetworkMessageHandlerAttribute>>("clientMessageHandlers");

            var serverRpcHandler = new NetworkMessageHandlerAttribute {server = true, msgType = MessageTypeC2S};
            var clientRpcHandler = new NetworkMessageHandlerAttribute {client = true, msgType = MessageTypeS2C};

            var delegateC2S = Delegate.CreateDelegate(typeof(NetworkMessageDelegate), GetMethodInfo<Action<NetworkMessage>>(HandleC2S));
            var delegateS2C = Delegate.CreateDelegate(typeof(NetworkMessageDelegate), GetMethodInfo<Action<NetworkMessage>>(HandleS2C));
            
            serverRpcHandler.SetFieldValue("messageHandler", delegateC2S);
            clientRpcHandler.SetFieldValue("messageHandler", delegateS2C);

            serverMessageHandlers.Add(serverRpcHandler);
            clientMessageHandlers.Add(clientRpcHandler);

            Log("Registered network handlers.");
        }

        [NetworkMessageHandler(msgType = MessageTypeS2C, client = true)]
        private static void HandleS2C(NetworkMessage netMsg) => HandleRpc(ExecuteOn.Client, netMsg);

        [NetworkMessageHandler(msgType = MessageTypeC2S, server = true)]
        private static void HandleC2S(NetworkMessage netMsg) => HandleRpc(ExecuteOn.Server, netMsg);

        private static void HandleRpc(ExecuteOn commandType, NetworkMessage netMsg)
        {
            Log($"{commandType} Received command.");

            var guid = netMsg.reader.ReadString();
            var commandId = netMsg.reader.ReadInt32();

            if (!Actions.TryGetValue(guid, out var actions) || !actions.TryGetValue(commandId, out var action))
            {
                LogError($"{commandType} Received unregistered CommandId: {guid}[{commandId}]");
                throw new Exception($"{commandType} Received unregistered CommandId: {guid}[{commandId}]");
            }

            Log($"{commandType} Received command: {guid}[{commandId}]");

            if (action.ExecuteOn != ExecuteOn.Any && action.ExecuteOn != commandType)
            {
                var err = $"Can not invoke {commandType} command as {action.ExecuteOn}.";
                LogError(err);
                throw new Exception(err);
            }

            try
            {
                var nu = NetworkUser.readOnlyInstancesList.FirstOrDefault(v => v.connectionToClient == netMsg.conn);

                action.Action.Invoke(nu,
                    action.ReceiveType == typeof(NetworkReader)
                        ? netMsg.reader
                        : ReadObject(netMsg.reader, action.ReceiveType));
            }
            catch (Exception e)
            {
                LogError($"Failed to invoke C2S command: {guid}[{commandId}] | {e}");
                throw;
            }
        }

        private static void SendC2S(string guid, int commandId, object argument)
        {
            if (!HasClient)
                throw new UnauthorizedAccessException(
                    "You can not invoke actions on the host as you do not have a client instance.");

            var networkWriter = new NetworkWriter();
            networkWriter.StartMessage(MessageTypeC2S);
            networkWriter.Write(guid);
            networkWriter.Write(commandId);

            if (argument is Action<NetworkWriter> anw)
                anw(networkWriter);
            else
                WriteObject(networkWriter, argument);

            networkWriter.FinishMessage();

            ClientScene.readyConnection.SendWriter(networkWriter, ChannelId);
        }

        private static void SendS2C(string guid, int commandId, object argument)
        {
            if (!IsHost)
                throw new UnauthorizedAccessException(
                    "You can not invoke actions on all clients as you are not the host.");

            var networkWriter = new NetworkWriter();
            networkWriter.StartMessage(MessageTypeS2C);
            networkWriter.Write(guid);
            networkWriter.Write(commandId);

            if (argument is Action<NetworkWriter> anw)
                anw(networkWriter);
            else
                WriteObject(networkWriter, argument);

            networkWriter.FinishMessage();

            foreach (var networkConnection in NetworkServer.connections)
                networkConnection?.SendWriter(networkWriter, ChannelId);
        }

        private static void WriteObject(NetworkWriter writer, object obj)
        {
            switch (obj)
            {
                case Color x:
                    writer.Write(x);
                    break;
                case Color32 x:
                    writer.Write(x);
                    break;
                case GameObject x:
                    writer.Write(x);
                    break;
                case Matrix4x4 x:
                    writer.Write(x);
                    break;
                case MessageBase x:
                    writer.Write(x);
                    break;
                case NetworkHash128 x:
                    writer.Write(x);
                    break;
                case NetworkIdentity x:
                    writer.Write(x);
                    break;
                case NetworkInstanceId x:
                    writer.Write(x);
                    break;
                case NetworkSceneId x:
                    writer.Write(x);
                    break;
                case Plane x:
                    writer.Write(x);
                    break;
                case Quaternion x:
                    writer.Write(x);
                    break;
                case Ray x:
                    writer.Write(x);
                    break;
                case Rect x:
                    writer.Write(x);
                    break;
                case Transform x:
                    writer.Write(x);
                    break;
                case Vector2 x:
                    writer.Write(x);
                    break;
                case Vector3 x:
                    writer.Write(x);
                    break;
                case Vector4 x:
                    writer.Write(x);
                    break;
                case bool x:
                    writer.Write(x);
                    break;
                case byte x:
                    writer.Write(x);
                    break;
                case byte[] x:
                    writer.WriteBytesFull(x);
                    break;
                case char x:
                    writer.Write(x);
                    break;
                case decimal x:
                    writer.Write(x);
                    break;
                case double x:
                    writer.Write(x);
                    break;
                case float x:
                    writer.Write(x);
                    break;
                case int x:
                    writer.Write(x);
                    break;
                case long x:
                    writer.Write(x);
                    break;
                case sbyte x:
                    writer.Write(x);
                    break;
                case short x:
                    writer.Write(x);
                    break;
                case ushort x:
                    writer.Write(x);
                    break;
                case string x:
                    writer.Write(x);
                    break;
                case uint x:
                    writer.Write(x);
                    break;
                case ulong x:
                    writer.Write(x);
                    break;
                default:
                    throw new ArgumentException(
                        $"The argument passed to WriteObject ({obj.GetType()}) is not a type supported by NetworkWriter.",
                        nameof(obj));
            }
        }

        private static object ReadObject(NetworkReader reader, Type type)
        {
            var @switch = new Dictionary<Type, Func<object>>
            {
                {typeof(Color), () => reader.ReadColor()},
                {typeof(Color32), () => reader.ReadInt32()},
                {typeof(GameObject), reader.ReadGameObject},
                {typeof(Matrix4x4), () => reader.ReadMatrix4x4()},
                {typeof(NetworkHash128), () => reader.ReadNetworkHash128()},
                {typeof(NetworkIdentity), reader.ReadNetworkIdentity},
                {typeof(NetworkInstanceId), () => reader.ReadNetworkId()},
                {typeof(NetworkSceneId), () => reader.ReadSceneId()},
                {typeof(Plane), () => reader.ReadPlane()},
                {typeof(Quaternion), () => reader.ReadQuaternion()},
                {typeof(Ray), () => reader.ReadRay()},
                {typeof(Rect), () => reader.ReadRect()},
                {typeof(Transform), reader.ReadTransform},
                {typeof(Vector2), () => reader.ReadVector2()},
                {typeof(Vector3), () => reader.ReadVector3()},
                {typeof(Vector4), () => reader.ReadVector4()},

                {typeof(bool), () => reader.ReadBoolean()},
                {typeof(byte[]), reader.ReadBytesAndSize},
                {typeof(char), () => reader.ReadChar()},
                {typeof(decimal), () => reader.ReadDecimal()},
                {typeof(double), () => reader.ReadDouble()},
                {typeof(float), () => reader.ReadSingle()},

                {typeof(sbyte), () => reader.ReadSByte()},
                {typeof(string), reader.ReadString},

                {typeof(short), () => reader.ReadInt16()},
                {typeof(int), () => reader.ReadInt32()},
                {typeof(long), () => reader.ReadInt64()},

                {typeof(ushort), () => reader.ReadUInt16()},
                {typeof(uint), () => reader.ReadUInt32()},
                {typeof(ulong), () => reader.ReadUInt64()},
            };

            if (!@switch.ContainsKey(type))
                throw new ArgumentException(
                    $"The type ({type}) passed to ReadObject is not a type supported by NetworkReader.", nameof(type));

            return @switch[type]();
        }

        private static MethodInfo GetMethodInfo<T>(T a) where T : Delegate => a.Method;

        internal static IRpcAction<T> RegisterAction<T>(string guid, ExecuteOn target, Action<NetworkUser, T> action)
            => RegisterAction<T, T>(guid, target, action);

        internal static IRpcAction<TSend> RegisterAction<TSend, TReceive>(string guid, ExecuteOn target,
            Action<NetworkUser, TReceive> action)
        {
            var actions = Actions[guid];
            var id = actions.Count;

            Log($"{guid}[{id}] Registering action | {target}");
            var rpcAction = new RpcAction<TSend, TReceive>(guid, actions.Count, target, action);

            actions.Add(id, rpcAction);
            return rpcAction;
        }

        internal static IRpcFunc<TRequest, TResponse> RegisterFunc<TRequest, TResponse>
            (string guid, ExecuteOn target, Func<NetworkUser, TRequest, TResponse> func)
            => RegisterFunc<TRequest, TRequest, TResponse, TResponse>(guid, target, func);

        internal static IRpcFunc<TRequestSend, TResponseReceive> RegisterFunc<TRequestSend, TRequestReceive,
                TResponseSend, TResponseReceive>
            (string guid, ExecuteOn target, Func<NetworkUser, TRequestReceive, TResponseSend> func)
        {
            var functions = Functions[guid];
            var id = functions.Count;

            Log($"{guid}[{id}] Registering action | {target}");
            var rpcFunc =
                new RpcFunc<TRequestSend, TRequestReceive, TResponseSend, TResponseReceive>(guid, id, target, func);

            functions.Add(id, rpcFunc);

            return rpcFunc;
        }

        internal static async Task<object> InvokeFunc(string guid, int funcId, object argument)
        {
            var invokeId = Random.Next(int.MinValue, int.MaxValue);
            var t = new TaskCompletionSource<object>();

            AwaitingResponse[invokeId] = x =>
            {
                AwaitingResponse.Remove(invokeId);
                var retReceiveType = Functions[guid][funcId].ResponseReceiveType;
                Log($"AwaitingResponse[invokeId] {retReceiveType}");
                var result = typeof(NetworkReader) == retReceiveType ? x : ReadObject(x, retReceiveType);
                t.SetResult(result);
                Log($"AwaitingResponse[invokeId] {result}");
            };

            _funcRequest.Invoke(x =>
            {
                x.Write(guid);
                x.Write(funcId);
                x.Write(invokeId);
                WriteObject(x, argument);
            });

            return await t.Task;
        }

        internal static void InvokeAction(string guid, int commandId, object argument)
        {
            Log($"{guid}[{commandId}] Sending command | {argument}");
            var rpc = Actions[guid][commandId];

            if (rpc.SendType != argument.GetType())
                throw new ArgumentException(
                    $"The passed argument type ({argument.GetType()})is not the correct type ({rpc.SendType}).",
                    nameof(argument));

            switch (rpc.ExecuteOn)
            {
                case ExecuteOn.Any:
                    if (IsHost)
                        goto case ExecuteOn.Server;
                    else
                        goto case ExecuteOn.Client;
                case ExecuteOn.Server:
                    SendC2S(guid, commandId, argument);
                    break;
                case ExecuteOn.Client:
                    SendS2C(guid, commandId, argument);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool IsHost => NetworkServer.active;
        public static bool HasClient => ClientScene.readyConnection != null;
    }
}