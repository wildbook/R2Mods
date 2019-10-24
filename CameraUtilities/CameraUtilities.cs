using BepInEx;
using MonoMod.Cil;
using Wildbook.R2Mods.Utilities;

namespace CameraUtilities
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("dev.wildbook.camerautilities", "CameraUtilities", "1.0.0")]
    public class CameraUtilities : BaseUnityPlugin
    {
        private static ConfigWrapperJson<float> SensSprintingX { get; set; }
        private static ConfigWrapperJson<float> SensSprintingY { get; set; }

        public void Awake()
        {
            SensSprintingX = Config.WrapJson(
                "Sensitivity",
                "SprintingX",
                "Sets the multiplier to use for mouse sensitivity when sprinting (X-Axis).\n[Game Default: 0.5]",
                1.0f);

            SensSprintingY = Config.WrapJson(
                "Sensitivity",
                "SprintingY",
                "Sets the multiplier to use for mouse sensitivity when sprinting (Y-Axis).\n[Game Default: 0.5]",
                1.0f);
        }

        public void OnEnable()
        {
            IL.RoR2.CameraRigController.Update += CameraRigControllerOnUpdate;
        }

        public void OnDisable()
        {
            IL.RoR2.CameraRigController.Update -= CameraRigControllerOnUpdate;
        }

        private void CameraRigControllerOnUpdate(ILContext il)
        {
            var c = new ILCursor(il);
            int xFov = 0, yFov = 0;

            c.GotoNext(
                x => x.MatchLdloc(out xFov),
                x => x.MatchLdcR4(0.5f),
                x => x.MatchMul(),
                x => x.MatchStloc(xFov),

                x => x.MatchLdloc(out yFov),
                x => x.MatchLdcR4(0.5f),
                x => x.MatchMul(),
                x => x.MatchStloc(yFov)
            );

            c.Index += 1;
            c.Next.Operand = SensSprintingX.Value;

            c.Index += 4;
            c.Next.Operand = SensSprintingY.Value;
        }
    }
}