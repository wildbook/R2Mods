using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using RoR2.Networking;
using UnityEngine;

namespace TooManyFriends
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("dev.wildbook.toomanyfriends", "TooManyFriends", "1.1.0")]
    [R2APISubmoduleDependency(nameof(CommandHelper))]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    public class TooManyFriends : BaseUnityPlugin
    {
        private static (int maxPlayers, int hardMaxPlayers, int maxLocalPlayers) _default;

        private static ConfigEntry<int> LobbySizeConfig { get; set; }

        public static int LobbySize
        {
            get => LobbySizeConfig.Value;
            protected set => LobbySizeConfig.Value = value;
        }

        public TooManyFriends()
        {
            CommandHelper.AddToConsoleWhenReady();

            _default = (
                RoR2Application.maxPlayers,
                RoR2Application.hardMaxPlayers,
                RoR2Application.maxLocalPlayers
            );

            LobbySizeConfig = Config.Bind("Game", "LobbySize", 16, "Sets the max size of custom game lobbies");

            LobbySizeConfig.SettingChanged += (sender, args) => SetLobbySize(LobbySize);
        }

        public void OnEnable() => SetLobbySize(LobbySize);

        public void OnDisable() => SetLobbySize(_default.maxPlayers, _default.hardMaxPlayers, _default.maxLocalPlayers);

        public void SetLobbySize(int maxPlayers, int? hardMaxPlayers = null, int? maxLocalPlayers = null)
        {
            typeof(RoR2Application).SetFieldValue("maxPlayers",      maxPlayers);
            typeof(RoR2Application).SetFieldValue("hardMaxPlayers",  hardMaxPlayers  ?? maxPlayers);
            typeof(RoR2Application).SetFieldValue("maxLocalPlayers", maxLocalPlayers ?? maxPlayers);
            SteamworksLobbyManager.cvSteamLobbyMaxMembers.defaultValue = maxPlayers.ToString();
            SteamworksLobbyManager.cvSteamLobbyMaxMembers.SetPropertyValue("value", maxPlayers);
            GameNetworkManager.SvMaxPlayersConVar.instance.SetString(maxPlayers.ToString());
        }

        [ConCommand(commandName = "mod_tmf", flags = ConVarFlags.None, helpText = "Lets you change the max size of custom game lobbies.")]
        private static void CCSetMaxLobbySize(ConCommandArgs args)
        {
            args.CheckArgumentCount(1);
 
            if (!int.TryParse(args[0], out var lobbySize))
            {
                Debug.Log("Invalid argument.");
            }
            else
            {
                LobbySizeConfig.Value = lobbySize;
                Debug.Log($"Lobby max size set to {LobbySizeConfig.Value}.");
            }
        }
    }
}