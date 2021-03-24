using System;
using BepInEx;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using BepInEx.Logging;
using TTGL_Survivor.UI;
using UnityEngine;
using TTGL_Survivor.Modules;
using TTGL_Survivor.Modules.Survivors;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace TTGL_Survivor
{
    [BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]
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
            MODVERSION = "0.0.4";
        // a prefix for name tokens to prevent conflicts
        public const string developerPrefix = MODAUTHOR;
        // soft dependency 
        public static bool scepterInstalled = false;

        public static TTGL_SurvivorPlugin instance;


        // plugin constructor, ignore this
        public TTGL_SurvivorPlugin()
        {
            
        }

        public void Awake()
        {
            //On.RoR2.Networking.GameNetworkManager.OnClientConnect += (self, user, t) => { };
            instance = this;
            try
            {
                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.AncientScepter")) scepterInstalled = true;

                // load assets and read config
                Logger.LogMessage("PopulateAssets");
                Modules.Assets.PopulateAssets();
                Logger.LogMessage("ReadConfig");
                Modules.Config.ReadConfig();
                Logger.LogMessage("PopulateDisplays");
                Modules.ItemDisplays.PopulateDisplays(); // collect item display prefabs for use in our display rules
                new Lagann().CreateCharacter();
                new GurrenLagann().CreateCharacter();
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
                On.RoR2.UI.HUD.Awake += HUD_Awake;
                RoR2.UI.HUD.onHudTargetChangedGlobal += HUD_onHudTargetChangedGlobal;

            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }

        }

        private void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            CreateSpiralPowerGauge(self);
            orig(self);            
        }

        private void HUD_onHudTargetChangedGlobal(RoR2.UI.HUD obj)
        {
            if (obj && obj.targetBodyObject && m_SpiralPowerGauge)
            {
                var spiralEnergy = obj.targetBodyObject.GetComponent<SpiralEnergyComponent>();
                if (spiralEnergy)
                {
                    m_SpiralPowerGauge.gameObject.SetActive(true);
                    m_SpiralPowerGauge.source = spiralEnergy;
                }
                else
                {
                    m_SpiralPowerGauge.gameObject.SetActive(false);
                    m_SpiralPowerGauge.source = null;
                }
            }
        }
        
        #region HUD
        
        private void CreateSpiralPowerGauge(RoR2.UI.HUD hud)
        {
            if (!m_SpiralPowerGauge)
            {
                if (hud != null && hud.mainUIPanel != null)
                {
                    m_SpiralPowerGauge = hud.mainUIPanel.GetComponentInChildren<SpiralPowerGauge>();
                    if (!m_SpiralPowerGauge)
                    {
                        var spiralPowerPanel = Instantiate(Modules.Assets.mainAssetBundle.LoadAsset<GameObject>("SpiralPowerPanel"));
                        m_SpiralPowerGauge = spiralPowerPanel.AddComponent<SpiralPowerGauge>();
                        spiralPowerPanel.transform.SetParent(hud.mainUIPanel.transform);
                        var rectTransform = spiralPowerPanel.GetComponent<RectTransform>();
                        rectTransform.anchorMin = new Vector2(1, 0);
                        rectTransform.anchorMax = new Vector2(1, 0);
                        rectTransform.pivot = new Vector2(1, 0);
                        rectTransform.sizeDelta = new Vector2(120, 120);
                        rectTransform.anchoredPosition = new Vector2(-20, 200);
                        rectTransform.localScale = new Vector3(2, 2, 2);
                        spiralPowerPanel.SetActive(false);
                    }
                }
            }
        }
        
        private SpiralPowerGauge m_SpiralPowerGauge;

        #endregion

        public new ManualLogSource Logger
        {
            get
            {
                return base.Logger;
            }
        }
    }
}
