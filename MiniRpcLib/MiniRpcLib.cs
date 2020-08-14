using BepInEx;
using MiniRpcLib.RpcLayer;

namespace MiniRpcLib
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class MiniRpcPlugin : BaseUnityPlugin
    {
        public const string Dependency = ModGuid;

        private const string ModVer = "1.0";
        private const string ModName = "MiniRpcLib";
        private const string ModGuid = "dev.wildbook.lib-minirpc";

        public MiniRpcPlugin()
        {
            Logger.LogInfo("Initializing Logger");
            InitLogger();

            Logger.LogInfo("Initializing MiniRpc");
            MiniRpc.Initialize(new UnityMessageHandler());
        }

        public void InitLogger()
        {
            MiniRpcLib.Logger.Debug   = Logger.LogDebug;
            MiniRpcLib.Logger.Error   = Logger.LogError;
            MiniRpcLib.Logger.Fatal   = Logger.LogFatal;
            MiniRpcLib.Logger.Info    = Logger.LogInfo;
            MiniRpcLib.Logger.Message = Logger.LogMessage;
            MiniRpcLib.Logger.Warning = Logger.LogWarning;
        }
    }
}
