using BepInEx.Configuration;
using UnityEngine;

namespace TTGL_Survivor.Modules
{
    public enum FrequencyConfig
    {
        Always,
        OncePerRun,
        Never
    }
    public static class Config
    {
        public static ConfigEntry<bool> lagannItemDisplayEnabled;
        public static ConfigEntry<bool> gurrenItemDisplayEnabled;
        public static ConfigEntry<bool> gurrenLaganItemDisplayEnabled;

        public static ConfigEntry<bool> spiralGaugeEnabled;
        public static ConfigEntry<Vector2> spiralGaugeAnchorMin;
        public static ConfigEntry<Vector2> spiralGaugeAnchorMax;
        public static ConfigEntry<Vector2> spiralGaugePivot;
        public static ConfigEntry<Vector2> spiralGaugeSizeDelta;
        public static ConfigEntry<Vector2> spiralGaugeAnchoredPosition;
        public static ConfigEntry<Vector3> spiralGaugeLocalScale;

        public static ConfigEntry<bool> ttglMusicEnabled;
        public static ConfigEntry<FrequencyConfig> ttglShowCombiningAnimation;
        public static ConfigEntry<bool> woopsEnabled;


        public static void ReadConfig()
        {
            lagannItemDisplayEnabled = Modules.Config.GetSetConfig("Item display", "Lagann", true, "Display Items on Lagann");
            gurrenItemDisplayEnabled = Modules.Config.GetSetConfig("Item display", "Gurren", true, "Display Items on Gurren");
            gurrenLaganItemDisplayEnabled = Modules.Config.GetSetConfig("Item display", "Gurren Lagann", true, "Display Items on Gurren Lagann");

            spiralGaugeEnabled = Modules.Config.GetSetConfig("Spiral Gauge", "Enabled", true, "Display Spiral Gauge");
            spiralGaugeAnchorMin = Modules.Config.GetSetConfig("Spiral Gauge", "AnchorMin", new Vector2(1, 0), "Spiral Gauge AnchorMin");
            spiralGaugeAnchorMax = Modules.Config.GetSetConfig("Spiral Gauge", "AnchorMax", new Vector2(1, 0), "Spiral Gauge AnchorMax");
            spiralGaugePivot = Modules.Config.GetSetConfig("Spiral Gauge", "Pivot", new Vector2(1, 0), "Spiral Gauge Pivot");
            spiralGaugeSizeDelta = Modules.Config.GetSetConfig("Spiral Gauge", "SizeDelta", new Vector2(120, 120), "Spiral Gauge SizeDelta");
            spiralGaugeAnchoredPosition = Modules.Config.GetSetConfig("Spiral Gauge", "AnchoredPosition", new Vector2(-20, 200), "Spiral Gauge AnchoredPosition");
            spiralGaugeLocalScale = Modules.Config.GetSetConfig("Spiral Gauge", "LocalScale", new Vector3(2, 2, 2), "Spiral Gauge LocalScale");

            ttglMusicEnabled = Modules.Config.GetSetConfig("TTGLMusic", "Enabled", true, "Set to false to disable TTGL music");
            ttglShowCombiningAnimation = Modules.Config.GetSetConfig("Gurren Lagann Combine Cinematic", "Frequency", FrequencyConfig.Always, "Set the frequency at which the cinematic plays");
            woopsEnabled = Modules.Config.GetSetConfig("WoopsSkin", "Enabled", false, "Set to true to enable woops skin");
        }

        // this helper automatically makes config entries for disabling survivors
        internal static ConfigEntry<T> GetSetConfig<T>(string section, string key, T defaultValue, string description)
        {
            return TTGL_SurvivorPlugin.instance.Config.Bind<T>(new ConfigDefinition(section, key), defaultValue, new ConfigDescription(description));
        }
    }
}