using System;
using UnityEngine;

namespace MiniRpcLib
{
    [Flags]
    public enum LogLevel
    {
        Info = 1,
        Error = 2,
    }
    
    public class Logger
    {
        public string Tag = "";
        public LogLevel Level = LogLevel.Info | LogLevel.Error;
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