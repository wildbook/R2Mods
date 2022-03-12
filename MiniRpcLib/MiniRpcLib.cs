using BepInEx;
using HarmonyLib;
using MiniRpcLib.RpcLayer;

namespace MiniRpcLib
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class MiniRpcPlugin : BaseUnityPlugin
    {
        internal Harmony harmony = new Harmony("dev.wildbook.minirpcplugin");

        public const string Dependency = ModGuid;

        private const string ModVer = "1.1";
        private const string ModName = "MiniRpcLib";
        private const string ModGuid = "dev.wildbook.libminirpc";

        public MiniRpcPlugin()
        {
            Logger.LogInfo("Initializing Logger");
            InitLogger();
        }

        public void Awake()
        {
            Logger.LogInfo("Initializing MiniRpc");
            MiniRpc.Initialize(harmony, new UnityMessageHandler());
        }

        public void OnDestroy() => harmony.UnpatchSelf();

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
