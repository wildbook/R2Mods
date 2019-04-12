using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using RoR2;
using UnityEngine;

namespace Multitudes
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("dev.wildbook.multitudes", "Multitudes", "1.0")]
    public class Multitudes : BaseUnityPlugin
    {
        public static int Multiplier = 4;

        public void Awake()
        {
            // Needed to register our commands as well when the game registers its own
            IL.RoR2.Console.Awake += il =>
            {
                var c = new ILCursor(il);
                c.GotoNext(x => x.MatchStloc(1));
                c.EmitDelegate<Func<Type[], Type[]>>(orig => orig.Concat(GetType().Assembly.GetTypes()).ToArray());
            };

            IL.RoR2.Run.FixedUpdate += il =>
            {
                var c = new ILCursor(il);
                c.GotoNext(x => x.MatchCallvirt<Run>("set_livingPlayerCount"));
                c.EmitDelegate<Func<int, int>>(x => x * Multiplier);

                c.GotoNext(x => x.MatchCallvirt<Run>("set_participatingPlayerCount"));
                c.EmitDelegate<Func<int, int>>(x => x * Multiplier);
            };
        }

        // Random example command to set multiplier with
        [ConCommand(commandName = "mod_wb_set_multiplier", flags = ConVarFlags.None, helpText = "Lets you pretend to have more friends than you actually do.")]
        private static void CCSetMultiplier(ConCommandArgs args)
        {
            if (args.Count != 1 || !int.TryParse(args[0], out Multiplier))
                Debug.Log("Invalid arguments.");
            else
                Debug.Log($"Multiplier set to {Multiplier}. Good luck!");
        }

        // Random example command to set multiplier with
        [ConCommand(commandName = "mod_wb_get_multiplier", flags = ConVarFlags.None, helpText = "Lets you know how what Multitudes' multiplier is set to.")]
        private static void CCGetMultiplier(ConCommandArgs args)
        {
            Debug.Log($"Your multiplier is currently {Multiplier}. Good luck!");
        }
    }
}