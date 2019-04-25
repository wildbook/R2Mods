using System;
using RoR2;

namespace MiniRpcLib.Action
{
    public interface IRpcAction
    {
        string Guid { get; }
        Type SendType { get; }
        Type ReceiveType { get; }
        int CommandId { get; }
        ExecuteOn ExecuteOn { get; }
        Action<NetworkUser, object> Action { get; }
        void Invoke(object parameter, NetworkUser target = null);
    }

    public interface IRpcAction<in TSend>
    {
        string Guid { get; }
        Type SendType { get; }
        Type ReceiveType { get; }
        int CommandId { get; }
        ExecuteOn ExecuteOn { get; }
        Action<NetworkUser, object> Action { get; }
        void Invoke(TSend parameter, NetworkUser target = null);
    }
}