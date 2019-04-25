using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiniRpcLib.Action;
using MiniRpcLib.Extensions;
using MiniRpcLib.Func;
using RoR2;
using RoR2.Networking;
using UnityEngine.Networking;

namespace MiniRpcLib
{
    public static class MiniRpc
    {
        private const string FuncChannelGuid = "mrpc_func";

        private static readonly Dictionary<string, Dictionary<int, IRpcAction>> Actions =
            new Dictionary<string, Dictionary<int, IRpcAction>>();

        private static readonly Dictionary<string, Dictionary<int, IRpcFunc>> Functions =
            new Dictionary<string, Dictionary<int, IRpcFunc>>();

        private static readonly Dictionary<int, Action<NetworkReader>> AwaitingResponse =
            new Dictionary<int, Action<NetworkReader>>();

        private static IRpcAction<Action<NetworkWriter>> _funcRequest;
        private static IRpcAction<Action<NetworkWriter>> _funcResponse;

        private static RpcLayer.RpcLayer _layer;

        private static readonly Random Random = new Random();
        private static readonly Logger Logger = new Logger { Tag = "Koi.MiniRpc", Level = LogLevel.Info };
        private static void Log(string x) => Logger.Log(x);
        private static void LogError(string x) => Logger.LogError(x);

        public static void Initialize(RpcLayer.RpcLayer layer)
        {
            _layer = layer;
            _layer.ReceivedC2S += x => HandleRpc(ExecuteOn.Server, x);
            _layer.ReceivedS2C += x => HandleRpc(ExecuteOn.Client, x);

            On.RoR2.Networking.NetworkMessageHandlerAttribute.CollectHandlers += orig =>
            {
                orig();
                _layer.Initialize();
            };

            Reflection.InvokeMethod<NetworkMessageHandlerAttribute>("CollectHandlers");

            var instance = CreateInstance(FuncChannelGuid);

            _funcRequest  = instance.RegisterAction(ExecuteOn.Any, HandleFunctionRequest);
            _funcResponse = instance.RegisterAction(ExecuteOn.Any, HandleFunctionResponse);
        }

        private static void HandleFunctionRequest(NetworkUser nu, NetworkReader reader)
        {
            Log("HandleFunctionRequest");
            var guid     = reader.ReadString();
            var funcId   = reader.ReadInt32();
            var invokeId = reader.ReadInt32();
            Log($"Received function: {guid}[{funcId}] - {invokeId}");
            var func = Functions[guid][funcId];
            var result = func.Function.Invoke(nu,
                func.RequestReceiveType == typeof(NetworkReader)
                    ? reader
                    : reader.ReadObject(func.RequestReceiveType));

            _funcResponse.Invoke(writer =>
            {
                writer.Write(invokeId);
                writer.WriteObject(result);
            }, nu);
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
                        : netMsg.reader.ReadObject(action.ReceiveType));
            }
            catch (Exception e)
            {
                LogError($"Failed to invoke C2S command: {guid}[{commandId}] | {e}");
                throw;
            }
        }

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

        internal static async Task<object> InvokeFunc(string guid, int funcId, object argument, NetworkUser target = null)
        {
            var invokeId = Random.Next(int.MinValue, int.MaxValue);
            var t = new TaskCompletionSource<object>();

            AwaitingResponse[invokeId] = x =>
            {
                AwaitingResponse.Remove(invokeId);
                var retReceiveType = Functions[guid][funcId].ResponseReceiveType;
                Log($"AwaitingResponse[invokeId] {retReceiveType}");
                var result = typeof(NetworkReader) == retReceiveType ? x : x.ReadObject(retReceiveType);
                t.SetResult(result);
                Log($"AwaitingResponse[invokeId] {result}");
            };

            _funcRequest.Invoke(x =>
            {
                x.Write(guid);
                x.Write(funcId);
                x.Write(invokeId);
                x.WriteObject(argument);
            }, target);

            return await t.Task;
        }

        internal static void InvokeAction(string guid, int commandId, object argument, NetworkUser target = null)
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
                    if (target) throw new ArgumentException("Specifying a target is not allowed for C2S packets as they are always sent to the server.");
                    _layer.SendC2S(guid, commandId, argument);
                    break;
                case ExecuteOn.Client:
                    _layer.SendS2C(guid, commandId, argument, target);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool IsHost => NetworkServer.active;
        public static bool HasClient => ClientScene.readyConnection != null;
    }
}