using System;
using System.Collections.Generic;
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

        public IRpcAction<Action<NetworkWriter>> RegisterAction(Target target, Action<NetworkUser, NetworkReader> action, int? id = null)
            => MiniRpc.RegisterAction<Action<NetworkWriter>, NetworkReader>(_guid, target, action, id);

        public IRpcFunc<NetworkReader, Action<NetworkWriter>> RegisterFunc(Target target, Func<NetworkUser, NetworkReader, Action<NetworkWriter>> action, int? id = null)
            => MiniRpc.RegisterFunc(_guid, target, action, id);

        public IRpcAction<TArg> RegisterAction<TArg, TIdEnum>(Target target, Action<NetworkUser, TArg> action, TIdEnum id) where TIdEnum : Enum
            => RegisterAction(target, action, (int)(object)id);

        public IRpcFunc<TArg, TReturn> RegisterFunc<TArg, TReturn, TIdEnum>(Target target, Func<NetworkUser, TArg, TReturn> action, TIdEnum id) where TIdEnum : Enum
            => RegisterFunc<TArg, TReturn>(target, action, (int)(object)id);

        public IRpcAction<TArg> RegisterAction<TArg>(Target target, Action<NetworkUser, TArg> action, int? id = null)
        {
            if (!_typesBase.Contains(typeof(TArg)) && !typeof(MessageBase).IsAssignableFrom(typeof(TArg)))
                throw new NotSupportedException($"Type {typeof(TArg)} is not a valid argument type. If this is a type of yours, please implement INetworkSerializable.");

            return MiniRpc.RegisterAction(_guid, target, action, id);
        }

        public IRpcFunc<TArg, TReturn> RegisterFunc<TArg, TReturn>(Target target, Func<NetworkUser, TArg, TReturn> action, int? id = null)
        {
            if (!_typesBase.Contains(typeof(TArg)) && !typeof(MessageBase).IsAssignableFrom(typeof(TArg)))
                throw new NotSupportedException($"Type {typeof(TArg)} is not a valid argument type. If this is a type of yours, please implement INetworkSerializable.");

            if (!_typesBase.Contains(typeof(TReturn)) && !typeof(MessageBase).IsAssignableFrom(typeof(TReturn)))
                throw new NotSupportedException($"Type {typeof(TReturn)} is not a valid return value type. If this is a type of yours, please implement INetworkSerializable.");

            return MiniRpc.RegisterFunc(_guid, target, action, id);
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