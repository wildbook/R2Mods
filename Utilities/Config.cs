using System;
using System.Linq;
using BepInEx.Configuration;
using TinyJson;

namespace Wildbook.R2Mods.Utilities
{
    internal class ConfigWrapperJson<T>
    {
        private readonly ConfigEntry<string> _wrapper;

        public ConfigDefinition Definition => _wrapper.Definition;

        public ConfigFile ConfigFile => _wrapper.ConfigFile;

        public event EventHandler SettingChanged
        {
            add => _wrapper.SettingChanged += value;
            remove => _wrapper.SettingChanged -= value;
        }

        public T Value
        {
            get => _wrapper.Value.FromJson<T>();
            set => _wrapper.Value = value.ToJson();
        }

        public static implicit operator T(ConfigWrapperJson<T> cw) => cw.Value;

        public ConfigWrapperJson(ConfigFile configFile, ConfigDefinition definition, T defaultValue = default) =>
            _wrapper = configFile.Bind(definition, defaultValue.ToJson());
    }

    internal static class ConfigExtensions
    {
        public static ConfigWrapperJson<T> WrapJson<T>(this ConfigFile file, string section, string key, T defaultValue = default)
        {
            // This is a terrible implementation, but BepInEx's list of supported types is internal, and I don't feel like using reflection to get it
            if (new[] { typeof(string), typeof(bool), typeof(int) }.Contains(typeof(T)))
                throw new Exception("This type is natively supported by BepInEx's ConfigWrapper / Wrap. Use that instead of WrapJson.");

            return new ConfigWrapperJson<T>(file, new ConfigDefinition(section, key), defaultValue);
        }
    }
}
