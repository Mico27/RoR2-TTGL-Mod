using System;
using BepInEx;
using System.Security;
using System.Security.Permissions;
using BepInEx.Logging;
using TTGL_Survivor.UI;
using UnityEngine;
using TTGL_Survivor.Modules;
using TTGL_Survivor.Modules.Survivors;
using R2API.Utils;
using RoR2;
using TTGL_Survivor.SkillStates;

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
        "LanguageAPI",
        "SoundAPI",
    })]

    public class TTGL_SurvivorPlugin : BaseUnityPlugin
    {
        public const string
            MODNAME = "TTGL_Survivor",
            MODAUTHOR = "Mico27",
            MODUID = "com." + MODAUTHOR + "." + MODNAME,
            MODVERSION = "0.1.3";
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

                Modules.Assets.PopulateAssets();
                Modules.Config.ReadConfig();
                Modules.ItemDisplays.PopulateDisplays();
                Modules.States.RegisterStates();
                Modules.Buffs.RegisterBuffs();
                Modules.Projectiles.RegisterProjectiles();
                Modules.Unlockables.RegisterUnlockables();
                Modules.Tokens.AddTokens();
                new Lagann().CreateCharacter();
                new GurrenLagann().CreateCharacter();
                ContentPacks.CreateContentPack();
                Hooks();                
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message + " - " + e.StackTrace);
            }

        }
        public void OnDestroy()
        {
            try
            {
                UnHooks();
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message + " - " + e.StackTrace);
            }
        }

        private void Hooks()
        {
            On.RoR2.UI.HUD.Awake += HUD_Awake;
            RoR2.UI.HUD.onHudTargetChangedGlobal += HUD_onHudTargetChangedGlobal;
        }

        private void UnHooks()
        {
            On.RoR2.UI.HUD.Awake -= HUD_Awake;
            RoR2.UI.HUD.onHudTargetChangedGlobal -= HUD_onHudTargetChangedGlobal;
        }

        #region HUD
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
