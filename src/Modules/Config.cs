using BepInEx.Configuration;

namespace TTGL_Survivor.Modules
{
    public enum FrequencyConfig
    {
        Always,
        Once,
        Never
    }
    public static class Config
    {
        public static ConfigEntry<bool> ttglMusicEnabled;
        public static ConfigEntry<FrequencyConfig> ttglShowCombiningAnimation;
        public static ConfigEntry<bool> woopsEnabled;
        public static void ReadConfig()
        {
            // there actually isn't any config right now but if you wanted to add some it would go here.
            ttglMusicEnabled = Modules.Config.GetSetConfig("TTGLMusic", "Enabled", true, "Set to false to disable TTGL music");
            ttglShowCombiningAnimation = Modules.Config.GetSetConfig("Gurren Lagann Combine Cinematic", "Frequency", FrequencyConfig.Once, "Set the frequency at which the cinematic plays");
            woopsEnabled = Modules.Config.GetSetConfig("WoopsSkin", "Enabled", false, "Set to true to enable woops skin");            
        }

        // this helper automatically makes config entries for disabling survivors
        internal static ConfigEntry<T> GetSetConfig<T>(string section, string key, T defaultValue, string description)
        {
            return TTGL_SurvivorPlugin.instance.Config.Bind<T>(new ConfigDefinition(section, key), defaultValue, new ConfigDescription(description));
        }
    }
}