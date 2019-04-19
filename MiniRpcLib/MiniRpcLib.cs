using BepInEx;
using UnityEngine;

namespace MiniRpcLib
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class MiniRpcPlugin : BaseUnityPlugin
    {
        public const string Dependency = ModGuid;
        
        private const string ModVer    = "1.0";
        private const string ModName   = "MiniRpcLib";
        private const string ModGuid   = "dev.wildbook.lib-minirpc";

        public MiniRpcPlugin()
        {
            On.RoR2.RoR2Application.UnitySystemConsoleRedirector.Redirect += orig => { };
            
            Debug.Log("Initializing MiniRpc");
            MiniRpc.Initialize();
        }
    }
}
