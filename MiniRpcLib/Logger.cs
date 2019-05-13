using System;

namespace MiniRpcLib
{
    internal class Logger
    {
        public static Action<object> Fatal   = delegate { };
        public static Action<object> Error   = delegate { };
        public static Action<object> Warning = delegate { };
        public static Action<object> Message = delegate { };
        public static Action<object> Info    = delegate { };
        public static Action<object> Debug   = delegate { };
    }
}