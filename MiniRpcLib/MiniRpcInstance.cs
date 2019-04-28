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

        public IRpcAction<Action<NetworkWriter>> RegisterAction(Target target, Action<NetworkUser, NetworkReader> action) => MiniRpc.RegisterAction<Action<NetworkWriter>, NetworkReader>(_guid, target, action);

        public IRpcFunc<NetworkReader, Action<NetworkWriter>> RegisterFunc(Target target, Func<NetworkUser, NetworkReader, Action<NetworkWriter>> action) => MiniRpc.RegisterFunc(_guid, target, action);

        public IRpcAction<TArg> RegisterAction<TArg>(Target target, Action<NetworkUser, TArg> action)
        {
            if (!_typesBase.Contains(typeof(TArg)) && !typeof(MessageBase).IsAssignableFrom(typeof(TArg)))
                throw new NotSupportedException($"Type {typeof(TArg)} is not a valid argument type. If this is a type of yours, please implement INetworkSerializable.");

            return MiniRpc.RegisterAction(_guid, target, action);
        }

        public IRpcFunc<TArg, TReturn> RegisterFunc<TArg, TReturn>(Target target, Func<NetworkUser, TArg, TReturn> action)
        {
            if (!_typesBase.Contains(typeof(TArg)) && !typeof(MessageBase).IsAssignableFrom(typeof(TArg)))
                throw new NotSupportedException($"Type {typeof(TArg)} is not a valid argument type. If this is a type of yours, please implement INetworkSerializable.");

            if (!_typesBase.Contains(typeof(TReturn)) && !typeof(MessageBase).IsAssignableFrom(typeof(TReturn)))
                throw new NotSupportedException($"Type {typeof(TReturn)} is not a valid return value type. If this is a type of yours, please implement INetworkSerializable.");

            return MiniRpc.RegisterFunc(_guid, target, action);
        }

        private readonly HashSet<Type> _typesBase = new HashSet<Type>
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