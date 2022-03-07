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
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;
using HG;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace TTGL_Survivor
{
    [BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.xoxfaby.BetterUI", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.cwmlolzlz.skills", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Mico27.RideMeExtended", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    //[BepInDependency("com.KingEnderBrine.ExtraSkillSlots", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.DrBibop.VRAPI", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]    
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(LanguageAPI), nameof(SoundAPI))]
    public class TTGL_SurvivorPlugin : BaseUnityPlugin, IContentPackProvider
    {
        public const string
            MODNAME = "TTGL_Survivor",
            MODAUTHOR = "Mico27",
            MODUID = "com." + MODAUTHOR + "." + MODNAME,
            MODVERSION = "0.4.0";
        // a prefix for name tokens to prevent conflicts
        public const string developerPrefix = MODAUTHOR;
        // soft dependency 
        public static bool scepterInstalled = false;
        public static bool betterUIInstalled = false;
        public static bool skillPlusInstalled = false;
        public static bool rideMeExtendedInstalled = false;

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
                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.xoxfaby.BetterUI")) betterUIInstalled = true;
                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.cwmlolzlz.skills")) skillPlusInstalled = true;
                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Mico27.RideMeExtended")) rideMeExtendedInstalled = true;
                Modules.Assets.PopulateAssets();
                Modules.Config.ReadConfig();
                Modules.ItemDisplays.PopulateDisplays();
                Modules.States.RegisterStates();
                Modules.Buffs.RegisterBuffs();
                Modules.Projectiles.RegisterProjectiles();
                Modules.TemporaryVisualEffects.RegisterTemporaryVisualEffects();                
                new Lagann().CreateCharacter();
                new Gurren().CreateCharacter();
                new GurrenLagann().CreateCharacter();
                Modules.CostTypeDefs.RegisterCostTypeDefs();
                Modules.Interactables.RegisterInteractables();
                Modules.Tokens.AddTokens();
                Hooks();
                if (betterUIInstalled)
                {
                    AddBetterUI();
                }
                if (skillPlusInstalled)
                {
                    AddSkillPlus();
                }
                if (rideMeExtendedInstalled)
                {
                    AddRideMeExtended();
                }
                AddVRAPI();
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message + " - " + e.StackTrace);
            }

        }

        private void LateSetup(ReadOnlyArray<ReadOnlyContentPack> obj)
        {
            Lagann.SetItemDisplays();
            Gurren.SetItemDisplays();
            GurrenLagann.SetItemDisplays();
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
            RoR2.ContentManagement.ContentManager.onContentPacksAssigned += LateSetup;
            On.RoR2.UI.HUD.Awake += HUD_Awake;
            RoR2.UI.HUD.onHudTargetChangedGlobal += HUD_onHudTargetChangedGlobal;
            On.RoR2.PickupPickerController.FixedUpdateServer += PickupPickerController_FixedUpdateServer;
            On.RoR2.GenericSkill.Start += GenericSkill_Start;
            Run.onRunStartGlobal += Run_onRunStartGlobal;
        }

        private void GenericSkill_Start(On.RoR2.GenericSkill.orig_Start orig, GenericSkill self)
        {
            orig(self);
            if (self.skillDef && !self.skillDef.fullRestockOnAssign)
            {
                self.RemoveAllStocks();
            }            
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
                if (spiralEnergy != null)
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
            if (!m_SpiralPowerGauge && TTGL_Survivor.Modules.Config.spiralGaugeEnabled)
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
                        rectTransform.anchorMin = TTGL_Survivor.Modules.Config.spiralGaugeAnchorMin;
                        rectTransform.anchorMax = TTGL_Survivor.Modules.Config.spiralGaugeAnchorMax;
                        rectTransform.pivot = TTGL_Survivor.Modules.Config.spiralGaugePivot;
                        rectTransform.sizeDelta = TTGL_Survivor.Modules.Config.spiralGaugeSizeDelta;
                        rectTransform.anchoredPosition = TTGL_Survivor.Modules.Config.spiralGaugeAnchoredPosition;
                        rectTransform.localScale = TTGL_Survivor.Modules.Config.spiralGaugeLocalScale;
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

        private static void Run_onRunStartGlobal(Run obj)
        {
            LagannCombine.playedCutSceneOnce = false;            
        }


        #endregion

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void AddBetterUI()
        {
            BetterUI.StatsDisplay.AddStatsDisplay("$spiralrate", (BetterUI.StatsDisplay.DisplayCallback)GetSpiralPowerRate);
            BetterUI.StatsDisplay.AddStatsDisplay("$spiralamount", (BetterUI.StatsDisplay.DisplayCallback)GetSpiralPowerAmount);
            BetterUI.Buffs.RegisterBuffInfo(Buffs.maxSpiralPowerBuff, developerPrefix + "_MAXSPIRALPOWER_BUFF_NAME", developerPrefix + "_MAXSPIRALPOWER_BUFF_DESCRIPTION");
            BetterUI.Buffs.RegisterBuffInfo(Buffs.maxSpiralPowerDeBuff, developerPrefix + "_MAXSPIRALPOWER_DEBUFF_NAME", developerPrefix + "_MAXSPIRALPOWER_DEBUFF_DESCRIPTION");
            BetterUI.Buffs.RegisterBuffInfo(Buffs.canopyBuff, developerPrefix + "_CANOPY_BUFF_NAME", developerPrefix + "_CANOPY_BUFF_DESCRIPTION");
            BetterUI.Buffs.RegisterBuffInfo(Buffs.kaminaBuff, developerPrefix + "_GURREN_BODY_PASSIVE_NAME", developerPrefix + "_GURREN_BODY_PASSIVE_DESCRIPTION");
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void AddSkillPlus()
        {
            //SkillsPlusPlus.SkillModifierManager.LoadSkillModifiers();            
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void AddRideMeExtended()
        {
            RideMeExtendedAddin.RideMeExtendedAddin.AddRideMeExtended();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void ExpulseAnyRider(GameObject gameObject)
        {
            RideMeExtendedAddin.RideMeExtendedAddin.ExpulseAnyRider(gameObject);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void ExitSeat(GameObject gameObject)
        {
            RideMeExtendedAddin.RideMeExtendedAddin.ExitSeat(gameObject);
        }
        
        private void AddVRAPI()
        {
            if (VRAPI.VR.enabled)
            {
                VRAPI.VR.AddVignetteState(typeof(LagannSpiralBurst));
                VRAPI.VR.AddVignetteState(typeof(LagannImpact));
                VRAPI.VR.AddVignetteState(typeof(LagannBurrowerStrike));
                VRAPI.VR.AddVignetteState(typeof(GurrenLagannTornadoKick));
            }
        }
        
        private static string GetSpiralPowerRate(CharacterBody body)
        {
            string value = null;
            var spiralEnergy = body.GetComponent<SpiralEnergyComponent>();
            if (spiralEnergy != null)
            {
                return (spiralEnergy.charge_rate * SpiralEnergyComponent.C_SPIRALENERGYCAP).ToString("0.##");
            }
            return value;
        }

        private static string GetSpiralPowerAmount(CharacterBody body)
        {
            string value = null;
            var spiralEnergy = body.GetComponent<SpiralEnergyComponent>();
            if (spiralEnergy != null)
            {
                return spiralEnergy.energy.ToString("0.##");
            }
            return value;
        }

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
            contentPack.networkedObjectPrefabs.Add(networkPrefabs.ToArray());
            contentPack.networkSoundEventDefs.Add(networkSoundEventDefs.ToArray());
            contentPack.projectilePrefabs.Add(projectilePrefabs.ToArray());
            contentPack.skillDefs.Add(skillDefs.ToArray());
            contentPack.skillFamilies.Add(skillFamilies.ToArray());
            contentPack.survivorDefs.Add(survivorDefinitions.ToArray());
            contentPack.unlockableDefs.Add(unlockableDefs.ToArray());
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
        internal static List<GameObject> networkPrefabs = new List<GameObject>();
        internal static List<GameObject> projectilePrefabs = new List<GameObject>();
        internal static List<EffectDef> effectDefs = new List<EffectDef>();
        internal static List<NetworkSoundEventDef> networkSoundEventDefs = new List<NetworkSoundEventDef>();
        internal static List<SkillFamily> skillFamilies = new List<SkillFamily>();
        internal static List<SkillDef> skillDefs = new List<SkillDef>();
        internal static List<Type> entityStates = new List<Type>();
        internal static List<UnlockableDef> unlockableDefs = new List<UnlockableDef>();

        public new ManualLogSource Logger
        {
            get
            {
                return base.Logger;
            }
        }
    }
}
