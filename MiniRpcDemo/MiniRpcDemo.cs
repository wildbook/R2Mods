using System;
using BepInEx;
using MiniRpcLib;
using MiniRpcLib.Action;
using MiniRpcLib.Func;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace MiniRpcDemo
{
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency(MiniRpcPlugin.Dependency)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class MiniRpcDemo : BaseUnityPlugin
    {
        private const string ModVer  = "1.0";
        private const string ModName = "MiniRpcDemo";
        private const string ModGuid = "dev.wildbook.minirpc_demo";

        // Define two actions sending/receiving a single string
        public IRpcAction<string> ExampleCommandHost   { get; set; }
        public IRpcAction<string> ExampleCommandClient { get; set; }

        // Define two actions that manages reading/writing messages themselves
        public IRpcAction<Action<NetworkWriter>> ExampleCommandHostCustom { get; set; }
        public IRpcAction<Action<NetworkWriter>> ExampleCommandClientCustom { get; set; }

        // Define two functions of type `string Function(bool);`
        public IRpcFunc<bool, string> ExampleFuncClient { get; set; }
        public IRpcFunc<bool, string> ExampleFuncHost { get; set; }

        public MiniRpcDemo()
        {
            // Fix the damn in-game console stealing our not-in-game consoles output.
            // Not related to the demo, just very useful.
            On.RoR2.RoR2Application.UnitySystemConsoleRedirector.Redirect += orig => { };

            // Create a MiniRpcInstance that automatically registers all commands to our ModGuid
            // This lets us support multiple mods using the same command ID
            // We could also just generate new command ID's without "isolating" them by mod as well, so it would break if mod load order was different for different clients
            // I opted for the ModGuid instead of an arbitrary number or GUID to encourage mods not to set the same ID
            var miniRpc = MiniRpc.CreateInstance(ModGuid);

            // Define two commands, both transmitting a single string
            ExampleCommandHost   = miniRpc.RegisterAction(ExecuteOn.Server, (NetworkUser user, string x) => Debug.Log($"[Host] {user?.userName} sent us: {x}"));
            ExampleCommandClient = miniRpc.RegisterAction(ExecuteOn.Client, (NetworkUser user, string x) => Debug.Log($"[Client] {user?.userName} sent us: {x}"));

            // Define two commands, both deserializing the data themselves

            // This command will be called by a client (including the host), and executed on the server (host)
            ExampleCommandHostCustom = miniRpc.RegisterAction(ExecuteOn.Server, (user, x) =>
            {
                // This is what the server will execute when a client invokes the IRpcAction

                var str   = x.ReadString();
                var int32 = x.ReadInt32();

                Debug.Log($"[Host] {user?.userName} sent us: {str} {int32}");
            });

            // This command will be called by the host, and executed on all clients
            ExampleCommandClientCustom = miniRpc.RegisterAction(ExecuteOn.Client, (user, x) =>
            {
                // This is what all clients will execute when the server invokes the IRpcAction

                var str   = x.ReadString();
                var int32 = x.ReadInt32();

                Debug.Log($"[Client] {user?.userName} sent us: {str} {int32}");
            });

            ExampleFuncHost = miniRpc.RegisterFunc(ExecuteOn.Server, (user, x) =>
            {
                Debug.Log($"[Host] {user?.userName} sent us: {x}");
                return $"Hello from the server, received {x}!";
            });

            ExampleFuncClient = miniRpc.RegisterFunc(ExecuteOn.Client, (user, x) =>
            {
                Debug.Log($"[Client] {user?.userName} sent us: {x}");
                return $"Hello from the client, received {x}!";
            });
        }

        public async void Update()
        {
            // If we hit PageUp on a client, execute ExampleCommandHost on the server with the parameter "C2S!"
            if (Input.GetKeyDown(KeyCode.PageUp))
                ExampleCommandHost.Invoke("C2S!");

            // If we hit PageUp on the server, execute ExampleCommandClient on all clients (including ourselves) with the parameter "S2C!"
            if (Input.GetKeyDown(KeyCode.PageDown))
                ExampleCommandClient.Invoke("S2C!");

            // If we hit Home on the client, execute ExampleCommandHostCustom on the server, which writes a custom message as "parameter"
            if (Input.GetKeyDown(KeyCode.Home))
            {
                ExampleCommandHostCustom.Invoke(x =>
                {
                    x.Write("Test C2S");
                    x.Write(2);
                });
            }

            // If we hit End on the server, execute ExampleCommandHostCustom on all clients, which writes a custom message as "parameter"
            if (Input.GetKeyDown(KeyCode.End))
            {
                ExampleCommandClientCustom.Invoke(x =>
                {
                    x.Write("Test S2C");
                    x.Write(4);
                });
            }

            if (Input.GetKeyDown(KeyCode.Insert))
            {
                var result = await ExampleFuncHost.InvokeAsync(true);
                
                Debug.Log($"Received response ExampleFuncHost: {result}");
            }
            
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                var result = await ExampleFuncClient.InvokeAsync(true);
                
                Debug.Log($"Received response ExampleFuncClient: {result}");
            }
        }
    }
}