using System;
using System.Threading.Tasks;
using RoR2;

namespace MiniRpcLib.Func
{
    internal class RpcFunc : IRpcFunc
    {
        public string Guid { get; }
        public Type RequestSendType { get; }
        public Type RequestReceiveType { get; }
        
        public Type ResponseSendType { get; }
        public Type ResponseReceiveType { get; }
        
        public int FunctionId { get; }
        public Target ExecuteOn { get; }
        public Func<NetworkUser, object, object> Function { get; }

        public async Task<object> Invoke(object argument, NetworkUser target = null) => await MiniRpc.InvokeFunc(Guid, FunctionId, argument, target).ContinueWith(x => x.Result);

        protected RpcFunc(string guid, int commandId, Target target, Type requestSendType, Type requestReceiveType, Type responseSendType, Type responseReceiveType, Func<NetworkUser, object, object> func)
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
        public Task<TResponseReceive> InvokeAsync(TRequestSend parameter, NetworkUser target = null) => base.Invoke(parameter, target).ContinueWith(x => (TResponseReceive)x.Result);
        public TResponseReceive Invoke(TRequestSend parameter, NetworkUser target = null) => InvokeAsync(parameter, target).GetAwaiter().GetResult();

        public RpcFunc(string guid, int funcId, Target target, Func<NetworkUser, TRequestReceive, TResponseSend> func) :
            base(guid, funcId, target, 
                typeof(TRequestSend), typeof(TRequestReceive), 
                typeof(TResponseSend), typeof(TResponseReceive), 
                (x, y) => func(x, (TRequestReceive)y)) { }
    }
}