using System;
using System.Threading.Tasks;
using RoR2;

namespace MiniRpcLib.Func
{
    public interface IRpcFunc<in TRequestSend, TResponseReceive>
    {
        Task<TResponseReceive> InvokeAsync(TRequestSend parameter, NetworkUser target = null);

        TResponseReceive Invoke(TRequestSend parameter, NetworkUser target = null);
    }

    public interface IRpcFunc
    {
        string Guid { get; }
        Type RequestSendType { get; }
        Type RequestReceiveType { get; }
        
        Type ResponseSendType { get; }
        Type ResponseReceiveType { get; }
        int FunctionId { get; }
        ExecuteOn ExecuteOn { get; }
        Func<NetworkUser, object, object> Function { get; }
        Task<object> Invoke(object parameter, NetworkUser user = null);
    }
}