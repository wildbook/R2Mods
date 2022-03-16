using System;
using BepInEx;
using BepInEx.Configuration;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Multitudes
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("dev.wildbook.multitudes", "Multitudes", "1.5.6")]
    [R2APISubmoduleDependency(nameof(CommandHelper))]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    public class Multitudes : BaseUnityPlugin
    {
        private static ConfigEntry<int> MultiplierConfig { get; set; }
        private static ConfigEntry<bool> ShouldAffectTeleporterChargeRateConfig { get; set; }
 
        private delegate int RunInstanceReturnInt(Run self);
 
        private static RunInstanceReturnInt origLivingPlayerCountGetter;
        private static RunInstanceReturnInt origParticipatingPlayerCountGetter;
 
        public static int Multiplier
        {
            get => MultiplierConfig.Value;
            protected set => MultiplierConfig.Value = value;
        }
 
        public void Awake()
        {
            MultiplierConfig = Config.Bind(
                "Game",
                "Multiplier",
                4,
                "Sets the multiplier for Multitudes.");

            ShouldAffectTeleporterChargeRateConfig = Config.Bind(
                "Game",
                "Should Affect Teleporter Charge Rate",
                false,
                "Sets if the Multitudes multiplier should affect the speed at which the teleporter is charging at.");
 
            CommandHelper.AddToConsoleWhenReady();
 
            var getLivingPlayerCountHook = new Hook(typeof(Run).GetMethodCached("get_livingPlayerCount"),
                typeof(Multitudes).GetMethodCached(nameof(GetLivingPlayerCountHook)));
            origLivingPlayerCountGetter = getLivingPlayerCountHook.GenerateTrampoline<RunInstanceReturnInt>();
 
            var getParticipatingPlayerCount = new Hook(typeof(Run).GetMethodCached("get_participatingPlayerCount"),
                typeof(Multitudes).GetMethodCached(nameof(GetParticipatingPlayerCountHook)));
            origParticipatingPlayerCountGetter = getParticipatingPlayerCount.GenerateTrampoline<RunInstanceReturnInt>();
 
            Run.onRunStartGlobal += run => { SendMultiplierChat(); };
            
            On.RoR2.HoldoutZoneController.CountPlayersInRadius += (orig, c, origin, chargingRadiusSqr, teamIndex) =>
                orig(c, origin, chargingRadiusSqr, teamIndex) * (ShouldAffectTeleporterChargeRateConfig.Value ? Multiplier : 1);

            IL.RoR2.AllPlayersTrigger.FixedUpdate += FixFinalBossZoneFailingToTrigger;
            IL.RoR2.MultiBodyTrigger.FixedUpdate += FixFinalZoneFailingToTrigger;
        }

        private void FixFinalBossZoneFailingToTrigger(ILContext il)
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                i => i.MatchCallOrCallvirt<Run>("get_livingPlayerCount")))
            {
                c.EmitDelegate<Func<int, int>>(livingPlayerCount => livingPlayerCount / Multiplier);
            }
            else
            {
                Logger.LogError("Failed hooking AllPlayersTrigger.UpdateActivated. Aborting.");
            }
        }

        private void FixFinalZoneFailingToTrigger(ILContext il)
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                i => i.MatchCallOrCallvirt<Run>("get_livingPlayerCount")))
            {
                c.EmitDelegate<Func<int, int>>(livingPlayerCount => livingPlayerCount / Multiplier);
            }
            else
            {
                Logger.LogError("Failed hooking MultiBodyTrigger.UpdateActivated. Aborting.");
            }
        }

        private static int GetLivingPlayerCountHook(Run self) => origLivingPlayerCountGetter(self) * Multiplier;
        private static int GetParticipatingPlayerCountHook(Run self) => origParticipatingPlayerCountGetter(self) * Multiplier;
 
        // Random example command to set multiplier with
        [ConCommand(commandName = "mod_wb_set_multiplier", flags = ConVarFlags.None, helpText = "Lets you pretend to have more friends than you actually do.")]
        private static void CCSetMultiplier(ConCommandArgs args)
        {
            args.CheckArgumentCount(1);
 
            if (!int.TryParse(args[0], out var multiplier))
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
                    baseToken = "Multitudes set to: {0}",
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

        [ConCommand(commandName = "mod_wb_set_teleporter_rate", flags = ConVarFlags.None, helpText = "Should Multitudes multiplier affect the speed at which the teleporter is charging at ?")]
        private static void CCSetMultiplierAffectTeleporterChargeRate(ConCommandArgs args)
        {
            args.CheckArgumentCount(1);

            if (bool.TryParse(args[0], out var shouldAffectTeleporterChargeRate))
            {
                ShouldAffectTeleporterChargeRateConfig.Value = shouldAffectTeleporterChargeRate;
                Debug.Log($"Should Affect Teleporter Charge Rate set to {ShouldAffectTeleporterChargeRateConfig.Value}.");
            }
            else
            {
                Debug.Log("Invalid argument. Correct usage is either : mod_wb_set_teleporter_rate true / mod_wb_set_teleporter_rate false");
            }
        }
    }
}