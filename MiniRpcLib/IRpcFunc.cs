using System;
using System.Threading.Tasks;
using RoR2;

namespace MiniRpcLib
{
    public interface IRpcFunc<in TRequestSend, TResponseReceive>
    {
        Task<TResponseReceive> InvokeAsync(TRequestSend parameter);
        TResponseReceive Invoke(TRequestSend parameter);
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
        Task<object> Invoke(object parameter);
    }
}