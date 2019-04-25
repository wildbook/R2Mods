using System;
using MiniRpcLib.Action;
using MiniRpcLib.Func;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace MiniRpcLib
{
    public class MiniRpcInstance
    {
        private readonly string _guid;

        internal MiniRpcInstance(string guid) => _guid = guid;

        public IRpcAction<Action<NetworkWriter>> RegisterAction(ExecuteOn target, Action<NetworkUser, NetworkReader> action)
            => MiniRpc.RegisterAction<Action<NetworkWriter>, NetworkReader>(_guid, target, action);

        #region RegisterAction
        public IRpcAction<Color> RegisterAction(ExecuteOn target, Action<NetworkUser, Color> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Color32> RegisterAction(ExecuteOn target, Action<NetworkUser, Color32> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<GameObject> RegisterAction(ExecuteOn target, Action<NetworkUser, GameObject> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Matrix4x4> RegisterAction(ExecuteOn target, Action<NetworkUser, Matrix4x4> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<NetworkHash128> RegisterAction(ExecuteOn target, Action<NetworkUser, NetworkHash128> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<NetworkIdentity> RegisterAction(ExecuteOn target, Action<NetworkUser, NetworkIdentity> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<NetworkInstanceId> RegisterAction(ExecuteOn target, Action<NetworkUser, NetworkInstanceId> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<NetworkSceneId> RegisterAction(ExecuteOn target, Action<NetworkUser, NetworkSceneId> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Plane> RegisterAction(ExecuteOn target, Action<NetworkUser, Plane> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Quaternion> RegisterAction(ExecuteOn target, Action<NetworkUser, Quaternion> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Ray> RegisterAction(ExecuteOn target, Action<NetworkUser, Ray> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Rect> RegisterAction(ExecuteOn target, Action<NetworkUser, Rect> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Transform> RegisterAction(ExecuteOn target, Action<NetworkUser, Transform> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Vector2> RegisterAction(ExecuteOn target, Action<NetworkUser, Vector2> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Vector3> RegisterAction(ExecuteOn target, Action<NetworkUser, Vector3> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Vector4> RegisterAction(ExecuteOn target, Action<NetworkUser, Vector4> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<bool> RegisterAction(ExecuteOn target, Action<NetworkUser, bool> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<byte> RegisterAction(ExecuteOn target, Action<NetworkUser, byte> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<byte[]> RegisterAction(ExecuteOn target, Action<NetworkUser, byte[]> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<char> RegisterAction(ExecuteOn target, Action<NetworkUser, char> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<decimal> RegisterAction(ExecuteOn target, Action<NetworkUser, decimal> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<double> RegisterAction(ExecuteOn target, Action<NetworkUser, double> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<float> RegisterAction(ExecuteOn target, Action<NetworkUser, float> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<int> RegisterAction(ExecuteOn target, Action<NetworkUser, int> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<long> RegisterAction(ExecuteOn target, Action<NetworkUser, long> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<sbyte> RegisterAction(ExecuteOn target, Action<NetworkUser, sbyte> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<string> RegisterAction(ExecuteOn target, Action<NetworkUser, string> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<short> RegisterAction(ExecuteOn target, Action<NetworkUser, short> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<ushort> RegisterAction(ExecuteOn target, Action<NetworkUser, ushort> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<uint> RegisterAction(ExecuteOn target, Action<NetworkUser, uint> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<ulong> RegisterAction(ExecuteOn target, Action<NetworkUser, ulong> action) => MiniRpc.RegisterAction(_guid, target, action);
        #endregion

        public IRpcFunc<bool, string> RegisterFunc(ExecuteOn target, Func<NetworkUser, bool, string> action)
            => MiniRpc.RegisterFunc(_guid, target, action);
    }
}