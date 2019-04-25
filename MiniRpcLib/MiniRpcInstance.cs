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

        public IRpcAction<Action<NetworkWriter>> RegisterAction(Target target, Action<NetworkUser, NetworkReader> action)
            => MiniRpc.RegisterAction<Action<NetworkWriter>, NetworkReader>(_guid, target, action);

        #region RegisterAction
        public IRpcAction<Color> RegisterAction(Target target, Action<NetworkUser, Color> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Color32> RegisterAction(Target target, Action<NetworkUser, Color32> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<GameObject> RegisterAction(Target target, Action<NetworkUser, GameObject> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Matrix4x4> RegisterAction(Target target, Action<NetworkUser, Matrix4x4> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<NetworkHash128> RegisterAction(Target target, Action<NetworkUser, NetworkHash128> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<NetworkIdentity> RegisterAction(Target target, Action<NetworkUser, NetworkIdentity> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<NetworkInstanceId> RegisterAction(Target target, Action<NetworkUser, NetworkInstanceId> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<NetworkSceneId> RegisterAction(Target target, Action<NetworkUser, NetworkSceneId> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Plane> RegisterAction(Target target, Action<NetworkUser, Plane> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Quaternion> RegisterAction(Target target, Action<NetworkUser, Quaternion> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Ray> RegisterAction(Target target, Action<NetworkUser, Ray> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Rect> RegisterAction(Target target, Action<NetworkUser, Rect> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Transform> RegisterAction(Target target, Action<NetworkUser, Transform> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Vector2> RegisterAction(Target target, Action<NetworkUser, Vector2> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Vector3> RegisterAction(Target target, Action<NetworkUser, Vector3> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<Vector4> RegisterAction(Target target, Action<NetworkUser, Vector4> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<bool> RegisterAction(Target target, Action<NetworkUser, bool> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<byte> RegisterAction(Target target, Action<NetworkUser, byte> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<byte[]> RegisterAction(Target target, Action<NetworkUser, byte[]> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<char> RegisterAction(Target target, Action<NetworkUser, char> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<decimal> RegisterAction(Target target, Action<NetworkUser, decimal> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<double> RegisterAction(Target target, Action<NetworkUser, double> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<float> RegisterAction(Target target, Action<NetworkUser, float> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<int> RegisterAction(Target target, Action<NetworkUser, int> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<long> RegisterAction(Target target, Action<NetworkUser, long> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<sbyte> RegisterAction(Target target, Action<NetworkUser, sbyte> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<string> RegisterAction(Target target, Action<NetworkUser, string> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<short> RegisterAction(Target target, Action<NetworkUser, short> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<ushort> RegisterAction(Target target, Action<NetworkUser, ushort> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<uint> RegisterAction(Target target, Action<NetworkUser, uint> action) => MiniRpc.RegisterAction(_guid, target, action);
        public IRpcAction<ulong> RegisterAction(Target target, Action<NetworkUser, ulong> action) => MiniRpc.RegisterAction(_guid, target, action);
        #endregion

        public IRpcFunc<bool, string> RegisterFunc(Target target, Func<NetworkUser, bool, string> action)
            => MiniRpc.RegisterFunc(_guid, target, action);
    }
}