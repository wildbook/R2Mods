using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using BepInEx;
using MonoMod.Cil;
using RoR2;
using UnityEngine;

namespace Multitudes
{
    public class CommandHelper
    {
        public static void RegisterCommands(RoR2.Console self)
        {
            var types = typeof(CommandHelper).Assembly.GetTypes();
            var catalog = self.GetFieldValue<IDictionary>("concommandCatalog");

            foreach (var methodInfo in types.SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)))
            {
                var customAttributes = methodInfo.GetCustomAttributes(false);
                foreach (var attribute in customAttributes.OfType<ConCommandAttribute>())
                {
                    var conCommand = Reflection.GetNestedType<RoR2.Console>("ConCommand").Instantiate();

                    conCommand.SetFieldValue("flags", attribute.flags);
                    conCommand.SetFieldValue("helpText", attribute.helpText);
                    conCommand.SetFieldValue("action", (RoR2.Console.ConCommandDelegate)Delegate.CreateDelegate(typeof(RoR2.Console.ConCommandDelegate), methodInfo));

                    catalog[attribute.commandName.ToLower()] = conCommand;
                }
            }
        }
    }

    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("dev.wildbook.multitudes", "Multitudes", "1.2.0")]
    public class Multitudes : BaseUnityPlugin
    {
        public static int Multiplier = 4;

        public void Awake()
        {
            On.RoR2.Console.Awake += (orig, self) =>
            {
                CommandHelper.RegisterCommands(self);
                orig(self);
            };

            IL.RoR2.Run.FixedUpdate += il =>
            {
                var c = new ILCursor(il);
                c.GotoNext(x => x.MatchCallvirt<Run>("set_livingPlayerCount"));
                c.EmitDelegate<Func<int, int>>(x => x * Multiplier);

                c.GotoNext(x => x.MatchCallvirt<Run>("set_participatingPlayerCount"));
                c.EmitDelegate<Func<int, int>>(x => x * Multiplier);
            };

            On.RoR2.TeleporterInteraction.GetPlayerCountInRadius += (orig, self) => orig(self) * Multiplier;
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
        [ConCommand(commandName = "mod_wb_get_multiplier", flags = ConVarFlags.None, helpText = "Lets you know what Multitudes' multiplier is set to.")]
        private static void CCGetMultiplier(ConCommandArgs args)
        {
            Debug.Log($"Your multiplier is currently {Multiplier}. Good luck!");
        }
    }
}