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
        private const string ModVer = "1.0";
        private const string ModName = "MiniRpcDemo";
        private const string ModGuid = "dev.wildbook.minirpc_demo";

        // Define two actions sending/receiving a single string
        public IRpcAction<string> ExampleCommandHost { get; set; }
        public IRpcAction<string> ExampleCommandClient { get; set; }

        // Define two actions that manages reading/writing messages themselves
        public IRpcAction<Action<NetworkWriter>> ExampleCommandHostCustom { get; set; }
        public IRpcAction<Action<NetworkWriter>> ExampleCommandClientCustom { get; set; }

        // Define two functions of type `string Function(bool);`
        public IRpcFunc<bool, string> ExampleFuncClient { get; set; }
        public IRpcFunc<bool, string> ExampleFuncHost { get; set; }

        // Define two functions of type `ExampleObject Function(ExampleObject);`
        public IRpcFunc<ExampleObject, ExampleObject> ExampleFuncClientObject { get; set; }

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
            ExampleCommandHost = miniRpc.RegisterAction(Target.Server, (NetworkUser user, string x) => Debug.Log($"[Host] {user?.userName} sent us: {x}"));
            ExampleCommandClient = miniRpc.RegisterAction(Target.Client, (NetworkUser user, string x) => Debug.Log($"[Client] Host sent us: {x}"));

            // Define two commands, both deserializing the data themselves

            // This command will be called by a client (including the host), and executed on the server (host)
            ExampleCommandHostCustom = miniRpc.RegisterAction(Target.Server, (user, x) =>
            {
                // This is what the server will execute when a client invokes the IRpcAction

                var str = x.ReadString();
                var int32 = x.ReadInt32();

                Debug.Log($"[Host] {user?.userName} sent us: {str} {int32}");
            });

            // This command will be called by the host, and executed on all clients
            ExampleCommandClientCustom = miniRpc.RegisterAction(Target.Client, (user, x) =>
            {
                // This is what all clients will execute when the server invokes the IRpcAction

                var str = x.ReadString();
                var int32 = x.ReadInt32();

                Debug.Log($"[Client] Host sent us: {str} {int32}");
            });

            // Here's three examples of RegisterFunc, where you also need to return a value to the caller
            ExampleFuncHost = miniRpc.RegisterFunc<bool, string>(Target.Server, (user, x) =>
            {
                Debug.Log($"[Host] {user?.userName} sent us: {x}");
                return $"Hello from the server, received {x}!";
            });

            ExampleFuncClient = miniRpc.RegisterFunc<bool, string>(Target.Client, (user, x) =>
            {
                Debug.Log($"[Client] Host sent us: {x}");
                return $"Hello from the client, received {x}!";
            });

            ExampleFuncClientObject = miniRpc.RegisterFunc(Target.Client, (NetworkUser user, ExampleObject obj) =>
            {
                Debug.Log($"[Client] Host sent us: {obj}");
                obj.StringExample = "Edited client-side!";
                return obj;
            });

            // By default, MiniRpcLib will create an ID based on the registration order (first command is ID 0, second command is ID 1, and so on
            // If you want to specify an ID manually, you can choose to do so by doing either of these:
            //
            // RpcActions and RpcFuncs have separate IDs, so both an RpcFunc and an RpcAction can have the same ID without collisions.
            // That said, there's nothing stopping you from using the same Enum for both, as all ID values are valid.
            // (1, 2, 3 being Actions, 4 being a Func, 5 being an action again and so on is okay and valid)

            _ = miniRpc.RegisterFunc(Target.Client, (NetworkUser user, ExampleObject obj) =>
            {
                Debug.Log($"[Client] Host sent us: {obj}");
                obj.StringExample = "Edited client-side!";
                return obj;
            }, 1234); // <-- Optional ID

            _ = miniRpc.RegisterFunc(Target.Client, (NetworkUser user, ExampleObject obj) =>
            {
                Debug.Log($"[Client] Host sent us: {obj}");
                obj.StringExample = "Edited client-side!";
                return obj;
            }, CommandId.SomeCommandName); // <-- Optional ID

            // The "_ ="'s above mean that the return value will be ignored. In your code you should assign the return value to something to be able to call the function.
        }

        // This enum only exists to show that it can be used as ID for an RpcAction/RpcFunc
        enum CommandId
        {
            //                     ----|    This number is only needed because we already created an RpcFunc with ID 0 (the first one we made without an ID).
            SomeCommandName      = 2345, // If you use IDs in your own code, you will most likely want to give all commands explicit IDs, which will avoid this issue.
            SomeOtherCommandName,
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
                Debug.Log("[MiniRpcDemo] Sending request ExampleFuncHost.");
                ExampleFuncHost.Invoke(true, result =>
                {
                    Debug.Log($"[MiniRpcDemo] Received response ExampleFuncHost: {result}");
                });
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                Debug.Log("[MiniRpcDemo] Sending request ExampleFuncClient.");
                ExampleFuncClient.Invoke(true, result =>
                {
                    Debug.Log($"[MiniRpcDemo] Received response ExampleFuncClient: {result}");
                });
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Debug.Log("[MiniRpcDemo] Sending request ExampleFuncClientObject.");
                ExampleFuncClientObject.Invoke(new ExampleObject(true, 28, "Pure"), result =>
                {
                    Debug.Log($"[MiniRpcDemo] Received response ExampleFuncClientObject: {result}");
                });
            }
        }
    }

    public class ExampleObject : MessageBase
    {
        public bool   BoolExample;
        public int    IntExample;
        public string StringExample;

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(BoolExample);
            writer.Write(IntExample);
            writer.Write(StringExample);
        }

        public override void Deserialize(NetworkReader reader)
        {
            BoolExample   = reader.ReadBoolean();
            IntExample    = reader.ReadInt32();
            StringExample = reader.ReadString();
        }
        public ExampleObject(bool boolExample, int intExample, string stringExample)
        {
            BoolExample   = boolExample;
            IntExample    = intExample;
            StringExample = stringExample;
        }

        public override string ToString() => $"ExampleObject: {BoolExample}, {IntExample}, {StringExample}";
    }
}