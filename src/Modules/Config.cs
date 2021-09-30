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
        public static bool lagannItemDisplayEnabled;
        public static bool gurrenItemDisplayEnabled;
        public static bool gurrenLaganItemDisplayEnabled;

        public static bool spiralGaugeEnabled;
        public static Vector2 spiralGaugeAnchorMin;
        public static Vector2 spiralGaugeAnchorMax;
        public static Vector2 spiralGaugePivot;
        public static Vector2 spiralGaugeSizeDelta;
        public static Vector2 spiralGaugeAnchoredPosition;
        public static Vector3 spiralGaugeLocalScale;

        public static bool ttglMusicEnabled;
        public static FrequencyConfig ttglShowCombiningAnimation;
        public static bool woopsEnabled;

        public static bool trackVRCameraToHeadPosition;

        public static void ReadConfig()
        {
            lagannItemDisplayEnabled = Modules.Config.GetSetConfig("Item display", "Lagann", true, "Display Items on Lagann").Value;
            gurrenItemDisplayEnabled = Modules.Config.GetSetConfig("Item display", "Gurren", true, "Display Items on Gurren").Value;
            gurrenLaganItemDisplayEnabled = Modules.Config.GetSetConfig("Item display", "Gurren Lagann", true, "Display Items on Gurren Lagann").Value;

            spiralGaugeEnabled = Modules.Config.GetSetConfig("Spiral Gauge", "Enabled", true, "Display Spiral Gauge").Value;
            spiralGaugeAnchorMin = Modules.Config.GetSetConfig("Spiral Gauge", "AnchorMin", new Vector2(1, 0), "Spiral Gauge AnchorMin").Value;
            spiralGaugeAnchorMax = Modules.Config.GetSetConfig("Spiral Gauge", "AnchorMax", new Vector2(1, 0), "Spiral Gauge AnchorMax").Value;
            spiralGaugePivot = Modules.Config.GetSetConfig("Spiral Gauge", "Pivot", new Vector2(1, 0), "Spiral Gauge Pivot").Value;
            spiralGaugeSizeDelta = Modules.Config.GetSetConfig("Spiral Gauge", "SizeDelta", new Vector2(120, 120), "Spiral Gauge SizeDelta").Value;
            spiralGaugeAnchoredPosition = Modules.Config.GetSetConfig("Spiral Gauge", "AnchoredPosition", new Vector2(-20, 200), "Spiral Gauge AnchoredPosition").Value;
            spiralGaugeLocalScale = Modules.Config.GetSetConfig("Spiral Gauge", "LocalScale", new Vector3(2, 2, 2), "Spiral Gauge LocalScale").Value;

            ttglMusicEnabled = Modules.Config.GetSetConfig("TTGLMusic", "Enabled", true, "Set to false to disable TTGL music").Value;
            ttglShowCombiningAnimation = Modules.Config.GetSetConfig("Gurren Lagann Combine Cinematic", "Frequency", FrequencyConfig.Always, "Set the frequency at which the cinematic plays").Value;
            woopsEnabled = Modules.Config.GetSetConfig("WoopsSkin", "Enabled", false, "Set to true to enable woops skin").Value;

            trackVRCameraToHeadPosition = Modules.Config.GetSetConfig("VR API", "Lagann - Track VR Camera to Simons head", true, "The VR camera will track to Simons head position").Value;

        }

        // this helper automatically makes config entries for disabling survivors
        internal static ConfigEntry<T> GetSetConfig<T>(string section, string key, T defaultValue, string description)
        {
            return TTGL_SurvivorPlugin.instance.Config.Bind<T>(new ConfigDefinition(section, key), defaultValue, new ConfigDescription(description));
        }
    }
}