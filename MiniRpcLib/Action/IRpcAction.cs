using System;
using RoR2;

namespace MiniRpcLib.Action
{
    public interface IRpcAction
    {
        uint Guid { get; }
        Type SendType { get; }
        Type ReceiveType { get; }
        int CommandId { get; }
        Target ExecuteOn { get; }
        Action<NetworkUser, object> Action { get; }
        void Invoke(object parameter, NetworkUser target = null);
    }

    public interface IRpcAction<in TSend>
    {
        uint Guid { get; }
        Type SendType { get; }
        Type ReceiveType { get; }
        int CommandId { get; }
        Target ExecuteOn { get; }
        Action<NetworkUser, object> Action { get; }
        void Invoke(TSend parameter, NetworkUser target = null);
    }
}