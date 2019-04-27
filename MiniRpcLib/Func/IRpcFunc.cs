using System;
using System.Threading.Tasks;
using RoR2;

namespace MiniRpcLib.Func
{
    public interface IRpcFunc<in TRequestSend, out TResponseReceive>
    {
        Task InvokeAsync(TRequestSend parameter, Action<TResponseReceive>[] callbacks = null, NetworkUser target = null);
        Task InvokeAsync(TRequestSend parameter, Action<TResponseReceive> callback = null, NetworkUser target = null);

        void Invoke(TRequestSend parameter, Action<TResponseReceive>[] callbacks = null, NetworkUser target = null);
        void Invoke(TRequestSend parameter, Action<TResponseReceive> callback = null, NetworkUser target = null);
    }

    public interface IRpcFunc
    {
        uint Guid { get; }
        Type RequestSendType { get; }
        Type RequestReceiveType { get; }
        
        Type ResponseSendType { get; }
        Type ResponseReceiveType { get; }
        int FunctionId { get; }
        Target ExecuteOn { get; }
        Func<NetworkUser, object, object> Function { get; }
        Task Invoke(object parameter, Action<object>[] callbacks = null, NetworkUser user = null);
    }
}