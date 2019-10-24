using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;

namespace TooManyFriends
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("dev.wildbook.toomanyfriends", "TooManyFriends", "1.0")]
    public class TooManyFriends : BaseUnityPlugin
    {
        private static (int maxPlayers, int hardMaxPlayers, int maxLocalPlayers) _default;

        private static ConfigWrapper<int> LobbySizeConfig { get; set; }

        public static int LobbySize
        {
            get => LobbySizeConfig.Value;
            protected set => LobbySizeConfig.Value = value;
        }

        public TooManyFriends()
        {
            _default = (
                Reflection.GetFieldValue<int>(typeof(RoR2Application), "maxPlayers"),
                Reflection.GetFieldValue<int>(typeof(RoR2Application), "hardMaxPlayers"),
                Reflection.GetFieldValue<int>(typeof(RoR2Application), "maxLocalPlayers")
            );

            LobbySizeConfig = Config.Wrap(
                "Game",
                "LobbySize",
                "Sets the max size of custom game lobbies.",
                16);

            LobbySizeConfig.SettingChanged += (sender, args) => SetLobbySize(LobbySize);
        }

        public void OnEnable() => SetLobbySize(LobbySize);

        public void OnDisable() => SetLobbySize(_default.maxPlayers, _default.hardMaxPlayers, _default.maxLocalPlayers);

        public void SetLobbySize(int maxPlayers, int? hardMaxPlayers = null, int? maxLocalPlayers = null)
        {
            Reflection.SetFieldValue(typeof(RoR2Application), "maxPlayers",      maxPlayers);
            Reflection.SetFieldValue(typeof(RoR2Application), "hardMaxPlayers",  hardMaxPlayers  ?? maxPlayers);
            Reflection.SetFieldValue(typeof(RoR2Application), "maxLocalPlayers", maxLocalPlayers ?? maxPlayers);
        }
    }
}