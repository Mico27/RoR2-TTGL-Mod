using BepInEx.Configuration;
using EntityStates;
using ExtraSkillSlots;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TTGL_Survivor.Modules.Components;
using TTGL_Survivor.SkillStates;
using TTGL_Survivor.UI;
using UnityEngine;
using UnityEngine.Animations;

namespace TTGL_Survivor.Modules.Survivors
{
    public class Lagann: BaseSurvivor
    {
        internal static GameObject characterPrefab;
        internal static GameObject displayPrefab;

        internal static ConfigEntry<bool> characterEnabled;
        
        // item display stuffs
        internal static ItemDisplayRuleSet itemDisplayRuleSet { get; set; }
        internal static List<ItemDisplayRuleSet.KeyAssetRuleGroup> itemDisplayRules { get; set; }

        public static SkillDef canopyOverrideSkillDef;
        public static SkillDef scepterSkillDef;
        public static SkillDef shootRifleSkillDef;
        public static SkillDef explosiveRifleSkillDef;
        public static SkillDef piercingRifleSkillDef;

        public void CreateCharacter()
        {
            // this creates a config option to enable the character- feel free to remove if the character is the only thing in your mod
            //characterEnabled = Modules.Config.CharacterEnableConfig("LAGANN");

            if (true)//characterEnabled.Value)
            {
                #region Body
                characterPrefab = CreatePrefab("LagannBody", "LagannPrefab");
                //Setup spiritEnergy components
                characterPrefab.AddComponent<TTGLMusicRemote>();
                characterPrefab.AddComponent<SpiralEnergyComponent>();
                characterPrefab.AddComponent<Modules.Components.LagannController>();
                var entityStateMachine = characterPrefab.GetComponent<EntityStateMachine>();
                entityStateMachine.mainStateType = new SerializableEntityStateType(typeof(SkillStates.LagannMain));
                entityStateMachine.initialStateType = new SerializableEntityStateType(typeof(SkillStates.LagannMain));
                #endregion

                #region Model

                SetupCharacterModel(characterPrefab, new CustomRendererInfo[] {
                new CustomRendererInfo
                {
                    childName = "Lagann",
                },
                new CustomRendererInfo
                {
                    childName = "Yoko",
                },
                new CustomRendererInfo
                {
                    childName = "YokoGun",
                },
                new CustomRendererInfo
                {
                    childName = "Simon",
                },
                new CustomRendererInfo
                {
                    childName = "Kamina",
                },
                new CustomRendererInfo
                {
                    childName = "KaminaCape",
                },
                new CustomRendererInfo
                {
                    childName = "MeshWoopsYokoSkin",
                }}, 0);
                #endregion

                displayPrefab = CreateDisplayPrefab("LagannMenuPrefab", characterPrefab);

                RegisterNewSurvivor(characterPrefab, displayPrefab, new Color(0.25f, 0.65f, 0.25f), "LAGANN", null, 12.1f);

                CreateHurtBoxes();
                CreateHitboxes();
                CreateSkills();
                CreateSkins();
                InitializeItemDisplays();
                CreateGenericDoppelganger(characterPrefab, "LagannMonsterMaster", "Merc");

                if (TTGL_SurvivorPlugin.scepterInstalled) CreateScepterSkills();
            }
        }

        protected override GameObject CreateDisplayPrefab(string modelName, GameObject prefab)
        {
            GameObject newPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"), modelName);

            GameObject model = CreateModel(newPrefab, modelName);

            Transform modelBaseTransform = SetupModel(newPrefab, model.transform, true);

            model.AddComponent<CharacterModel>().baseRendererInfos = prefab.GetComponentInChildren<CharacterModel>().baseRendererInfos;

            return model.gameObject;
        }

        protected override GameObject CreatePrefab(string bodyName, string modelName)
        {
            GameObject newPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"), bodyName);

            GameObject model = CreateModel(newPrefab, modelName);
            Transform modelBaseTransform = SetupModel(newPrefab, model.transform, false);

            SetupCharacterBody(bodyName, newPrefab, modelBaseTransform);
            SetupCharacterMotor(newPrefab);
            SetupCharacterDirection(newPrefab, modelBaseTransform, model.transform);
            SetupCameraTargetParams(newPrefab);
            SetupModelLocator(newPrefab, modelBaseTransform, model.transform);
            SetupRigidbody(newPrefab);
            SetupCapsuleCollider(newPrefab);
            SetupFootstepController(model);
            SetupRagdoll(model);
            SetupAimAnimator(newPrefab, model);
            ChildLocator childLocator = model.GetComponent<ChildLocator>();
            if (childLocator)
            {
                var specialMoveCameraSource = childLocator.FindChild("SpecialMoveCameraSource");
                if (specialMoveCameraSource)
                {
                    var forcedCamera = specialMoveCameraSource.gameObject.AddComponent<SkippableCamera>();
                    forcedCamera.allowUserHud = false;
                    forcedCamera.allowUserLook = false;
                    forcedCamera.allowUserControl = false;
                    forcedCamera.entryLerpDuration = 0f;
                    forcedCamera.exitLerpDuration = 1f;
                }
                var VRCamera = childLocator.FindChild("VRCamera");
                if (VRCamera)
                {
                    if (!Config.trackVRCameraToHeadPosition)
                    {
                        var positionContraint = VRCamera.GetComponent<PositionConstraint>();
                        positionContraint.constraintActive = false;
                    }
                }
            }

            TTGL_SurvivorPlugin.bodyPrefabs.Add(newPrefab);
            return newPrefab;
        }

        protected override void SetupCharacterMotor(GameObject newPrefab)
        {
            CharacterMotor motorComponent = newPrefab.GetComponent<CharacterMotor>();
            motorComponent.mass = 100f;
        }

        protected override Transform SetupModel(GameObject prefab, Transform modelTransform, bool isDisplay)
        {
            GameObject modelBase = new GameObject("ModelBase");
            modelBase.transform.parent = prefab.transform;
            modelBase.transform.localPosition = new Vector3(0f, -1f, 0f);
            modelBase.transform.localRotation = Quaternion.identity;
            modelBase.transform.localScale = new Vector3(1f, 1f, 1f);

            GameObject cameraPivot = new GameObject("CameraPivot");
            cameraPivot.transform.parent = modelBase.transform;
            cameraPivot.transform.localPosition = new Vector3(0f, 2.6f, 0f);
            cameraPivot.transform.localRotation = Quaternion.identity;
            cameraPivot.transform.localScale = Vector3.one;

            GameObject aimOrigin = new GameObject("AimOrigin");
            aimOrigin.transform.parent = modelBase.transform;
            aimOrigin.transform.localPosition = new Vector3(0f, 2.6f, 0f);
            aimOrigin.transform.localRotation = Quaternion.identity;
            aimOrigin.transform.localScale = Vector3.one;
            prefab.GetComponent<CharacterBody>().aimOriginTransform = aimOrigin.transform;

            modelTransform.parent = modelBase.transform;
            modelTransform.localPosition = Vector3.zero;
            modelTransform.localRotation = Quaternion.identity;
            modelTransform.localScale = (isDisplay) ? new Vector3(0.8f, 0.8f, 0.8f) : new Vector3(1.0f, 1.0f, 1.0f);

            return modelBase.transform;
        }

        protected override void SetupCameraTargetParams(GameObject prefab)
        {
            CameraTargetParams cameraTargetParams = prefab.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraPivotTransform = prefab.transform.Find("ModelBase").Find("CameraPivot");
            cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.idealLocalCameraPos = Vector3.zero;
            cameraTargetParams.dontRaycastToPivot = false;
            var cameraParams = ScriptableObject.CreateInstance<CharacterCameraParams>();
            cameraParams.maxPitch = cameraTargetParams.cameraParams.maxPitch;
            cameraParams.minPitch = cameraTargetParams.cameraParams.minPitch;
            cameraParams.pivotVerticalOffset = cameraTargetParams.cameraParams.pivotVerticalOffset;
            cameraParams.standardLocalCameraPos = cameraTargetParams.cameraParams.standardLocalCameraPos * 1.2f;
            cameraParams.wallCushion = cameraTargetParams.cameraParams.wallCushion;
            cameraTargetParams.cameraParams = cameraParams;
        }

        private void CreateHurtBoxes()
        {
            GameObject model = characterPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            HurtBoxGroup hurtBoxGroup = model.AddComponent<HurtBoxGroup>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            if (!childLocator.FindChild("MainHurtbox"))
            {
                TTGL_SurvivorPlugin.instance.Logger.LogError("Could not set up main hurtbox: make sure you have a transform pair in your prefab's ChildLocator component called 'Head'");
                return;
            }

            HurtBox mainHurtbox = childLocator.FindChild("MainHurtbox").gameObject.AddComponent<HurtBox>();
            mainHurtbox.gameObject.layer = LayerIndex.entityPrecise.intVal;
            mainHurtbox.healthComponent = characterPrefab.GetComponent<HealthComponent>();
            mainHurtbox.isBullseye = true;
            mainHurtbox.damageModifier = HurtBox.DamageModifier.Normal;
            mainHurtbox.hurtBoxGroup = hurtBoxGroup;
            mainHurtbox.indexInGroup = 0;

            hurtBoxGroup.hurtBoxes = new HurtBox[]
            {
                mainHurtbox
            };

            hurtBoxGroup.mainHurtBox = mainHurtbox;
            hurtBoxGroup.bullseyeCount = 1;
        }

        private void CreateHitboxes()
        {
            GameObject model = characterPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            Transform hitboxTransform = childLocator.FindChild("DrillRushHitbox");
            SetupHitbox(model, hitboxTransform, "DrillRushHitbox");

            hitboxTransform = childLocator.FindChild("LagannImpactHitbox");
            SetupHitbox(model, hitboxTransform, "LagannImpactHitbox");

            hitboxTransform = childLocator.FindChild("SpiralBurst1Hitbox");
            SetupHitbox(model, hitboxTransform, "SpiralBurst1Hitbox");

            hitboxTransform = childLocator.FindChild("SpiralBurst2Hitbox");
            SetupHitbox(model, hitboxTransform, "SpiralBurst2Hitbox");
        }

        private void CreateSkills()
        {
            Modules.Skills.CreateSkillFamilies(characterPrefab);
            Modules.Skills.CreateFirstExtraSkillFamily(characterPrefab);

            string prefix = TTGL_SurvivorPlugin.developerPrefix;

            #region Passive

            SkillLocator skillLocator = characterPrefab.GetComponent<SkillLocator>();
            skillLocator.passiveSkill.enabled = true;
            skillLocator.passiveSkill.skillNameToken = prefix + "_LAGANN_BODY_PASSIVE_NAME";
            skillLocator.passiveSkill.skillDescriptionToken = prefix + "_LAGANN_BODY_PASSIVE_DESCRIPTION";
            skillLocator.passiveSkill.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("SpiralPowerIcon");

            #endregion

            #region Primary

            SkillDef drillRushSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            drillRushSkillDef.skillName = "LagannDrillRush";
            drillRushSkillDef.skillNameToken = prefix + "_LAGANN_BODY_PRIMARY_DRILL_NAME";
            drillRushSkillDef.skillDescriptionToken = prefix + "_LAGANN_BODY_PRIMARY_DRILL_DESCRIPTION";
            drillRushSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("DrillRushIcon");
            drillRushSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.LagannDrillRush));
            drillRushSkillDef.activationStateMachineName = "Weapon";
            drillRushSkillDef.baseMaxStock = 1;
            drillRushSkillDef.baseRechargeInterval = 0f;
            drillRushSkillDef.beginSkillCooldownOnSkillEnd = false;
            drillRushSkillDef.canceledFromSprinting = false;
            drillRushSkillDef.forceSprintDuringState = false;
            drillRushSkillDef.fullRestockOnAssign = true;
            drillRushSkillDef.interruptPriority = EntityStates.InterruptPriority.Any;
            drillRushSkillDef.isCombatSkill = true;
            drillRushSkillDef.mustKeyPress = false;
            drillRushSkillDef.cancelSprintingOnActivation = false;
            drillRushSkillDef.rechargeStock = 1;
            drillRushSkillDef.requiredStock = 0;
            drillRushSkillDef.stockToConsume = 0;
            drillRushSkillDef.keywordTokens = new string[] { "KEYWORD_AGILE" };
            TTGL_SurvivorPlugin.skillDefs.Add(drillRushSkillDef);
            Modules.Skills.AddPrimarySkill(characterPrefab, drillRushSkillDef);

            SkillDef drillSpikeSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            drillSpikeSkillDef.skillName = "LagannDrillSpike";
            drillSpikeSkillDef.skillNameToken = prefix + "_LAGANN_BODY_PRIMARY_DRILLSPIKE_NAME";
            drillSpikeSkillDef.skillDescriptionToken = prefix + "_LAGANN_BODY_PRIMARY_DRILLSPIKE_DESCRIPTION";
            drillSpikeSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("DrillSpikeIcon");
            drillSpikeSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.LagannDrillSpike));
            drillSpikeSkillDef.activationStateMachineName = "Weapon";
            drillSpikeSkillDef.baseMaxStock = 1;
            drillSpikeSkillDef.baseRechargeInterval = 0f;
            drillSpikeSkillDef.beginSkillCooldownOnSkillEnd = false;
            drillSpikeSkillDef.canceledFromSprinting = false;
            drillSpikeSkillDef.forceSprintDuringState = false;
            drillSpikeSkillDef.fullRestockOnAssign = true;
            drillSpikeSkillDef.interruptPriority = EntityStates.InterruptPriority.Any;
            drillSpikeSkillDef.isCombatSkill = true;
            drillSpikeSkillDef.mustKeyPress = false;
            drillSpikeSkillDef.cancelSprintingOnActivation = true;
            drillSpikeSkillDef.rechargeStock = 1;
            drillSpikeSkillDef.requiredStock = 0;
            drillSpikeSkillDef.stockToConsume = 0;
            TTGL_SurvivorPlugin.skillDefs.Add(drillSpikeSkillDef);
            Modules.Skills.AddPrimarySkill(characterPrefab, drillSpikeSkillDef);

            #endregion

            #region Secondary
            SkillStates.YokoShootRifle.maxRicochetCount = (TTGL_SurvivorPlugin.skillPlusInstalled) ? 2 : 6;
            SkillStates.YokoShootRifle.resetBouncedObjects = (TTGL_SurvivorPlugin.skillPlusInstalled) ? false : true;
            shootRifleSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            shootRifleSkillDef.skillName = "YokoShootRifle";
            shootRifleSkillDef.skillNameToken = prefix + "_LAGANN_BODY_SECONDARY_RIFLE_NAME";
            shootRifleSkillDef.skillDescriptionToken = prefix + "_LAGANN_BODY_SECONDARY_RIFLE_DESCRIPTION";
            shootRifleSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("YokoRifleIcon");
            shootRifleSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.YokoShootRifle));
            shootRifleSkillDef.activationStateMachineName = "Weapon";
            shootRifleSkillDef.baseMaxStock = 1;
            shootRifleSkillDef.baseRechargeInterval = 1f;
            shootRifleSkillDef.beginSkillCooldownOnSkillEnd = false;
            shootRifleSkillDef.canceledFromSprinting = false;
            shootRifleSkillDef.forceSprintDuringState = false;
            shootRifleSkillDef.fullRestockOnAssign = true;
            shootRifleSkillDef.interruptPriority = EntityStates.InterruptPriority.Skill;
            shootRifleSkillDef.isCombatSkill = true;
            shootRifleSkillDef.mustKeyPress = false;
            shootRifleSkillDef.cancelSprintingOnActivation = false;
            shootRifleSkillDef.rechargeStock = 1;
            shootRifleSkillDef.requiredStock = 1;
            shootRifleSkillDef.stockToConsume = 1;
            shootRifleSkillDef.keywordTokens = new string[] { "KEYWORD_AGILE" };
            TTGL_SurvivorPlugin.skillDefs.Add(shootRifleSkillDef);
            Modules.Skills.AddSecondarySkill(characterPrefab, shootRifleSkillDef);

            explosiveRifleSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            explosiveRifleSkillDef.skillName = "YokoExplosiveRifle";
            explosiveRifleSkillDef.skillNameToken = prefix + "_LAGANN_BODY_SECONDARY_EXPLOSION_NAME";
            explosiveRifleSkillDef.skillDescriptionToken = prefix + "_LAGANN_BODY_SECONDARY_EXPLOSION_DESCRIPTION";
            explosiveRifleSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("YokoRifleExplosionIcon");
            explosiveRifleSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.YokoExplosiveRifle));
            explosiveRifleSkillDef.activationStateMachineName = "Weapon";
            explosiveRifleSkillDef.baseMaxStock = 1;
            explosiveRifleSkillDef.baseRechargeInterval = 2f;
            explosiveRifleSkillDef.beginSkillCooldownOnSkillEnd = false;
            explosiveRifleSkillDef.canceledFromSprinting = false;
            explosiveRifleSkillDef.forceSprintDuringState = false;
            explosiveRifleSkillDef.fullRestockOnAssign = true;
            explosiveRifleSkillDef.interruptPriority = EntityStates.InterruptPriority.Skill;
            explosiveRifleSkillDef.isCombatSkill = true;
            explosiveRifleSkillDef.mustKeyPress = false;
            explosiveRifleSkillDef.cancelSprintingOnActivation = true;
            explosiveRifleSkillDef.rechargeStock = 1;
            explosiveRifleSkillDef.requiredStock = 1;
            explosiveRifleSkillDef.stockToConsume = 1;
            TTGL_SurvivorPlugin.skillDefs.Add(explosiveRifleSkillDef);
            Modules.Skills.AddSecondarySkill(characterPrefab, explosiveRifleSkillDef);
            
            piercingRifleSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            piercingRifleSkillDef.skillName = "YokoPiercingRifle";
            piercingRifleSkillDef.skillNameToken = prefix + "_LAGANN_BODY_SECONDARY_PIERCING_NAME";
            piercingRifleSkillDef.skillDescriptionToken = prefix + "_LAGANN_BODY_SECONDARY_PIERCING_DESCRIPTION";
            piercingRifleSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("YokoRifleChargeIcon");
            piercingRifleSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.YokoPiercingRifleCharging));
            piercingRifleSkillDef.activationStateMachineName = "Weapon";
            piercingRifleSkillDef.baseMaxStock = 3;
            piercingRifleSkillDef.baseRechargeInterval = 2f;
            piercingRifleSkillDef.beginSkillCooldownOnSkillEnd = true;
            piercingRifleSkillDef.canceledFromSprinting = false;
            piercingRifleSkillDef.forceSprintDuringState = false;
            piercingRifleSkillDef.fullRestockOnAssign = true;
            piercingRifleSkillDef.interruptPriority = EntityStates.InterruptPriority.Skill;
            piercingRifleSkillDef.isCombatSkill = true;
            piercingRifleSkillDef.mustKeyPress = true;
            piercingRifleSkillDef.cancelSprintingOnActivation = false;
            piercingRifleSkillDef.rechargeStock = 1;
            piercingRifleSkillDef.requiredStock = 1;
            piercingRifleSkillDef.stockToConsume = 1;
            piercingRifleSkillDef.keywordTokens = new string[] { "KEYWORD_AGILE" };
            TTGL_SurvivorPlugin.skillDefs.Add(piercingRifleSkillDef);
            Modules.Skills.AddSecondarySkill(characterPrefab, piercingRifleSkillDef);
            #endregion

            #region Utility
            SkillDef spiralBurstSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            spiralBurstSkillDef.skillName = "LagannSpiralBurst";
            spiralBurstSkillDef.skillNameToken = prefix + "_LAGANN_BODY_UTILITY_SPIRALBURST_NAME";
            spiralBurstSkillDef.skillDescriptionToken = prefix + "_LAGANN_BODY_UTILITY_SPIRALBURST_DESCRIPTION";
            spiralBurstSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("SpiralBurstIcon");
            spiralBurstSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.LagannSpiralBurst));
            spiralBurstSkillDef.activationStateMachineName = "Weapon";
            spiralBurstSkillDef.baseMaxStock = 1;
            spiralBurstSkillDef.baseRechargeInterval = 4f;
            spiralBurstSkillDef.beginSkillCooldownOnSkillEnd = true;
            spiralBurstSkillDef.canceledFromSprinting = false;
            spiralBurstSkillDef.forceSprintDuringState = true;
            spiralBurstSkillDef.fullRestockOnAssign = true;
            spiralBurstSkillDef.interruptPriority = EntityStates.InterruptPriority.PrioritySkill;
            spiralBurstSkillDef.isCombatSkill = true;
            spiralBurstSkillDef.mustKeyPress = true;
            spiralBurstSkillDef.cancelSprintingOnActivation = false;
            spiralBurstSkillDef.rechargeStock = 1;
            spiralBurstSkillDef.requiredStock = 1;
            spiralBurstSkillDef.stockToConsume = 1;
            TTGL_SurvivorPlugin.skillDefs.Add(spiralBurstSkillDef);
            Modules.Skills.AddUtilitySkill(characterPrefab, spiralBurstSkillDef);

            SkillDef toggleCanopySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            toggleCanopySkillDef.skillName = "LagannToggleCanopy";
            toggleCanopySkillDef.skillNameToken = prefix + "_LAGANN_BODY_UTILITY_TOGGLECANOPY_NAME";
            toggleCanopySkillDef.skillDescriptionToken = prefix + "_LAGANN_BODY_UTILITY_TOGGLECANOPY_DESCRIPTION";
            toggleCanopySkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("ToggleCanopyIcon");
            toggleCanopySkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.LagannToggleCanopy));
            toggleCanopySkillDef.activationStateMachineName = "Weapon";
            toggleCanopySkillDef.baseMaxStock = 1;
            toggleCanopySkillDef.baseRechargeInterval = 1f;
            toggleCanopySkillDef.beginSkillCooldownOnSkillEnd = false;
            toggleCanopySkillDef.canceledFromSprinting = false;
            toggleCanopySkillDef.forceSprintDuringState = true;
            toggleCanopySkillDef.fullRestockOnAssign = true;
            toggleCanopySkillDef.interruptPriority = EntityStates.InterruptPriority.PrioritySkill;
            toggleCanopySkillDef.isCombatSkill = false;
            toggleCanopySkillDef.mustKeyPress = true;
            toggleCanopySkillDef.cancelSprintingOnActivation = false;
            toggleCanopySkillDef.rechargeStock = 1;
            toggleCanopySkillDef.requiredStock = 1;
            toggleCanopySkillDef.stockToConsume = 1;
            TTGL_SurvivorPlugin.skillDefs.Add(toggleCanopySkillDef);
            Modules.Skills.AddUtilitySkill(characterPrefab, toggleCanopySkillDef);

            canopyOverrideSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            canopyOverrideSkillDef.skillName = prefix + "LagannDisableRifle";
            canopyOverrideSkillDef.skillNameToken = prefix + "_LAGANN_BODY_UTILITY_DISABLERIFLE_NAME";
            canopyOverrideSkillDef.skillDescriptionToken = prefix + "_LAGANN_BODY_UTILITY_DISABLERIFLE_DESCRIPTION";
            canopyOverrideSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("DisableYokoRifleIcon");
            TTGL_SurvivorPlugin.skillDefs.Add(canopyOverrideSkillDef);

            ShieldBarrirerSkillDef spiralConversionSkillDef = ScriptableObject.CreateInstance<ShieldBarrirerSkillDef>();
            spiralConversionSkillDef.skillName = "LagannSpiralConversion";
            spiralConversionSkillDef.skillNameToken = prefix + "_LAGANN_BODY_UTILITY_SPIRALCONVERSION_NAME";
            spiralConversionSkillDef.skillDescriptionToken = prefix + "_LAGANN_BODY_UTILITY_SPIRALCONVERSION_DESCRIPTION";
            spiralConversionSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("SpiralConversionIcon");
            spiralConversionSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.LagannSpiralConversion));
            spiralConversionSkillDef.activationStateMachineName = "Body";
            spiralConversionSkillDef.baseMaxStock = 1;
            spiralConversionSkillDef.baseRechargeInterval = 8f;
            spiralConversionSkillDef.beginSkillCooldownOnSkillEnd = true;
            spiralConversionSkillDef.canceledFromSprinting = false;
            spiralConversionSkillDef.forceSprintDuringState = false;
            spiralConversionSkillDef.fullRestockOnAssign = true;
            spiralConversionSkillDef.interruptPriority = EntityStates.InterruptPriority.Skill;
            spiralConversionSkillDef.isCombatSkill = false;
            spiralConversionSkillDef.mustKeyPress = true;
            spiralConversionSkillDef.cancelSprintingOnActivation = false;
            spiralConversionSkillDef.rechargeStock = 1;
            spiralConversionSkillDef.requiredStock = 1;
            spiralConversionSkillDef.stockToConsume = 1;
            TTGL_SurvivorPlugin.skillDefs.Add(spiralConversionSkillDef);
            Modules.Skills.AddUtilitySkill(characterPrefab, spiralConversionSkillDef);
            #endregion

            #region Special
            AimLagannImpact.maxRebound = (TTGL_SurvivorPlugin.skillPlusInstalled) ? 2 : 4;
            SkillDef lagannImpactSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            lagannImpactSkillDef.skillName = "LagannImpact";
            lagannImpactSkillDef.skillNameToken = prefix + "_LAGANN_BODY_SPECIAL_IMPACT_NAME";
            lagannImpactSkillDef.skillDescriptionToken = prefix + "_LAGANN_BODY_SPECIAL_IMPACT_DESCRIPTION";
            lagannImpactSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("LagannImpactIcon");
            lagannImpactSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.PrepareLagannImpact));
            lagannImpactSkillDef.activationStateMachineName = "Body";
            lagannImpactSkillDef.baseMaxStock = 1;
            lagannImpactSkillDef.baseRechargeInterval = 8f;
            lagannImpactSkillDef.beginSkillCooldownOnSkillEnd = true;
            lagannImpactSkillDef.canceledFromSprinting = false;
            lagannImpactSkillDef.forceSprintDuringState = false;
            lagannImpactSkillDef.fullRestockOnAssign = true;
            lagannImpactSkillDef.interruptPriority = EntityStates.InterruptPriority.Skill;
            lagannImpactSkillDef.isCombatSkill = true;
            lagannImpactSkillDef.mustKeyPress = false;
            lagannImpactSkillDef.cancelSprintingOnActivation = true;
            lagannImpactSkillDef.rechargeStock = 1;
            lagannImpactSkillDef.requiredStock = 1;
            lagannImpactSkillDef.stockToConsume = 1;
            TTGL_SurvivorPlugin.skillDefs.Add(lagannImpactSkillDef);
            Modules.Skills.AddSpecialSkill(characterPrefab, lagannImpactSkillDef);

            SkillDef lagannBurrowerStrikeSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            lagannBurrowerStrikeSkillDef.skillName = "LagannBurrowerStrike";
            lagannBurrowerStrikeSkillDef.skillNameToken = prefix + "_LAGANN_BODY_SPECIAL_BURROWER_NAME";
            lagannBurrowerStrikeSkillDef.skillDescriptionToken = prefix + "_LAGANN_BODY_SPECIAL_BURROWER_DESCRIPTION";
            lagannBurrowerStrikeSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("LagannImpactIcon");
            lagannBurrowerStrikeSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.PrepareLagannBurrowerStrike));
            lagannBurrowerStrikeSkillDef.activationStateMachineName = "Body";
            lagannBurrowerStrikeSkillDef.baseMaxStock = 1;
            lagannBurrowerStrikeSkillDef.baseRechargeInterval = 12f;
            lagannBurrowerStrikeSkillDef.beginSkillCooldownOnSkillEnd = true;
            lagannBurrowerStrikeSkillDef.canceledFromSprinting = false;
            lagannBurrowerStrikeSkillDef.forceSprintDuringState = false;
            lagannBurrowerStrikeSkillDef.fullRestockOnAssign = true;
            lagannBurrowerStrikeSkillDef.interruptPriority = EntityStates.InterruptPriority.Skill;
            lagannBurrowerStrikeSkillDef.isCombatSkill = true;
            lagannBurrowerStrikeSkillDef.mustKeyPress = false;
            lagannBurrowerStrikeSkillDef.cancelSprintingOnActivation = true;
            lagannBurrowerStrikeSkillDef.rechargeStock = 1;
            lagannBurrowerStrikeSkillDef.requiredStock = 1;
            lagannBurrowerStrikeSkillDef.stockToConsume = 1;
            TTGL_SurvivorPlugin.skillDefs.Add(lagannBurrowerStrikeSkillDef);
            Modules.Skills.AddSpecialSkill(characterPrefab, lagannBurrowerStrikeSkillDef);

            #endregion
            #region FirstExtra
            LagannCombineSkillDef lagannCombineSkillDef = ScriptableObject.CreateInstance<LagannCombineSkillDef>();
            lagannCombineSkillDef.energyCost = LagannCombine.energyCost;
            lagannCombineSkillDef.skillName = "LagannCombine";
            lagannCombineSkillDef.skillNameToken = prefix + "_LAGANN_BODY_COMBINE_NAME";
            lagannCombineSkillDef.skillDescriptionToken = prefix + "_LAGANN_BODY_COMBINE_DESCRIPTION";
            lagannCombineSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("LagannCombineIcon");
            lagannCombineSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(LagannCombine));
            lagannCombineSkillDef.activationStateMachineName = "Body";
            lagannCombineSkillDef.baseMaxStock = 1;
            lagannCombineSkillDef.baseRechargeInterval = 15f;
            lagannCombineSkillDef.beginSkillCooldownOnSkillEnd = true;
            lagannCombineSkillDef.canceledFromSprinting = false;
            lagannCombineSkillDef.forceSprintDuringState = false;
            lagannCombineSkillDef.fullRestockOnAssign = false;
            lagannCombineSkillDef.interruptPriority = EntityStates.InterruptPriority.PrioritySkill;
            lagannCombineSkillDef.isCombatSkill = true;
            lagannCombineSkillDef.mustKeyPress = true;
            lagannCombineSkillDef.cancelSprintingOnActivation = true;
            lagannCombineSkillDef.rechargeStock = 1;
            lagannCombineSkillDef.requiredStock = 1;
            lagannCombineSkillDef.stockToConsume = 1;
            TTGL_SurvivorPlugin.skillDefs.Add(lagannCombineSkillDef);
            Modules.Skills.AddFirstExtraSkill(characterPrefab, lagannCombineSkillDef);

            #endregion
        }

        private void CreateSkins()
        {
            GameObject model = characterPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();
            GameObject yoko = childLocator.FindChild("Yoko").gameObject;
            GameObject meshWoopsYokoSkin = childLocator.FindChild("MeshWoopsYokoSkin").gameObject;

            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;

            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            SkinDef defaultSkin = Modules.Skins.CreateSkinDef(TTGL_SurvivorPlugin.developerPrefix + "_LAGANN_BODY_DEFAULT_SKIN_NAME",
                Assets.mainAssetBundle.LoadAsset<Sprite>("DefaultSkinIcon"),
                defaultRenderers,
                mainRenderer,
                model);

            defaultSkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            {
                new SkinDef.GameObjectActivation
                {
                    gameObject = yoko,
                    shouldActivate = true
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = meshWoopsYokoSkin,
                    shouldActivate = false
                }
            };

            skins.Add(defaultSkin);
            #endregion

            #region WoopsSkin
            var woopsEnabled = Modules.Config.woopsEnabled;
            if (woopsEnabled)
            {
                SkinDef woopsSkin = Modules.Skins.CreateSkinDef(TTGL_SurvivorPlugin.developerPrefix + "_LAGANN_BODY_WOOPS_SKIN_NAME",
                Assets.mainAssetBundle.LoadAsset<Sprite>("WoopsSkinIcon"),
                defaultRenderers,
                mainRenderer,
                model);

                woopsSkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
                {
                new SkinDef.GameObjectActivation
                {
                    gameObject = meshWoopsYokoSkin,
                    shouldActivate = true
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = yoko,
                    shouldActivate = false
                }
                };

                skins.Add(woopsSkin);
            }
            #endregion
            
            skinController.skins = skins.ToArray();
        }

        internal virtual void InitializeItemDisplays()
        {
            GameObject model = characterPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            itemDisplayRuleSet = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
            itemDisplayRuleSet.name = "idrs" + "Lagann";

            characterModel.itemDisplayRuleSet = itemDisplayRuleSet;
        }

        internal static void SetItemDisplays()
        {
            itemDisplayRules = new List<ItemDisplayRuleSet.KeyAssetRuleGroup>();

            // add item displays here
            //  HIGHLY recommend using KingEnderBrine's ItemDisplayPlacementHelper mod for this
            #region Item Displays

            if (Config.lagannItemDisplayEnabled)
            {
                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Jetpack,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBugWings"),
                            childName = "Head",
localPos = new Vector3(0.0009F, 0.78585F, -0.86052F),
localAngles = new Vector3(21.4993F, 358.6616F, 358.3334F),
localScale = new Vector3(0.3F, 0.3F, 0.3F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.GoldGat,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGoldGat"),
childName = "Right_Lower_Arm",
localPos = new Vector3(0.01809F, 0.47797F, 0.01139F),
localAngles = new Vector3(355.804F, 357.838F, 227.4261F),
localScale = new Vector3(0.3F, 0.3F, 0.3F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.BFG,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBFG"),
childName = "Right_Upper_Arm",
localPos = new Vector3(0.16431F, 0.47524F, -0.1255F),
localAngles = new Vector3(6.58403F, 269.4264F, 89.93426F),
localScale = new Vector3(0.3F, 0.3F, 0.3F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.CritGlasses,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGlasses"),
childName = "Simon_Head",
localPos = new Vector3(0F, 0.09668F, 0.14352F),
localAngles = new Vector3(3.21891F, 359.2669F, 178.3015F),
localScale = new Vector3(0.25F, 0.25F, 0.25F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Syringe,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySyringeCluster"),
childName = "Kamina_UpperArmL",
localPos = new Vector3(0.07267F, 0.0533F, 0.02118F),
localAngles = new Vector3(57.33885F, 133.3951F, 90.26039F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Behemoth,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBehemoth"),
childName = "Left_Lower_Arm",
localPos = new Vector3(-0.05441F, 0.62723F, 0.38047F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Missile,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMissileLauncher"),
childName = "Left_Upper_Arm",
localPos = new Vector3(0.82838F, 0.4998F, 0.00001F),
localAngles = new Vector3(0F, 0F, 270F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Dagger,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDagger"),
childName = "Kamina_Chest",
localPos = new Vector3(-0.04703F, 0.25852F, -0.11102F),
localAngles = new Vector3(330.7535F, 359.1341F, 38.14787F),
localScale = new Vector3(1.2428F, 1.2428F, 1.2299F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Hoof,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayHoof"),
childName = "Simon_RightLowerLeg",
localPos = new Vector3(-0.03149F, 0.21073F, 0.11144F),
localAngles = new Vector3(65.16771F, 171.0145F, 355.5159F),
localScale = new Vector3(0.1F, 0.1F, 0.075F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayHoof"),
childName = "Simon_LeftLowerLeg",
localPos = new Vector3(-0.03149F, 0.21073F, 0.11144F),
localAngles = new Vector3(65.16771F, 171.0145F, 355.5159F),
localScale = new Vector3(0.1F, 0.1F, 0.075F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ChainLightning,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayUkulele"),
childName = "Kamina_Root",
localPos = new Vector3(-0.18584F, 1.73703F, 0.3224F),
localAngles = new Vector3(355.3529F, 349.4957F, 53.11215F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.GhostOnKill,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMask"),
childName = "Yoko_Head",
localPos = new Vector3(0.14259F, 0.42479F, -0.09915F),
localAngles = new Vector3(317.7824F, 111.5255F, 355.0938F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Mushroom,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMushroom"),
childName = "Kamina_Head",
localPos = new Vector3(-0.00023F, 0.26227F, -0.00421F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.0501F, 0.0501F, 0.0501F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.AttackSpeedOnCrit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWolfPelt"),
childName = "Simon_Head",
localPos = new Vector3(0F, 0.13582F, 0.0828F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.6F, 0.6F, 0.6F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.BleedOnHit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTriTip"),
childName = "Right_Lower_Arm",
localPos = new Vector3(-0.11817F, 0.6162F, -0.03188F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.WardOnLevel,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWarbanner"),
childName = "Head",
localPos = new Vector3(0.00133F, 1.22243F, -1.23424F),
localAngles = new Vector3(0F, 0F, 90F),
localScale = new Vector3(0.3162F, 0.3162F, 0.3162F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.HealOnCrit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayScythe"),
childName = "Yoko_UpperBody2",
localPos = new Vector3(0.25231F, -0.09855F, -0.28756F),
localAngles = new Vector3(52.41183F, 103.6588F, 243.8694F),
localScale = new Vector3(0.7F, 0.7F, 0.7F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.HealWhileSafe,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySnail"),
childName = "Simon_Chest",
localPos = new Vector3(-0.13704F, 0.23329F, -0.07994F),
localAngles = new Vector3(81.14896F, 80.74899F, 99.25114F),
localScale = new Vector3(0.0289F, 0.0289F, 0.0289F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Clover,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayClover"),
childName = "Yoko_Head",
localPos = new Vector3(-0.22744F, 0.32779F, 0.01122F),
localAngles = new Vector3(9.65349F, 311.5749F, 29.68526F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.BarrierOnOverHeal,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAegis"),
childName = "Right_Upper_Arm",
localPos = new Vector3(0F, 0.60912F, -0.13631F),
localAngles = new Vector3(5.63827F, 179.8669F, 180.1904F),
localScale = new Vector3(0.7F, 0.7F, 0.7F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.GoldOnHit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBoneCrown"),
childName = "Kamina_Head",
localPos = new Vector3(-0.00013F, 0.15626F, -0.00251F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.WarCryOnMultiKill,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayPauldron"),
childName = "Right_Upper_Leg",
localPos = new Vector3(0.10075F, 0.5746F, 0.02378F),
localAngles = new Vector3(331.2744F, 265.8005F, 0.06988F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.SprintArmor,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBuckler"),
childName = "Head",
localPos = new Vector3(0.45344F, 0.928F, 0.92888F),
localAngles = new Vector3(0.00001F, 24.60378F, 0.00001F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.IceRing,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayIceRing"),
childName = "Left_Upper_Arm",
localPos = new Vector3(0F, 0.29392F, 0F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1.5F, 1.5F, 1.5F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.FireRing,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFireRing"),
childName = "Right_Upper_Arm",
localPos = new Vector3(0F, 0.29392F, 0F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1.5F, 1.5F, 1.5F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.UtilitySkillMagazine,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAfterburnerShoulderRing"),
                            childName = "FootR",
localPos = new Vector3(0F, 0.17609F, 0F),
localAngles = new Vector3(0F, 180F, 180F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAfterburnerShoulderRing"),
                            childName = "FootL",
localPos = new Vector3(0F, 0.17609F, 0F),
localAngles = new Vector3(0F, 180F, 180F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.JumpBoost,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWaxBird"),
childName = "Kamina_LowerLegR",
localPos = new Vector3(0.11118F, 0.82598F, 0.1853F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(0.5253F, 0.5253F, 0.5253F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ArmorReductionOnHit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWarhammer"),
childName = "Yoko_UpperBody2",
localPos = new Vector3(0.22928F, -0.19927F, -0.17298F),
localAngles = new Vector3(48.65155F, 90F, 90F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.NearbyDamageBonus,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDiamond"),
childName = "Head",
localPos = new Vector3(-0.09185F, 0.54047F, 0.2634F),
localAngles = new Vector3(319.0285F, 152.6564F, 31.07953F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ArmorPlate,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayRepulsionArmorPlate"),
childName = "Left_Upper_Arm",
localPos = new Vector3(-0.08609F, 0.62803F, -0.11225F),
localAngles = new Vector3(355.5205F, 90.00003F, 180F),
localScale = new Vector3(1.2F, 1.2F, 1.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.CommandMissile,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMissileRack"),
childName = "Right_Upper_Arm",
localPos = new Vector3(-0.00002F, 0.28864F, -0.44707F),
localAngles = new Vector3(2.42924F, 115.6223F, 268.7745F),
localScale = new Vector3(0.75F, 0.75F, 0.75F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Feather,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFeather"),
childName = "Kamina_Chest",
localPos = new Vector3(-0.1976F, 0.17209F, 0.12766F),
localAngles = new Vector3(12.10463F, 5.07036F, 35.93486F),
localScale = new Vector3(0.02F, 0.02F, 0.02F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Crowbar,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayCrowbar"),
childName = "Yoko_UpperBody2",
localPos = new Vector3(0.00946F, 0.11053F, -0.16196F),
localAngles = new Vector3(85.95184F, 91.92277F, 343.7595F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.FallBoots,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGravBoots"),
childName = "FootR",
localPos = new Vector3(-0.0038F, 0.3729F, -0.0046F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.6F, 0.5F, 0.4F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGravBoots"),
childName = "FootL",
localPos = new Vector3(-0.0038F, 0.3729F, -0.0046F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.6F, 0.5F, 0.4F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ExecuteLowHealthElite,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGuillotine"),
childName = "Left_Lower_Arm",
localPos = new Vector3(0.17952F, 0.6099F, -0.0107F),
localAngles = new Vector3(90F, 261F, 0F),
localScale = new Vector3(0.4F, 0.4F, 0.4F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.EquipmentMagazine,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBattery"),
childName = "Head",
localPos = new Vector3(0F, 0.36004F, -0.66817F),
localAngles = new Vector3(270F, 180F, 0F),
localScale = new Vector3(1.7F, 1.7F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.NovaOnHeal,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDevilHorns"),
childName = "Kamina_Head",
localPos = new Vector3(0.09857F, 0.19257F, 0.01395F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.5349F, 0.5349F, 0.5349F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDevilHorns"),
childName = "Kamina_Head",
localPos = new Vector3(-0.09857F, 0.19257F, 0.01395F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(-0.5349F, 0.5349F, 0.5349F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Infusion,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayInfusion"),
childName = "Yoko_Leg_L",
localPos = new Vector3(0.08336F, 0.42197F, -0.17609F),
localAngles = new Vector3(2.69442F, 339.7525F, 181.5413F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Medkit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMedkit"),
childName = "Head",
localPos = new Vector3(0.07143F, 0.48659F, 0.49166F),
localAngles = new Vector3(0F, 159.6547F, 180F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Bandolier,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBandolier"),
childName = "Yoko_Root",
localPos = new Vector3(0.04622F, 2.57112F, 0.07727F),
localAngles = new Vector3(307.7859F, 183.1415F, 164.5463F),
localScale = new Vector3(0.7F, 0.7F, 0.7F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.BounceNearby,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayHook"),
childName = "Head",
localPos = new Vector3(0F, 1.35409F, -0.60059F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.IgniteOnKill,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGasoline"),
childName = "Head",
localPos = new Vector3(0.41712F, 0.6231F, -0.14811F),
localAngles = new Vector3(270F, 17.04718F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.StunChanceOnHit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayStunGrenade"),
childName = "Muzzle",
localPos = new Vector3(-0.2401F, -2.46996F, -0.51588F),
localAngles = new Vector3(330.9445F, 248.489F, 145.1801F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Firework,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFirework"),
childName = "Head",
localPos = new Vector3(0.0086F, 1.382F, -0.81788F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.LunarDagger,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayLunarDagger"),
childName = "Yoko_Knee_R",
localPos = new Vector3(0.11658F, 0.65113F, 0.14825F),
localAngles = new Vector3(47.88065F, 7.78883F, 103.6532F),
localScale = new Vector3(0.8F, 0.8F, 0.8F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Knurl,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayKnurl"),
childName = "Head",
localPos = new Vector3(-0.0186F, 1.53638F, 0.03632F),
localAngles = new Vector3(78.87074F, 36.6722F, 105.8275F),
localScale = new Vector3(0.01F, 0.01F, 0.01F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.BeetleGland,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBeetleGland"),
childName = "Yoko_Root",
localPos = new Vector3(0.34252F, 2.06018F, -0.15773F),
localAngles = new Vector3(14.39303F, 48.01755F, 336.6603F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.SprintBonus,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySoda"),
childName = "Head",
localPos = new Vector3(-0.47433F, 0.55618F, 0.22349F),
localAngles = new Vector3(270F, 251.0168F, 0F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.SecondarySkillMagazine,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDoubleMag"),
childName = "Muzzle",
localPos = new Vector3(-0.29287F, -2.8445F, -0.57662F),
localAngles = new Vector3(279.9269F, 356.6822F, 85.3522F),
localScale = new Vector3(0.15F, 0.15F, 0.15F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.StickyBomb,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayStickyBomb"),
childName = "Head",
localPos = new Vector3(0.24909F, 0.79484F, 0.02327F),
localAngles = new Vector3(8.4958F, 176.5473F, 162.7601F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.TreasureCache,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayKey"),
childName = "Kamina_Chest",
localPos = new Vector3(-0.07521F, 0.13128F, 0.13932F),
localAngles = new Vector3(349.8004F, 270.245F, 80.19169F),
localScale = new Vector3(0.75F, 0.75F, 0.75F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.BossDamageBonus,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAPRound"),
childName = "Yoko_Leg_R",
localPos = new Vector3(0.18793F, 0.31562F, -0.08979F),
localAngles = new Vector3(85.83981F, 265.0102F, 177.9674F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.SlowOnHit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBauble"),
childName = "Head",
localPos = new Vector3(0.1995F, 0.36513F, 0.08005F),
localAngles = new Vector3(0.00003F, 39.82773F, 22.2349F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ExtraLife,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayHippo"),
childName = "Head",
localPos = new Vector3(0.17651F, 0.71958F, 0.23402F),
localAngles = new Vector3(342.8309F, 309.3666F, 0.00001F),
localScale = new Vector3(0.2645F, 0.2645F, 0.2645F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.KillEliteFrenzy,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBrainstalk"),
childName = "Kamina_Head",
localPos = new Vector3(-0.00017F, 0.19365F, -0.00311F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.2638F, 0.2638F, 0.2638F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.RepeatHeal,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayCorpseFlower"),
childName = "Yoko_Head",
localPos = new Vector3(-0.18847F, 0.47235F, 0.00713F),
localAngles = new Vector3(312.5705F, 91.83267F, 13.78203F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.AutoCastEquipment,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFossil"),
childName = "Head",
localPos = new Vector3(-0.00894F, 0.96457F, 1.08091F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(0.95F, 0.95F, 0.95F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.IncreaseHealing,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAntler"),
childName = "Simon_Head",
localPos = new Vector3(0.10305F, 0.2245F, 0.05445F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAntler"),
childName = "Simon_Head",
localPos = new Vector3(-0.10305F, 0.2245F, 0.05445F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(0.2F, 0.2F, -0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.TitanGoldDuringTP,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGoldHeart"),
childName = "Head",
localPos = new Vector3(-0.35908F, 0.95858F, 1.00107F),
localAngles = new Vector3(13.08055F, 296.3911F, 19.3483F),
localScale = new Vector3(0.3F, 0.3F, 0.3F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.SprintWisp,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBrokenMask"),
childName = "Yoko_Head",
localPos = new Vector3(-0.21922F, 0.34712F, -0.13784F),
localAngles = new Vector3(347.1928F, 272.9277F, 356.7859F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.BarrierOnKill,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBrooch"),
childName = "Head",
localPos = new Vector3(0F, 0.94155F, 1.1148F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.TPHealingNova,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGlowFlower"),
childName = "Head",
localPos = new Vector3(0F, 0.97264F, 1.05076F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.LunarUtilityReplacement,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBirdFoot"),
childName = "Head",
localPos = new Vector3(-0.61989F, 0.63863F, 0.86161F),
localAngles = new Vector3(3.67517F, 57.1064F, 54.39582F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Thorns,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayRazorwireLeft"),
childName = "Left_Lower_Arm",
localPos = new Vector3(-0.00853F, -0.06238F, 0.00323F),
localAngles = new Vector3(277.8344F, 256.4616F, 106.0955F),
localScale = new Vector3(1.2F, 1.2F, 1.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.LunarPrimaryReplacement,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBirdEye"),
childName = "Head",
localPos = new Vector3(-0.31505F, 0.6825F, 0.72752F),
localAngles = new Vector3(270F, 349.7074F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.NovaOnLowHealth,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayJellyGuts"),
childName = "Left_Upper_Arm",
localPos = new Vector3(-0.50576F, 0.39424F, -0.13732F),
localAngles = new Vector3(-0.00001F, 23.76322F, 0.00008F),
localScale = new Vector3(0.15F, 0.15F, 0.15F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.LunarTrinket,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBeads"),
childName = "Yoko_Knee_L",
localPos = new Vector3(-0.02694F, 1.32312F, -0.28368F),
localAngles = new Vector3(312.126F, 251.9112F, 204.201F),
localScale = new Vector3(2F, 2F, 2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Plant,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayInterstellarDeskPlant"),
childName = "Head",
localPos = new Vector3(0.47762F, 0.54678F, 0.56695F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(0.075F, 0.075F, 0.075F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Bear,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBear"),
childName = "Head",
localPos = new Vector3(0.00461F, 0.78781F, 0.07632F),
localAngles = new Vector3(338.8434F, 0.00001F, 0F),
localScale = new Vector3(0.2034F, 0.2034F, 0.2034F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.DeathMark,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDeathMark"),
childName = "Yoko_Head",
localPos = new Vector3(-0.14172F, 0.42787F, 0.11206F),
localAngles = new Vector3(312.049F, 225.5405F, 110.5738F),
localScale = new Vector3(0.055F, 0.04F, 0.04F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ExplodeOnDeath,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWilloWisp"),
childName = "Head",
localPos = new Vector3(-0.40969F, 0.64228F, -0.05976F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Seed,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySeed"),
childName = "Head",
localPos = new Vector3(0.01663F, 0.52392F, 0.23733F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.0275F, 0.0275F, 0.0275F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.SprintOutOfCombat,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWhip"),
childName = "Yoko_Root",
localPos = new Vector3(0.45319F, 2.37929F, 0.25523F),
localAngles = new Vector3(324.4162F, 353.8228F, 32.80781F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.CooldownOnCrit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySkull"),
childName = "Simon_Head",
localPos = new Vector3(0F, 0.14044F, 0.05451F),
localAngles = new Vector3(281.4445F, 179.4497F, 175.5473F),
localScale = new Vector3(0.2F, 0.3F, 0.3F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Phasing,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayStealthkit"),
childName = "Right_Lower_Leg",
localPos = new Vector3(0.00098F, 0.00001F, -0.11908F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(0.3F, 0.3F, 0.3F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.PersonalShield,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayShieldGenerator"),
childName = "Left_Lower_Leg",
localPos = new Vector3(-0.00034F, 0.00002F, 0.04232F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(0.3F, 0.3F, 0.3F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ShockNearby,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTeslaCoil"),
childName = "Head",
localPos = new Vector3(0F, 0.01776F, -0.68942F),
localAngles = new Vector3(357.3112F, 181.5587F, 181.8791F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ShieldOnly,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayShieldBug"),
childName = "Yoko_Head",
localPos = new Vector3(0.0868F, 0.3114F, 0F),
localAngles = new Vector3(348.1819F, 268.0985F, 0.3896F),
localScale = new Vector3(0.3521F, 0.3521F, 0.3521F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayShieldBug"),
childName = "Yoko_Head",
localPos = new Vector3(-0.0868F, 0.3114F, 0F),
localAngles = new Vector3(11.8181F, 268.0985F, 359.6104F),
localScale = new Vector3(0.3521F, 0.3521F, -0.3521F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.AlienHead,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAlienHead"),
childName = "Yoko_Root",
localPos = new Vector3(0.34627F, 2.46231F, 0.0937F),
localAngles = new Vector3(284.1172F, 243.7966F, 260.89F),
localScale = new Vector3(2F, 2F, 2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.HeadHunter,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySkullCrown"),
childName = "Yoko_UpperBody2",
localPos = new Vector3(0.01085F, -0.09254F, -0.02302F),
localAngles = new Vector3(13.77267F, 0.2962F, 0.96665F),
localScale = new Vector3(0.6F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.EnergizedOnEquipmentUse,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWarHorn"),
childName = "Simon_Chest",
localPos = new Vector3(0F, 0.17587F, 0.10491F),
localAngles = new Vector3(359.5171F, 354.8591F, 349.2732F),
localScale = new Vector3(0.15F, 0.15F, 0.15F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.FlatHealth,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySteakCurved"),
childName = "Kamina_Head",
localPos = new Vector3(-0.0203F, 0.00555F, 0.11748F),
localAngles = new Vector3(294.98F, 180F, 180F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Tooth,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayToothMeshLarge"),
childName = "Head",
localPos = new Vector3(0F, 1.20353F, 0.98157F),
localAngles = new Vector3(299.296F, 126.9402F, 56.74509F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Pearl,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayPearl"),
childName = "Head",
localPos = new Vector3(0F, 0.58529F, 0F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.ShinyPearl,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayShinyPearl"),
childName = "Head",
localPos = new Vector3(0F, 0.48178F, 0F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.BonusGoldPackOnKill,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTome"),
childName = "Head",
localPos = new Vector3(-0.30753F, 0.53831F, 0.3931F),
localAngles = new Vector3(270F, 310.4225F, 0F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Squid,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySquidTurret"),
childName = "Head",
localPos = new Vector3(0F, -0.27876F, 0.16498F),
localAngles = new Vector3(0F, 180F, 180F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Icicle,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFrostRelic"),
childName = "Head",
localPos = new Vector3(0F, -0.79243F, 0F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Talisman,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTalisman"),
childName = "Head",
localPos = new Vector3(0.8357F, 2.6406F, -0.2979F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.LaserTurbine,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayLaserTurbine"),
childName = "Head",
localPos = new Vector3(0F, 0.94622F, 0.74675F),
localAngles = new Vector3(43.58096F, -0.00003F, -0.00001F),
localScale = new Vector3(0.4F, 0.4F, 0.4F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.FocusConvergence,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFocusedConvergence"),
childName = "Head",
localPos = new Vector3(0F, 2.81473F, 0F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.Incubator,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAncestralIncubator"),
childName = "Right_Upper_Arm",
localPos = new Vector3(0F, 0.4709F, 0.03725F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(0.08F, 0.08F, 0.08F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.FireballsOnHit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFireballsOnHit"),
childName = "Head",
localPos = new Vector3(0F, 0.89723F, 1.42927F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.SiphonOnLowHealth,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySiphonOnLowHealth"),
childName = "Head",
localPos = new Vector3(-0.46855F, 0.72423F, 0.48933F),
localAngles = new Vector3(0.00004F, 196.8021F, 0.00008F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.BleedOnHitAndExplode,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBleedOnHitAndExplode"),
childName = "Right_Lower_Arm",
localPos = new Vector3(-0.01192F, 0.46981F, -0.07995F),
localAngles = new Vector3(-0.00001F, 180F, 180F),
localScale = new Vector3(0.2F, 0.3F, 0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.MonstersOnShrineUse,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMonstersOnShrineUse"),
childName = "Head",
localPos = new Vector3(-0.37353F, 0.61696F, 0.43219F),
localAngles = new Vector3(0F, 0F, 90F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Items.RandomDamageZone,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayRandomDamageZone"),
childName = "Head",
localPos = new Vector3(0F, -0.26219F, -0.5683F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Fruit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayFruit"),
childName = "Yoko_Root",
localPos = new Vector3(-0.18788F, 1.74422F, 0.08566F),
localAngles = new Vector3(358.554F, 48.61952F, 12.62497F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.AffixRed,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteHorn"),
childName = "Kamina_Head",
localPos = new Vector3(0.1201F, 0.2516F, 0F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.1036F, 0.1036F, 0.1036F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteHorn"),
childName = "Kamina_Head",
localPos = new Vector3(-0.1201F, 0.2516F, 0F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(-0.1036F, 0.1036F, 0.1036F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.AffixBlue,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteRhinoHorn"),
childName = "Kamina_Head",
localPos = new Vector3(0F, 0.2648F, 0.1528F),
localAngles = new Vector3(315F, 0F, 0F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteRhinoHorn"),
childName = "Kamina_Head",
localPos = new Vector3(0.00001F, 0.28864F, 0.10571F),
localAngles = new Vector3(300F, 0F, 0F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.AffixWhite,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteIceCrown"),
childName = "Kamina_Head",
localPos = new Vector3(0F, 0.2836F, 0F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(0.0265F, 0.0265F, 0.0265F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.AffixPoison,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteUrchinCrown"),
childName = "Kamina_Head",
localPos = new Vector3(0F, 0.2679F, 0F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(0.0496F, 0.0496F, 0.0496F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.AffixHaunted,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteStealthCrown"),
childName = "Kamina_Head",
localPos = new Vector3(0F, 0.2143F, 0F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(0.065F, 0.065F, 0.065F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.CritOnUse,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayNeuralImplant"),
childName = "Muzzle",
localPos = new Vector3(-0.55413F, -2.17791F, -0.36188F),
localAngles = new Vector3(282.5182F, 50.4808F, 30.10366F),
localScale = new Vector3(0.3F, 0.3F, 0.3F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.DroneBackup,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayRadio"),
childName = "Yoko_UpperBody2",
localPos = new Vector3(-0.12498F, 0.80327F, 0.07354F),
localAngles = new Vector3(342.7247F, 61.61902F, 331.2042F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Lightning,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayLightningArmRight"),
childName = "Head",
localPos = new Vector3(-0.10993F, 1.17898F, -0.18093F),
localAngles = new Vector3(-0.00005F, 336.2121F, 50.58207F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.BurnNearby,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayPotion"),
childName = "Head",
localPos = new Vector3(0.45861F, 0.57601F, 0.40195F),
localAngles = new Vector3(359.1402F, 0.1068F, 331.8908F),
localScale = new Vector3(0.0307F, 0.0307F, 0.0307F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.CrippleWard,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEffigy"),
childName = "Head",
localPos = new Vector3(0.0768F, 0.68991F, 0.16597F),
localAngles = new Vector3(0F, 180F, 0F),
localScale = new Vector3(0.2812F, 0.2812F, 0.2812F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.QuestVolatileBattery,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayBatteryArray"),
childName = "Head",
localPos = new Vector3(0F, 0.66125F, 0.47802F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.GainArmor,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayElephantFigure"),
childName = "Head",
localPos = new Vector3(0.03604F, 0.63717F, 0.36686F),
localAngles = new Vector3(-0.00001F, 33.35197F, 276.7059F),
localScale = new Vector3(0.6279F, 0.6279F, 0.6279F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Recycle,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayRecycler"),
childName = "Head",
localPos = new Vector3(0.02627F, 0.62378F, 0.43577F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(0.0802F, 0.0802F, 0.0802F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.FireBallDash,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEgg"),
childName = "Head",
localPos = new Vector3(-0.49267F, 0.5351F, 0.1218F),
localAngles = new Vector3(297.0638F, 224.1774F, 139.1307F),
localScale = new Vector3(0.1891F, 0.1891F, 0.1891F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Cleanse,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayWaterPack"),
childName = "Yoko_UpperBody2",
localPos = new Vector3(0.00777F, 0.37571F, -0.18797F),
localAngles = new Vector3(0F, 180F, 0F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Tonic,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTonic"),
childName = "Simon_Hips",
localPos = new Vector3(-0.14983F, 0.28585F, 0.22657F),
localAngles = new Vector3(339.065F, 313.0146F, 350.6208F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Gateway,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayVase"),
childName = "Head",
localPos = new Vector3(-0.37794F, 0.90483F, 0.46911F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(0.3F, 0.3F, 0.3F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Meteor,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayMeteor"),
childName = "Head",
localPos = new Vector3(-0.78166F, 2.55919F, -0.11489F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Saw,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplaySawmerang"),
childName = "Head",
localPos = new Vector3(-0.71505F, 2.59759F, -0.11678F),
localAngles = new Vector3(62.06876F, 180.0001F, 180.0001F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Blackhole,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGravCube"),
childName = "Head",
localPos = new Vector3(-0.91098F, 2.64487F, -0.18746F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.Scanner,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayScanner"),
childName = "Right_Upper_Arm",
localPos = new Vector3(0F, 0.13901F, 0F),
localAngles = new Vector3(0F, 160F, 90F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.DeathProjectile,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDeathProjectile"),
childName = "Yoko_UpperBody2",
localPos = new Vector3(0.06207F, 0.55251F, -0.2426F),
localAngles = new Vector3(0F, 180F, 0F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.LifestealOnHit,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayLifestealOnHit"),
childName = "Yoko_UpperBody2",
localPos = new Vector3(-0.24302F, 0.60944F, -0.04328F),
localAngles = new Vector3(358.3056F, 322.2139F, 351.8546F),
localScale = new Vector3(0.1246F, 0.1246F, 0.1246F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });

                itemDisplayRules.Add(new ItemDisplayRuleSet.KeyAssetRuleGroup
                {
                    keyAsset = RoR2Content.Equipment.TeamWarCry,
                    displayRuleGroup = new DisplayRuleGroup
                    {
                        rules = new ItemDisplayRule[]
                        {
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayTeamWarCry"),
childName = "Head",
localPos = new Vector3(0F, 0.97603F, 0.60917F),
localAngles = new Vector3(337.0476F, 180F, 0F),
localScale = new Vector3(0.1233F, 0.1233F, 0.1233F),
                            limbMask = LimbFlags.None
                        }
                        }
                    }
                });
            }
            #endregion

            itemDisplayRuleSet.keyAssetRuleGroups = itemDisplayRules.ToArray();
            itemDisplayRuleSet.GenerateRuntimeValues();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void CreateScepterSkills()
        {
            string prefix = TTGL_SurvivorPlugin.developerPrefix;

            scepterSkillDef = ScriptableObject.CreateInstance<SkillDef>();

            scepterSkillDef.skillName = "YokoScepterRifle";
            scepterSkillDef.skillNameToken = prefix + "_LAGANN_BODY_SECONDARY_SCEPTER_RIFLE_NAME";
            scepterSkillDef.skillDescriptionToken = prefix + "_LAGANN_BODY_SECONDARY_SCEPTER_RIFLE_DESCRIPTION";
            scepterSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("YokoRifleAncientScepterIcon");
            scepterSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.YokoScepterRifle));
            scepterSkillDef.activationStateMachineName = "Weapon";
            scepterSkillDef.baseMaxStock = 1;
            scepterSkillDef.baseRechargeInterval = 1f;
            scepterSkillDef.beginSkillCooldownOnSkillEnd = false;
            scepterSkillDef.canceledFromSprinting = false;
            scepterSkillDef.forceSprintDuringState = false;
            scepterSkillDef.fullRestockOnAssign = true;
            scepterSkillDef.interruptPriority = EntityStates.InterruptPriority.Skill;
            scepterSkillDef.isCombatSkill = true;
            scepterSkillDef.mustKeyPress = false;
            scepterSkillDef.cancelSprintingOnActivation = false;
            scepterSkillDef.rechargeStock = 1;
            scepterSkillDef.requiredStock = 1;
            scepterSkillDef.stockToConsume = 1;

            TTGL_SurvivorPlugin.skillDefs.Add(scepterSkillDef);

            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(scepterSkillDef, "LagannBody", SkillSlot.Secondary, 0);
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(scepterSkillDef, "LagannBody", SkillSlot.Secondary, 1);
        }
    }
}