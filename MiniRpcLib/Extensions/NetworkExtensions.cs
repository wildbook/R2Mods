using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MiniRpcLib.Extensions
{
    internal static class NetworkExtensions
    {
        public static void WriteObject(this NetworkWriter writer, object obj)
        {
            switch (obj)
            {
                case Color x:
                    writer.Write(x);
                    break;
                case Color32 x:
                    writer.Write(x);
                    break;
                case GameObject x:
                    writer.Write(x);
                    break;
                case Matrix4x4 x:
                    writer.Write(x);
                    break;
                case MessageBase x:
                    writer.Write(x);
                    break;
                case NetworkHash128 x:
                    writer.Write(x);
                    break;
                case NetworkIdentity x:
                    writer.Write(x);
                    break;
                case NetworkInstanceId x:
                    writer.Write(x);
                    break;
                case NetworkSceneId x:
                    writer.Write(x);
                    break;
                case Plane x:
                    writer.Write(x);
                    break;
                case Quaternion x:
                    writer.Write(x);
                    break;
                case Ray x:
                    writer.Write(x);
                    break;
                case Rect x:
                    writer.Write(x);
                    break;
                case Transform x:
                    writer.Write(x);
                    break;
                case Vector2 x:
                    writer.Write(x);
                    break;
                case Vector3 x:
                    writer.Write(x);
                    break;
                case Vector4 x:
                    writer.Write(x);
                    break;
                case bool x:
                    writer.Write(x);
                    break;
                case byte x:
                    writer.Write(x);
                    break;
                case byte[] x:
                    writer.WriteBytesFull(x);
                    break;
                case char x:
                    writer.Write(x);
                    break;
                case decimal x:
                    writer.Write(x);
                    break;
                case double x:
                    writer.Write(x);
                    break;
                case float x:
                    writer.Write(x);
                    break;
                case int x:
                    writer.Write(x);
                    break;
                case long x:
                    writer.Write(x);
                    break;
                case sbyte x:
                    writer.Write(x);
                    break;
                case short x:
                    writer.Write(x);
                    break;
                case ushort x:
                    writer.Write(x);
                    break;
                case string x:
                    writer.Write(x);
                    break;
                case uint x:
                    writer.Write(x);
                    break;
                case ulong x:
                    writer.Write(x);
                    break;
                default:
                    throw new ArgumentException(
                        $"The argument passed to WriteObject ({obj.GetType()}) is not a type supported by NetworkWriter.",
                        nameof(obj));
            }
        }

        public static object ReadObject(this NetworkReader reader, Type type)
        {
            var @switch = new Dictionary<Type, Func<object>>
            {
                {typeof(Color), () => reader.ReadColor()},
                {typeof(Color32), () => reader.ReadInt32()},
                {typeof(GameObject), reader.ReadGameObject},
                {typeof(Matrix4x4), () => reader.ReadMatrix4x4()},
                {typeof(NetworkHash128), () => reader.ReadNetworkHash128()},
                {typeof(NetworkIdentity), reader.ReadNetworkIdentity},
                {typeof(NetworkInstanceId), () => reader.ReadNetworkId()},
                {typeof(NetworkSceneId), () => reader.ReadSceneId()},
                {typeof(Plane), () => reader.ReadPlane()},
                {typeof(Quaternion), () => reader.ReadQuaternion()},
                {typeof(Ray), () => reader.ReadRay()},
                {typeof(Rect), () => reader.ReadRect()},
                {typeof(Transform), reader.ReadTransform},
                {typeof(Vector2), () => reader.ReadVector2()},
                {typeof(Vector3), () => reader.ReadVector3()},
                {typeof(Vector4), () => reader.ReadVector4()},

                {typeof(bool), () => reader.ReadBoolean()},
                {typeof(byte[]), reader.ReadBytesAndSize},
                {typeof(char), () => reader.ReadChar()},
                {typeof(decimal), () => reader.ReadDecimal()},
                {typeof(double), () => reader.ReadDouble()},
                {typeof(float), () => reader.ReadSingle()},

                {typeof(sbyte), () => reader.ReadSByte()},
                {typeof(string), reader.ReadString},

                {typeof(short), () => reader.ReadInt16()},
                {typeof(int), () => reader.ReadInt32()},
                {typeof(long), () => reader.ReadInt64()},

                {typeof(ushort), () => reader.ReadUInt16()},
                {typeof(uint), () => reader.ReadUInt32()},
                {typeof(ulong), () => reader.ReadUInt64()},
            };

            if (!@switch.ContainsKey(type))
                throw new ArgumentException(
                    $"The type ({type}) passed to ReadObject is not a type supported by NetworkReader.", nameof(type));

            return @switch[type]();
        }

    }
}
