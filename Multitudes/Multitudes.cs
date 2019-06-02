using System;
using BepInEx;
using BepInEx.Configuration;
using MonoMod.Cil;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Multitudes
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("dev.wildbook.multitudes", "Multitudes", "1.3.0")]
    public class Multitudes : BaseUnityPlugin
    {
        private static ConfigWrapper<float> MultiplierConfig { get; set; }

        public float Multiplier
        {
            get => MultiplierConfig.Value * (enabled ? 1 : 0);
            protected set => MultiplierConfig.Value = value;
        }

        public void Awake()
        {
            MultiplierConfig = Config.Wrap<float>(
                "Game",
                "Multiplier",
                "Sets the multiplier for Multitudes. Rounds number of players * multipler down to nearest integer.",
                4);

            On.RoR2.Console.Awake += (orig, self) =>
            {
                CommandHelper.RegisterCommands(self);
                orig(self);
            };

            IL.RoR2.Run.FixedUpdate += il =>
            {
                var c = new ILCursor(il);
                c.GotoNext(x => x.MatchCallvirt<Run>("set_livingPlayerCount"));
                c.EmitDelegate<Func<int, int>>(x => (int)(x * Multiplier));

                c.GotoNext(x => x.MatchCallvirt<Run>("set_participatingPlayerCount"));
                c.EmitDelegate<Func<int, int>>(x => (int)(x * Multiplier));
            };

            Run.onRunStartGlobal += run => { SendMultiplierChat(); };

            // Round up players * multiplier for teleporter counter to prevent extra slow charging
            On.RoR2.TeleporterInteraction.GetPlayerCountInRadius += (orig, self) => Mathf.CeilToInt(orig(self) * Multiplier);
        }

        // Random example command to set multiplier with
        [ConCommand(commandName = "mod_wb_set_multiplier", flags = ConVarFlags.None, helpText = "Lets you pretend to have more friends than you actually do.")]
        private static void CCSetMultiplier(ConCommandArgs args)
        {
            args.CheckArgumentCount(1);

            if (!float.TryParse(args[0], out var multiplier))
            {
                Debug.Log("Invalid argument.");
            }
            else
            {
                MultiplierConfig.Value = multiplier;
                Debug.Log($"Multiplier set to {MultiplierConfig.Value}. Good luck!");
                SendMultiplierChat();
            }
        }

        private static void SendMultiplierChat()
        {
            // If we're not host, we're not setting it for the current lobby
            // That also means no one cares what our Multitudes is set to
            if (!NetworkServer.active)
                return;

            Chat.SendBroadcastChat(
                new Chat.SimpleChatMessage
                {
                    baseToken = "<color=lightblue>Multitudes set to: </color> {0}",
                    paramTokens = new[]
                    {
                        MultiplierConfig.Value.ToString()
                    }
                });
        }

        // Random example command to set multiplier with
        [ConCommand(commandName = "mod_wb_get_multiplier", flags = ConVarFlags.None, helpText = "Lets you know what Multitudes' multiplier is set to.")]
        private static void CCGetMultiplier(ConCommandArgs args)
        {
            Debug.Log(args.Count != 0
                ? "Invalid arguments. Did you mean mod_wb_set_multiplier?"
                : $"Your multiplier is currently {MultiplierConfig.Value}. Good luck!");
        }
    }
}