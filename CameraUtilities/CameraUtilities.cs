using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace CameraUtilities
{
    internal static class CodeInstructionExtensions
    {
        public static bool MatchLdloc(this CodeInstruction code, out int operand)
        {
            if (!code.IsLdloc())
            {
                operand = 0;
                return false;
            }
            OpCode opCode = code.opcode;
            operand = opCode == OpCodes.Ldloc_0 ? 0 :
                opCode == OpCodes.Ldloc_1 ? 1 :
                opCode == OpCodes.Ldloc_2 ? 2 :
                opCode == OpCodes.Ldloc_3 ? 3 :
                (int)code.operand;
            return true;
        }
        public static bool MatchStloc(this CodeInstruction code, out int operand)
        {
            if (!code.IsStloc())
            {
                operand = 0;
                return false;
            }
            OpCode opCode = code.opcode;
            operand = opCode == OpCodes.Stloc_0 ? 0 :
                opCode == OpCodes.Stloc_1 ? 1 :
                opCode == OpCodes.Stloc_2 ? 2 :
                opCode == OpCodes.Stloc_3 ? 3 :
                (int)code.operand;
            return true;
        }
        public static bool MatchStloc(this CodeInstruction code, int operand) =>
            code.MatchStloc(out int matched_operand) && operand == matched_operand;
    }

    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("dev.wildbook.camerautilities", "CameraUtilities", "1.0.0")]
    public class CameraUtilities : BaseUnityPlugin
    {
        internal Harmony harmony = new Harmony("dev.wildbook.camerautilities");

        private static ConfigEntry<float> SensSprintingX { get; set; }
        private static ConfigEntry<float> SensSprintingY { get; set; }

        public void Awake()
        {
            SensSprintingX = Config.Bind(
                "Sensitivity",
                "SprintingX",
                1.0f,
                "Sets the multiplier to use for mouse sensitivity when sprinting (X-Axis).\n[Game Default: 0.5]");

            SensSprintingY = Config.Bind(
                "Sensitivity",
                "SprintingY",
                1.0f,
                "Sets the multiplier to use for mouse sensitivity when sprinting (Y-Axis).\n[Game Default: 0.5]");
        }

        public void OnEnable() => harmony.PatchAll(typeof(CameraUtilities));

        public void OnDisable() => harmony.UnpatchSelf();

        [HarmonyPatch(typeof(RoR2.CameraRigController), nameof(RoR2.CameraRigController.Update))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CameraRigControllerOnUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int xFov = 0, yFov = 0;

            CodeMatcher codeMatcher = new CodeMatcher();
            codeMatcher.Instructions().AddRange(instructions.Select(c => new CodeInstruction(c)));
            return codeMatcher.MatchStartForward(
                new CodeMatch(c => c.MatchLdloc(out xFov)),
                new CodeMatch(OpCodes.Ldc_R4, 0.5f),
                new CodeMatch(OpCodes.Mul),
                new CodeMatch(c => c.MatchStloc(xFov)),

                new CodeMatch(c => c.MatchLdloc(out yFov)),
                new CodeMatch(OpCodes.Ldc_R4, 0.5f),
                new CodeMatch(OpCodes.Mul),
                new CodeMatch(c => c.MatchStloc(yFov))
            ).Advance(1).SetOperandAndAdvance(
                SensSprintingX.Value
            ).Advance(3).SetOperandAndAdvance(
                SensSprintingY.Value
            ).InstructionEnumeration();
        }
    }
}