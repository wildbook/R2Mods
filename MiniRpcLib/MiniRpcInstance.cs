using System;
using System.Collections.Generic;
using IL.RoR2.Projectile;
using MiniRpcLib.Action;
using MiniRpcLib.Func;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace MiniRpcLib
{
    public class MiniRpcInstance
    {
        private readonly uint _guid;

        internal MiniRpcInstance(uint guid) => _guid = guid;

        public IRpcAction<Action<NetworkWriter>> RegisterAction(Target target, Action<NetworkUser, NetworkReader> action) => 
            MiniRpc.RegisterAction<Action<NetworkWriter>, NetworkReader>(_guid, target, action);

        public IRpcAction<T> RegisterAction<T>(Target target, Action<NetworkUser, T> action) where T : INetworkSerializable => MiniRpc.RegisterAction(_guid, target, action);

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

        public IRpcFunc<NetworkReader, Action<NetworkWriter>> RegisterFunc(Target target, Func<NetworkUser, NetworkReader, Action<NetworkWriter>> action) => MiniRpc.RegisterFunc(_guid, target, action);

        public IRpcFunc<TArg, TReturn> RegisterFunc<TArg, TReturn>(Target target, Func<NetworkUser, TArg, TReturn> action)
        {
            if (!_typesBase.Contains(typeof(TArg)) && !typeof(INetworkSerializable).IsAssignableFrom(typeof(TArg)))
                throw new NotSupportedException($"Type {typeof(TArg)} is not a valid argument type.");

            if (!_typesBase.Contains(typeof(TReturn)) && !typeof(INetworkSerializable).IsAssignableFrom(typeof(TReturn)))
                throw new NotSupportedException($"Type {typeof(TReturn)} is not a valid return value type.");

            return MiniRpc.RegisterFunc(_guid, target, action);
        }

        private readonly HashSet<Type> _typesBase = new HashSet<Type>()
        {
            typeof(Color),
            typeof(Color32),
            typeof(GameObject),
            typeof(Matrix4x4),
            typeof(NetworkHash128),
            typeof(NetworkIdentity),
            typeof(NetworkInstanceId),
            typeof(NetworkSceneId),
            typeof(Plane),
            typeof(Quaternion),
            typeof(Ray),
            typeof(Rect),
            typeof(Transform),
            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),
            typeof(bool),
            typeof(byte),
            typeof(byte[]),
            typeof(char),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(long),
            typeof(sbyte),
            typeof(string),
            typeof(short),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
        };
    }
}