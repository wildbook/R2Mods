using System;
using RoR2;
using UnityEngine;

namespace MiniRpcLib
{
    internal class RpcAction : IRpcAction
    {
        protected IRpcAction AsIRpcAction => this;
        public string Guid { get; }
        public Type SendType { get; }
        public Type ReceiveType { get; }
        public int CommandId { get; set; }
        public ExecuteOn ExecuteOn { get; set; }
        public Action<NetworkUser, object> Action { get; set; }

        public void Invoke(object argument)
        {
            Debug.Log($"Sending command | {argument}");
            MiniRpc.InvokeAction(Guid, CommandId, argument);
        }

        public RpcAction(string guid, int commandId, ExecuteOn executeOn, Type sendType, Type receiveType, Action<NetworkUser, object> action)
        {
            Action      = action;
            ExecuteOn   = executeOn;
            SendType    = sendType;
            ReceiveType = receiveType;
            Guid        = guid;
            CommandId   = commandId;
        }
    }

    internal class RpcAction<TSend, TReceive> : RpcAction, IRpcAction<TSend>
    {
        public new Action<NetworkUser, TReceive> Action => (x, y) => AsIRpcAction.Action(x, y);
        public void Invoke(TSend argument) => AsIRpcAction.Invoke(argument);

        public RpcAction(string guid, int commandId, ExecuteOn executeOn, Action<NetworkUser, TReceive> action) :
            base(guid, commandId, executeOn, typeof(TSend), typeof(TReceive), (x, y) => action(x, (TReceive)y)) { }
    }
}