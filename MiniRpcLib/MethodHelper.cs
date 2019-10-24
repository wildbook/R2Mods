using System;
using System.Reflection;

namespace MiniRpcLib.Extensions
{
    public static class MethodHelper
    {
        public  static MethodInfo GetMethodInfo<T>(this T a) where T : Delegate => a.Method;
    }
}
