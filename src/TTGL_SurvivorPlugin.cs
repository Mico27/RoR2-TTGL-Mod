using System;
using BepInEx;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using BepInEx.Logging;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace TTGL_Survivor
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    [R2APISubmoduleDependency(new string[]
    {
        "PrefabAPI",
        "SurvivorAPI",
        "LoadoutAPI",
        "BuffAPI",
        "LanguageAPI",
        "SoundAPI",
        "EffectAPI",
        "UnlockablesAPI",
        "ResourcesAPI"
    })]

    public class TTGL_SurvivorPlugin : BaseUnityPlugin
    {
        public const string
            MODNAME = "TTGL_Survivor",
            MODAUTHOR = "Mico27",
            MODUID = "com." + MODAUTHOR + "." + MODNAME,
            MODVERSION = "1.0.0";
        // a prefix for name tokens to prevent conflicts
        public const string developerPrefix = MODAUTHOR;

        public static TTGL_SurvivorPlugin instance;

        public static event Action awake;
        public static event Action start;

        // plugin constructor, ignore this
        public TTGL_SurvivorPlugin()
        {
            awake += TTGL_SurvivorPlugin_Load;
            start += TTGL_SurvivorPlugin_LoadStart;
        }

        private void TTGL_SurvivorPlugin_Load()
        {
            instance = this;
            try
            {
                // load assets and read config
                Logger.LogMessage("PopulateAssets");
                Modules.Assets.PopulateAssets();
                Logger.LogMessage("ReadConfig");
                Modules.Config.ReadConfig();
                Logger.LogMessage("PopulateDisplays");
                Modules.ItemDisplays.PopulateDisplays(); // collect item display prefabs for use in our display rules
                Modules.Survivors.Lagann.CreateCharacter();
                Logger.LogMessage("RegisterStates");
                Modules.States.RegisterStates(); // register states(not yet implemented)
                Logger.LogMessage("RegisterBuffs");
                Modules.Buffs.RegisterBuffs(); // add and register custom buffs/debuffs
                Logger.LogMessage("RegisterProjectiles");
                Modules.Projectiles.RegisterProjectiles(); // add and register custom projectiles(not yet implemented)
                Logger.LogMessage("RegisterUnlockables");
                Modules.Unlockables.RegisterUnlockables(); // add unlockables
                Logger.LogMessage("AddTokens");
                Modules.Tokens.AddTokens(); // register name tokens
                //CreateDoppelganger(); // artifact of vengeance(not yet implemented)

            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
            
        }

        private void TTGL_SurvivorPlugin_LoadStart()
        {
            // any code you need to run in Start() goes here
        }

        public void Awake()
        {
            Action awake = TTGL_SurvivorPlugin.awake;
            if (awake == null)
            {
                return;
            }
            awake();
        }

        public void Start()
        {
            Action start = TTGL_SurvivorPlugin.start;
            if (start == null)
            {
                return;
            }
            start();
        }

        public new ManualLogSource Logger
        {
            get
            {
                return base.Logger;
            }
        }
    }
}
