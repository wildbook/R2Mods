using System;
using UnityEngine;

namespace MiniRpcLib
{
    [Flags]
    public enum LogLevel
    {
        Info,
        Error,
    }
    
    public class Logger
    {
        public string Tag = "";
        public LogLevel Level = LogLevel.Info;
        private bool ShouldLog(LogLevel level) => (Level & level) != 0;
        
        public void Log(string x)
        {
            if (ShouldLog(LogLevel.Info))
                Debug.Log($"[{Tag}] {x}");
        }

        public void LogError(string x)
        {
            if (ShouldLog(LogLevel.Error))
                Debug.Log($"[{Tag}] {x}");
        }
    }
}