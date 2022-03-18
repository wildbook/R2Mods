using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
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
        internal Harmony harmony = new Harmony("dev.wildbook.multitudes");

        public static ConfigEntry<int> MultiplierConfig { get; set; }
        public static ConfigEntry<int> DivisorConfig { get; set; }
        public static ConfigEntry<bool> ShouldAffectTeleporterChargeRateConfig { get; set; }
 
        public static int Multiplier
        {
            get => MultiplierConfig.Value;
            protected set => MultiplierConfig.Value = value;
        }
        public static int Divisor {
            get => DivisorConfig.Value;
            protected set => DivisorConfig.Value = value;
        }

        public void Awake()
        {
            MultiplierConfig = Config.Bind(
                "Game",
                "Multiplier",
                4,
                "Sets the multiplier for Multitudes.");

            DivisorConfig = Config.Bind(
                "Game",
                "Divisor",
                1,
                "Sets the divisor for Multitudes. Division occurs after multiplication. Avoid non-integer quotients.");

            ShouldAffectTeleporterChargeRateConfig = Config.Bind(
                "Game",
                "Should Affect Teleporter Charge Rate",
                false,
                "Sets if the Multitudes adjustment should affect the speed at which the teleporter is charging at.");
 
            CommandHelper.AddToConsoleWhenReady();
 
            Run.onRunStartGlobal += run => { SendAdjustmentChat(); };

            harmony.PatchAll(typeof(Multitudes));
        }

        public void OnDestroy() => harmony.UnpatchSelf();

        [HarmonyPatch(typeof(Run), nameof(Run.livingPlayerCount), MethodType.Getter)]
        [HarmonyPatch(typeof(Run), nameof(Run.participatingPlayerCount), MethodType.Getter)]
        [HarmonyPostfix]
        public static int AdjustPlayerCount(int playerCount) => playerCount * Multiplier / Divisor;
        // This isn't perfect, but it works and the user can not pick fractional adjustments.
        public static int UnadjustPlayerCount(int adjustedPlayerCount) => adjustedPlayerCount * Divisor / Multiplier;

        [HarmonyPatch(typeof(HoldoutZoneController), nameof(HoldoutZoneController.CountPlayersInRadius))]
        [HarmonyPostfix]
        public static int AdjustTeleporterChargeRate(int chargeRate) =>
            ShouldAffectTeleporterChargeRateConfig.Value ? AdjustPlayerCount(chargeRate) : chargeRate;
        public static int UnadjustTeleporterChargeRate(int adjustedChargeRate) =>
            ShouldAffectTeleporterChargeRateConfig.Value ? UnadjustPlayerCount(adjustedChargeRate) : adjustedChargeRate;

        // Fix final boss failing to trigger
        [HarmonyPatch(typeof(AllPlayersTrigger), nameof(AllPlayersTrigger.FixedUpdate))]
        // Fix final zone failing to trigger
        [HarmonyPatch(typeof(MultiBodyTrigger), nameof(MultiBodyTrigger.FixedUpdate))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetTrueLivingPlayerCountTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var get_livingPlayerCount = typeof(Run).GetPropertyCached(nameof(Run.livingPlayerCount)).GetGetMethod();
            CodeMatcher codeMatcher = new CodeMatcher();
            codeMatcher.Instructions().AddRange(instructions.Select(c => new CodeInstruction(c)));
            return codeMatcher.MatchEndForward(
                new CodeMatch(OpCodes.Call, get_livingPlayerCount),
                new CodeMatch(OpCodes.Callvirt, get_livingPlayerCount)
            ).Repeat(matcher => matcher.Advance(1).InsertAndAdvance(
                Transpilers.EmitDelegate<Func<int, int>>(adjustedPlayerCount => UnadjustPlayerCount(adjustedPlayerCount))
            )).InstructionEnumeration();
        }

        [ConCommand(commandName = "mod_wb_get_adjustment", flags = ConVarFlags.None,
            helpText = "Lets you know what Multitudes' adjustment is.")]
        private static void CCGetAdjustment(ConCommandArgs args) {
            Debug.Log(args.Count != 0
                ? "Invalid arguments. Did you mean `mod_wb_set_multiplier` or `mod_wb_set_divisor`?"
                : $"Your adjustment is currently {MultiplierConfig.Value}/{DivisorConfig.Value}. Good luck!");
        }

        [ConCommand(commandName = "mod_wb_get_multiplier", flags = ConVarFlags.None,
            helpText = "Lets you know what Multitudes' multiplier is set to.")]
        private static void CCGetMultiplier(ConCommandArgs args)
        {
            Debug.Log(args.Count != 0
                ? "Invalid arguments. Did you mean `mod_wb_set_multiplier`?"
                : $"Your multiplier is currently {MultiplierConfig.Value}. Good luck!");
        }

        [ConCommand(commandName = "mod_wb_set_multiplier", flags = ConVarFlags.None,
            helpText = "Lets you pretend to have more friends than you actually do.")]
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
                SendAdjustmentChat();
            }
        }

        [ConCommand(commandName = "mod_wb_get_divisor", flags = ConVarFlags.None,
            helpText = "Lets you know what Multitudes' divisor is set to.")]
        private static void CCGetDivisor(ConCommandArgs args)
        {
            Debug.Log(args.Count != 0
                ? "Invalid arguments. Did you mean `mod_wb_set_divisor`?"
                : $"Your divisor is currently {DivisorConfig.Value}. Good luck!");
        }

        [ConCommand(commandName = "mod_wb_set_divisor", flags = ConVarFlags.None,
            helpText = "Lets you pretend to have less friends than you actually do.")]
        private static void CCSetDivisor(ConCommandArgs args)
        {
            args.CheckArgumentCount(1);

            if (!int.TryParse(args[0], out var divisor))
            {
                Debug.Log("Invalid argument.");
            }
            else
            {
                DivisorConfig.Value = divisor;
                Debug.Log($"Divisor set to {DivisorConfig.Value}. Good luck!");
                SendAdjustmentChat();
            }
        }

        private static void SendAdjustmentChat()
        {
            // If we're not host, we're not setting it for the current lobby
            // That also means no one cares what our Multitudes is set to
            if (!NetworkServer.active)
                return;

            // If there's no active run, estimate with connected player controllers.
            int playerCount = Run.instance != null ? Run.instance.participatingPlayerCount
                : AdjustPlayerCount(NetworkServer.connections.SelectMany(conn => conn.playerControllers).Count());
            Chat.SendBroadcastChat(
                new Chat.SimpleChatMessage
                {
                    baseToken = "Multitudes adjustment set to: {0}/{1} (current adjusted player count: {2})",
                    paramTokens = new[]
                    {
                        MultiplierConfig.Value.ToString(),
                        DivisorConfig.Value.ToString(),
                        playerCount.ToString(),
                    }
                });
        }

        [ConCommand(commandName = "mod_wb_set_teleporter_rate", flags = ConVarFlags.None,
            helpText = "Should the Multitudes adjustment affect the speed at which the teleporter is charging at?")]
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
                Debug.Log("Invalid argument. Correct usage is: `mod_wb_set_teleporter_rate true` / `mod_wb_set_teleporter_rate false`");
            }
        }
    }
}
