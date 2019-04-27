using System;
using System.Linq;
using System.Threading.Tasks;
using RoR2;

namespace MiniRpcLib.Func
{
    internal class RpcFunc : IRpcFunc
    {
        public uint Guid { get; }
        public Type RequestSendType { get; }
        public Type RequestReceiveType { get; }
        
        public Type ResponseSendType { get; }
        public Type ResponseReceiveType { get; }
        
        public int FunctionId { get; }
        public Target ExecuteOn { get; }
        public Func<NetworkUser, object, object> Function { get; }

        public async Task Invoke(object argument, Action<object>[] callbacks, NetworkUser target = null) => 
            await MiniRpc.InvokeFunc(Guid, FunctionId, argument, callbacks, target);

        protected RpcFunc(uint guid, int commandId, Target target, Type requestSendType, Type requestReceiveType, Type responseSendType, Type responseReceiveType, Func<NetworkUser, object, object> func)
        {
            Guid = guid;
            FunctionId = commandId;
            ExecuteOn = target;
            Function = func;
            RequestSendType = requestSendType;
            RequestReceiveType = requestReceiveType;
            
            ResponseSendType = responseSendType;
            ResponseReceiveType = responseReceiveType;
        }
    }

    internal class RpcFunc<TRequestSend, TRequestReceive, TResponseSend, TResponseReceive> : RpcFunc, IRpcFunc<TRequestSend, TResponseReceive>
    {
        public Task InvokeAsync(TRequestSend parameter, Action<TResponseReceive>[] callbacks, NetworkUser target = null) =>
            Invoke(parameter, callbacks.Select(x => new Action<object>(y => x((TResponseReceive)y))).ToArray(), target);

        public void Invoke(TRequestSend parameter, Action<TResponseReceive>[] callbacks, NetworkUser target = null) => 
            InvokeAsync(parameter, callbacks, target);

        public Task InvokeAsync(TRequestSend parameter, Action<TResponseReceive> callback = null, NetworkUser target = null) =>
            InvokeAsync(parameter, new [] { callback }, target);

        public void Invoke(TRequestSend parameter, Action<TResponseReceive> callback = null, NetworkUser target = null) =>
            Invoke(parameter, new[] { callback }, target);

        public RpcFunc(uint guid, int funcId, Target target, Func<NetworkUser, TRequestReceive, TResponseSend> func) :
            base(guid, funcId, target, 
                typeof(TRequestSend), typeof(TRequestReceive), 
                typeof(TResponseSend), typeof(TResponseReceive), 
                (x, y) => func(x, (TRequestReceive)y)) { }
    }
}