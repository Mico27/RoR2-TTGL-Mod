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
using R2API;
using RoR2.ContentManagement;
using System.Collections.Generic;
using RoR2.Skills;
using System.Collections;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace TTGL_Survivor
{
    [BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.KingEnderBrine.ExtraSkillSlots", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]    
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(LanguageAPI), nameof(SoundAPI))]
    public class TTGL_SurvivorPlugin : BaseUnityPlugin, IContentPackProvider
    {
        public const string
            MODNAME = "TTGL_Survivor",
            MODAUTHOR = "Mico27",
            MODUID = "com." + MODAUTHOR + "." + MODNAME,
            MODVERSION = "0.1.10";
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
                Modules.TemporaryVisualEffects.RegisterTemporaryVisualEffects();
                Modules.Unlockables.RegisterUnlockables();
                Modules.Tokens.AddTokens();
                new Lagann().CreateCharacter();
                new GurrenLagann().CreateCharacter();
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
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
            On.RoR2.UI.HUD.Awake += HUD_Awake;
            RoR2.UI.HUD.onHudTargetChangedGlobal += HUD_onHudTargetChangedGlobal;
            On.RoR2.PickupPickerController.FixedUpdateServer += PickupPickerController_FixedUpdateServer;
        }
        
        private void UnHooks()
        {
            ContentManager.collectContentPackProviders -= ContentManager_collectContentPackProviders;
            On.RoR2.UI.HUD.Awake -= HUD_Awake;
            RoR2.UI.HUD.onHudTargetChangedGlobal -= HUD_onHudTargetChangedGlobal;
            On.RoR2.PickupPickerController.FixedUpdateServer -= PickupPickerController_FixedUpdateServer;
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


        private void PickupPickerController_FixedUpdateServer(On.RoR2.PickupPickerController.orig_FixedUpdateServer orig, PickupPickerController self)
        {
            CharacterMaster currentParticipantMaster = self.networkUIPromptController.currentParticipantMaster;
            if (currentParticipantMaster)
            {
                CharacterBody body = currentParticipantMaster.GetBody();
                var interactor = (body)? body.GetComponent<Interactor>(): null;                
                if (!body || (body.inputBank.aimOrigin - self.transform.position).sqrMagnitude > ((interactor)? Math.Pow((interactor.maxInteractionDistance + self.cutoffDistance), 2f): (self.cutoffDistance * self.cutoffDistance)))
                {                    
                    self.networkUIPromptController.SetParticipantMaster(null);
                }
            }
        }

        #endregion
        
        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {         
            this.contentPack.identifier = this.identifier;
            contentPack.bodyPrefabs.Add(bodyPrefabs.ToArray());
            contentPack.buffDefs.Add(buffDefs.ToArray());
            contentPack.effectDefs.Add(effectDefs.ToArray());
            contentPack.entityStateTypes.Add(entityStates.ToArray());
            contentPack.masterPrefabs.Add(masterPrefabs.ToArray());
            contentPack.networkSoundEventDefs.Add(networkSoundEventDefs.ToArray());
            contentPack.projectilePrefabs.Add(projectilePrefabs.ToArray());
            contentPack.skillDefs.Add(skillDefs.ToArray());
            contentPack.skillFamilies.Add(skillFamilies.ToArray());
            contentPack.survivorDefs.Add(survivorDefinitions.ToArray());

            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(this.contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public ContentPack contentPack = new ContentPack();
        public string identifier => TTGL_SurvivorPlugin.MODUID;

        internal static List<BuffDef> buffDefs = new List<BuffDef>();
        internal static List<SurvivorDef> survivorDefinitions = new List<SurvivorDef>();
        internal static List<GameObject> bodyPrefabs = new List<GameObject>();
        internal static List<GameObject> masterPrefabs = new List<GameObject>();
        internal static List<GameObject> projectilePrefabs = new List<GameObject>();
        internal static List<EffectDef> effectDefs = new List<EffectDef>();
        internal static List<NetworkSoundEventDef> networkSoundEventDefs = new List<NetworkSoundEventDef>();
        internal static List<SkillFamily> skillFamilies = new List<SkillFamily>();
        internal static List<SkillDef> skillDefs = new List<SkillDef>();
        internal static List<Type> entityStates = new List<Type>();


        public new ManualLogSource Logger
        {
            get
            {
                return base.Logger;
            }
        }
    }
}
