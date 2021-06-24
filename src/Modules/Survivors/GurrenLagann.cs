using BepInEx.Configuration;
using EntityStates;
using KinematicCharacterController;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TTGL_Survivor.Modules.Achievements;
using TTGL_Survivor.Modules.Components;
using TTGL_Survivor.SkillStates;
using TTGL_Survivor.UI;
using UnityEngine;

namespace TTGL_Survivor.Modules.Survivors
{
    public class GurrenLagann: BaseSurvivor
    {
        internal static GameObject characterPrefab;
        internal static GameObject displayPrefab;
        internal static SkillDef gigaDrillBreakerSkillDef;

        internal static ConfigEntry<bool> characterEnabled;

        // item display stuffs
        internal static ItemDisplayRuleSet itemDisplayRuleSet { get; set; }
        internal static List<ItemDisplayRuleSet.KeyAssetRuleGroup> itemDisplayRules { get; set; }

        public void CreateCharacter()
        {
            // this creates a config option to enable the character- feel free to remove if the character is the only thing in your mod
            //characterEnabled = Modules.Config.CharacterEnableConfig("GurrenLagann");

            if (true)//characterEnabled.Value)
            {
                #region Body
                characterPrefab = CreatePrefab("GurrenLagannBody", "GurrenLagannPrefab");
                //Setup spiritEnergy components
                characterPrefab.AddComponent<TTGLMusicRemote>();
                var spiralEnergyComponent = characterPrefab.AddComponent<SpiralEnergyComponent>();
                spiralEnergyComponent.energyModifier = 2.0f;
                characterPrefab.AddComponent<GurrenLagannController>();
                var entityStateMachine = characterPrefab.GetComponent<EntityStateMachine>();
                entityStateMachine.mainStateType = new SerializableEntityStateType(typeof(SkillStates.GurrenLagannMain));
                entityStateMachine.initialStateType = new SerializableEntityStateType(typeof(SkillStates.GurrenLagannMain));
                //Fix interaction distance because GurrenLagann is too big
                var interactor = characterPrefab.GetComponent<Interactor>();
                interactor.maxInteractionDistance = 20f;
                #endregion

                #region Model

                SetupCharacterModel(characterPrefab, new CustomRendererInfo[] {
                new CustomRendererInfo
                {
                    childName = "GurrenLagann",
                },
                new CustomRendererInfo
                {
                    childName = "GigaDrill",
                },
                new CustomRendererInfo
                {
                    childName = "GurrenLagannBodyDrills",
                }}, 0);
                #endregion

                displayPrefab = CreateDisplayPrefab("GurrenLagannMenuPrefab", characterPrefab);
                var lagannCombineUnlockable = Unlockables.AddUnlockable<LagannCombineAchievement>(true);
                RegisterNewSurvivor(characterPrefab, displayPrefab, new Color(0.25f, 0.65f, 0.25f), "GURRENLAGANN", lagannCombineUnlockable, 12.3f);

                CreateHurtBoxes();
                CreateHitboxes();
                CreateSkills();
                CreateSkins();
                InitializeItemDisplays();
                CreateGenericDoppelganger(characterPrefab, "GurrenLagannMonsterMaster", "Merc");
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
            }

            TTGL_SurvivorPlugin.bodyPrefabs.Add(newPrefab);
            return newPrefab;
        }
        protected override void SetupCharacterBody(string bodyName, GameObject newPrefab, Transform modelBaseTransform)
        {
            CharacterBody bodyComponent = newPrefab.GetComponent<CharacterBody>();

            bodyComponent.bodyIndex = BodyIndex.None;
            bodyComponent.name = bodyName;
            bodyComponent.bodyColor = new Color(0.25f, 0.65f, 0.25f);
            bodyComponent.baseNameToken = TTGL_SurvivorPlugin.developerPrefix + "_GURRENLAGANN_BODY_NAME";
            bodyComponent.subtitleNameToken = TTGL_SurvivorPlugin.developerPrefix + "_GURRENLAGANN_BODY_SUBTITLE";
            bodyComponent.portraitIcon = Modules.Assets.mainAssetBundle.LoadAsset<Texture>("GurrenLagannIcon");
            bodyComponent.crosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");

            bodyComponent.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            bodyComponent.rootMotionInMainState = false;

            bodyComponent.baseMaxHealth = 330f;
            bodyComponent.levelMaxHealth = 132f;

            bodyComponent.baseRegen = 4.0f;
            bodyComponent.levelRegen = 1.2f;

            bodyComponent.baseMaxShield = 0f;
            bodyComponent.levelMaxShield = 0f;

            bodyComponent.baseMoveSpeed = 7f;
            bodyComponent.levelMoveSpeed = 0f;
            
            bodyComponent.baseAcceleration = 80f;

            bodyComponent.baseJumpPower = 25f;
            bodyComponent.levelJumpPower = 0f;

            bodyComponent.baseDamage = 18f;
            bodyComponent.levelDamage = 4.8f;

            bodyComponent.baseAttackSpeed = 1f;
            bodyComponent.levelAttackSpeed = 0f;

            bodyComponent.baseArmor = 30f;
            bodyComponent.levelArmor = 0f;

            bodyComponent.baseCrit = 0f;
            bodyComponent.levelCrit = 0f;

            bodyComponent.baseJumpCount = 1;

            bodyComponent.sprintingSpeedMultiplier = 2.5f;

            bodyComponent.hideCrosshair = false;
            bodyComponent.aimOriginTransform = modelBaseTransform.Find("AimOrigin");
            bodyComponent.hullClassification = HullClassification.Human;

            bodyComponent.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");

            bodyComponent.isChampion = false;
        }

        protected override void SetupCharacterMotor(GameObject newPrefab)
        {
            CharacterMotor motorComponent = newPrefab.GetComponent<CharacterMotor>();
            motorComponent.mass = 2000f;
            KinematicCharacterMotor kinematicCharacterMotor = newPrefab.GetComponent<KinematicCharacterMotor>();
            if (kinematicCharacterMotor)
            {
                kinematicCharacterMotor.MaxStepHeight = 10f;
            }
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
            cameraPivot.transform.localPosition = (isDisplay) ? new Vector3(0f, 2.6f, 0f): new Vector3(0f, 16.0f, 0f);
            cameraPivot.transform.localRotation = Quaternion.identity;
            cameraPivot.transform.localScale = Vector3.one;

            GameObject aimOrigin = new GameObject("AimOrigin");
            aimOrigin.transform.parent = modelBase.transform;
            aimOrigin.transform.localPosition = (isDisplay) ? new Vector3(0f, 2.6f, 0f) : new Vector3(0f, 16.0f, 0f);
            aimOrigin.transform.localRotation = Quaternion.identity;
            aimOrigin.transform.localScale = Vector3.one;
            prefab.GetComponent<CharacterBody>().aimOriginTransform = aimOrigin.transform;

            modelTransform.parent = modelBase.transform;
            modelTransform.localPosition = Vector3.zero;
            modelTransform.localRotation = Quaternion.identity;
            modelTransform.localScale = (isDisplay) ? new Vector3(0.12f, 0.12f, 0.12f) : new Vector3(0.8f, 0.8f, 0.8f);

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
            cameraParams.standardLocalCameraPos = cameraTargetParams.cameraParams.standardLocalCameraPos * 3;
            cameraParams.wallCushion = cameraTargetParams.cameraParams.wallCushion;
            cameraTargetParams.cameraParams = cameraParams;
        }
        protected override void SetupRigidbody(GameObject prefab)
        {
            Rigidbody rigidbody = prefab.GetComponent<Rigidbody>();
            rigidbody.mass = 2000f;
        }

        protected override void SetupFootstepController(GameObject model)
        {
            var footstepHandler = model.AddComponent<FootstepHandler>();
            footstepHandler.enableFootstepDust = true;
            footstepHandler.baseFootstepString = "Play_player_footstep";
            footstepHandler.footstepDustPrefab = Modules.Assets.mainAssetBundle.LoadAsset<GameObject>("DustDirtyPoofSoft");
        }

        private void CreateHurtBoxes()
        {
            GameObject model = characterPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            HurtBoxGroup hurtBoxGroup = model.AddComponent<HurtBoxGroup>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            List<HurtBox> hurtBoxes = new List<HurtBox>();
            string[] hurtBoxeNames = new string[]{
                "HurtboxChest",                
                "HurtboxLeftLowerArm",
                "HurtboxLeftUpperArm",
                "BarrierboxLeftUpperArm",                
                "HurtboxRightLowerArm",
                "HurtboxRightUpperArm",
                "BarrierboxRightUpperArm",
                "HurtboxHead",
                "HurtboxLeftLowerLeg",
                "HurtboxLeftUpperLeg",
                "HurtboxRightLowerLeg",
                "HurtboxRightUpperLeg",
            };
            for(short i = 0; i < hurtBoxeNames.Length; i++)
            {
                var hutBoxName = hurtBoxeNames[i];
                var hurtBoxTransform = childLocator.FindChild(hutBoxName);
                if (!hurtBoxTransform)
                {
                    TTGL_SurvivorPlugin.instance.Logger.LogError("Could not set up main hurtbox: make sure you have a transform pair in your prefab's ChildLocator component called 'Head'");
                    continue;
                }
                HurtBox hurtbox = hurtBoxTransform.gameObject.AddComponent<HurtBox>();
                hurtbox.gameObject.layer = LayerIndex.entityPrecise.intVal;
                hurtbox.healthComponent = characterPrefab.GetComponent<HealthComponent>();
                hurtbox.isBullseye = (hutBoxName == "HurtboxHead");
                hurtbox.damageModifier = (hutBoxName.StartsWith("Barrier"))? HurtBox.DamageModifier.Barrier: HurtBox.DamageModifier.Normal;
                hurtbox.hurtBoxGroup = hurtBoxGroup;
                hurtbox.indexInGroup = i;
                hurtBoxes.Add(hurtbox);
            }            
            hurtBoxGroup.hurtBoxes = hurtBoxes.ToArray();
            hurtBoxGroup.mainHurtBox = hurtBoxes.FirstOrDefault();
            hurtBoxGroup.bullseyeCount = 1;
        }

        private void CreateHitboxes()
        {
            GameObject model = characterPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            Transform hitboxTransform = childLocator.FindChild("DammageHitbox");
            SetupHitbox(model, hitboxTransform, "DammageHitbox");
        }

        private void CreateSkills()
        {
            Modules.Skills.CreateSkillFamilies(characterPrefab);
            Modules.Skills.CreateFirstExtraSkillFamily(characterPrefab);

            string prefix = TTGL_SurvivorPlugin.developerPrefix;

            #region Passive

            SkillLocator skillLocator = characterPrefab.GetComponent<SkillLocator>();
            skillLocator.passiveSkill.enabled = true;
            skillLocator.passiveSkill.skillNameToken = prefix + "_GURRENLAGANN_BODY_PASSIVE_NAME";
            skillLocator.passiveSkill.skillDescriptionToken = prefix + "_GURRENLAGANN_BODY_PASSIVE_DESCRIPTION";
            skillLocator.passiveSkill.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("SpiralPowerIcon");

            #endregion

            #region Primary
            SkillDef spiralingComboSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            spiralingComboSkillDef.skillName = "GurrenLagannSpiralingCombo";
            spiralingComboSkillDef.skillNameToken = prefix + "_GURRENLAGANN_BODY_SPIRALINGCOMBO_NAME";
            spiralingComboSkillDef.skillDescriptionToken = prefix + "_GURRENLAGANN_BODY_SPIRALINGCOMBO_DESCRIPTION";
            spiralingComboSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("SpiralComboIcon");
            spiralingComboSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenLagannSpiralingCombo));
            spiralingComboSkillDef.activationStateMachineName = "Body";
            spiralingComboSkillDef.baseMaxStock = 1;
            spiralingComboSkillDef.baseRechargeInterval = 0f;
            spiralingComboSkillDef.beginSkillCooldownOnSkillEnd = false;
            spiralingComboSkillDef.canceledFromSprinting = false;
            spiralingComboSkillDef.forceSprintDuringState = false;
            spiralingComboSkillDef.fullRestockOnAssign = true;
            spiralingComboSkillDef.interruptPriority = EntityStates.InterruptPriority.Any;
            spiralingComboSkillDef.cancelSprintingOnActivation = false;
            spiralingComboSkillDef.isCombatSkill = true;
            spiralingComboSkillDef.mustKeyPress = false;
            spiralingComboSkillDef.rechargeStock = 1;
            spiralingComboSkillDef.requiredStock = 0;
            spiralingComboSkillDef.stockToConsume = 0;
            TTGL_SurvivorPlugin.skillDefs.Add(spiralingComboSkillDef);
            Modules.Skills.AddPrimarySkill(characterPrefab, spiralingComboSkillDef);

            #endregion

            #region Secondary
            SkillDef throwingShadesSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            throwingShadesSkillDef.skillName = "GurrenLagannThrowingShades";
            throwingShadesSkillDef.skillNameToken = prefix + "_GURRENLAGANN_BODY_THROWINGSHADES_NAME";
            throwingShadesSkillDef.skillDescriptionToken = prefix + "_GURRENLAGANN_BODY_THROWINGSHADES_DESCRIPTION";
            throwingShadesSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("GurrenLagannShadeThrowIcon");
            throwingShadesSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenLagannThrowingShades));
            throwingShadesSkillDef.activationStateMachineName = "Weapon";
            throwingShadesSkillDef.baseMaxStock = 2;
            throwingShadesSkillDef.baseRechargeInterval = 3f;
            throwingShadesSkillDef.beginSkillCooldownOnSkillEnd = false;
            throwingShadesSkillDef.canceledFromSprinting = false;
            throwingShadesSkillDef.forceSprintDuringState = false;
            throwingShadesSkillDef.fullRestockOnAssign = true;
            throwingShadesSkillDef.interruptPriority = EntityStates.InterruptPriority.Skill;
            throwingShadesSkillDef.isCombatSkill = true;
            throwingShadesSkillDef.mustKeyPress = false;
            throwingShadesSkillDef.cancelSprintingOnActivation = false;
            throwingShadesSkillDef.rechargeStock = 1;
            throwingShadesSkillDef.requiredStock = 1;
            throwingShadesSkillDef.stockToConsume = 1;
            throwingShadesSkillDef.keywordTokens = new string[] { "KEYWORD_AGILE" };
            TTGL_SurvivorPlugin.skillDefs.Add(throwingShadesSkillDef);
            Modules.Skills.AddSecondarySkill(characterPrefab, throwingShadesSkillDef);

            #endregion

            #region Utility
            SkillDef tornadoKickSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            tornadoKickSkillDef.skillName = "GurrenLagannTornadoKick";
            tornadoKickSkillDef.skillNameToken = prefix + "_GURRENLAGANN_BODY_TORNADOKICK_NAME";
            tornadoKickSkillDef.skillDescriptionToken = prefix + "_GURRENLAGANN_BODY_TORNADOKICK_DESCRIPTION";
            tornadoKickSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("HuricaneKickIcon");
            tornadoKickSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenLagannTornadoKick));
            tornadoKickSkillDef.activationStateMachineName = "Body";
            tornadoKickSkillDef.baseMaxStock = 1;
            tornadoKickSkillDef.baseRechargeInterval = 4f;
            tornadoKickSkillDef.beginSkillCooldownOnSkillEnd = true;
            tornadoKickSkillDef.canceledFromSprinting = false;
            tornadoKickSkillDef.forceSprintDuringState = true;
            tornadoKickSkillDef.fullRestockOnAssign = true;
            tornadoKickSkillDef.interruptPriority = EntityStates.InterruptPriority.PrioritySkill;
            tornadoKickSkillDef.isCombatSkill = true;
            tornadoKickSkillDef.mustKeyPress = false;
            tornadoKickSkillDef.cancelSprintingOnActivation = false;
            tornadoKickSkillDef.rechargeStock = 1;
            tornadoKickSkillDef.requiredStock = 1;
            tornadoKickSkillDef.stockToConsume = 1;
            TTGL_SurvivorPlugin.skillDefs.Add(tornadoKickSkillDef);
            Modules.Skills.AddUtilitySkill(characterPrefab, tornadoKickSkillDef);

            #endregion

            #region Special

            SpiralEnergySkillDef gigaDrillMaximumSkillDef = ScriptableObject.CreateInstance<SpiralEnergySkillDef>();
            gigaDrillMaximumSkillDef.energyCost = GurrenLagannGigaDrillMaximum.energyCost;
            gigaDrillMaximumSkillDef.skillName = "GurrenLagannGigaDrillMaximum";
            gigaDrillMaximumSkillDef.skillNameToken = prefix + "_GURRENLAGANN_BODY_GIGADRILLMAXIMUM_NAME";
            gigaDrillMaximumSkillDef.skillDescriptionToken = prefix + "_GURRENLAGANN_BODY_GIGADRILLMAXIMUM_DESCRIPTION";
            gigaDrillMaximumSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("GigaDrillMaximumIcon");
            gigaDrillMaximumSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(GurrenLagannGigaDrillMaximum));
            gigaDrillMaximumSkillDef.activationStateMachineName = "Body";
            gigaDrillMaximumSkillDef.baseMaxStock = 1;
            gigaDrillMaximumSkillDef.baseRechargeInterval = 8f;
            gigaDrillMaximumSkillDef.beginSkillCooldownOnSkillEnd = true;
            gigaDrillMaximumSkillDef.canceledFromSprinting = false;
            gigaDrillMaximumSkillDef.forceSprintDuringState = false;
            gigaDrillMaximumSkillDef.fullRestockOnAssign = true;
            gigaDrillMaximumSkillDef.interruptPriority = EntityStates.InterruptPriority.Skill;
            gigaDrillMaximumSkillDef.isCombatSkill = true;
            gigaDrillMaximumSkillDef.mustKeyPress = false;
            gigaDrillMaximumSkillDef.cancelSprintingOnActivation = true;
            gigaDrillMaximumSkillDef.rechargeStock = 1;
            gigaDrillMaximumSkillDef.requiredStock = 1;
            gigaDrillMaximumSkillDef.stockToConsume = 1;
            TTGL_SurvivorPlugin.skillDefs.Add(gigaDrillMaximumSkillDef);
            Modules.Skills.AddSpecialSkill(characterPrefab, gigaDrillMaximumSkillDef);

            gigaDrillBreakerSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            gigaDrillBreakerSkillDef.skillName = "GurrenLagannGigaDrillBreak";
            gigaDrillBreakerSkillDef.skillNameToken = prefix + "_GURRENLAGANN_BODY_GIGADRILLBREAK_NAME";
            gigaDrillBreakerSkillDef.skillDescriptionToken = prefix + "_GURRENLAGANN_BODY_GIGADRILLBREAK_DESCRIPTION";
            gigaDrillBreakerSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("GigaDrillBreakIcon");
            gigaDrillBreakerSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenLagannGigaDrillBreak));
            gigaDrillBreakerSkillDef.activationStateMachineName = "Body";
            gigaDrillBreakerSkillDef.baseMaxStock = 1;
            gigaDrillBreakerSkillDef.baseRechargeInterval = 60f;
            gigaDrillBreakerSkillDef.beginSkillCooldownOnSkillEnd = true;
            gigaDrillBreakerSkillDef.canceledFromSprinting = false;
            gigaDrillBreakerSkillDef.forceSprintDuringState = false;
            gigaDrillBreakerSkillDef.fullRestockOnAssign = true;
            gigaDrillBreakerSkillDef.interruptPriority = EntityStates.InterruptPriority.Skill;
            gigaDrillBreakerSkillDef.isCombatSkill = true;
            gigaDrillBreakerSkillDef.mustKeyPress = true;
            gigaDrillBreakerSkillDef.cancelSprintingOnActivation = true;
            gigaDrillBreakerSkillDef.rechargeStock = 1;
            gigaDrillBreakerSkillDef.requiredStock = 1;
            gigaDrillBreakerSkillDef.stockToConsume = 1;
            TTGL_SurvivorPlugin.skillDefs.Add(gigaDrillBreakerSkillDef);

            #endregion

            #region FirstExtra
            SkillDef gurrenLagannSplitSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            gurrenLagannSplitSkillDef.skillName = "GurrenLagannSplit";
            gurrenLagannSplitSkillDef.skillNameToken = prefix + "_GURRENLAGANN_BODY_SPLIT_NAME";
            gurrenLagannSplitSkillDef.skillDescriptionToken = prefix + "_GURRENLAGANN_BODY_SPLIT_DESCRIPTION";
            gurrenLagannSplitSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("GurrenLagannSplitIcon");
            gurrenLagannSplitSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenLagannSplit));
            gurrenLagannSplitSkillDef.activationStateMachineName = "Body";
            gurrenLagannSplitSkillDef.baseMaxStock = 1;
            gurrenLagannSplitSkillDef.baseRechargeInterval = 15f;
            gurrenLagannSplitSkillDef.beginSkillCooldownOnSkillEnd = true;
            gurrenLagannSplitSkillDef.canceledFromSprinting = false;
            gurrenLagannSplitSkillDef.forceSprintDuringState = false;
            gurrenLagannSplitSkillDef.fullRestockOnAssign = false;
            gurrenLagannSplitSkillDef.interruptPriority = EntityStates.InterruptPriority.Skill;
            gurrenLagannSplitSkillDef.isCombatSkill = true;
            gurrenLagannSplitSkillDef.mustKeyPress = true;
            gurrenLagannSplitSkillDef.cancelSprintingOnActivation = true;
            gurrenLagannSplitSkillDef.rechargeStock = 1;
            gurrenLagannSplitSkillDef.requiredStock = 1;
            gurrenLagannSplitSkillDef.stockToConsume = 1;
            TTGL_SurvivorPlugin.skillDefs.Add(gurrenLagannSplitSkillDef);
            Modules.Skills.AddFirstExtraSkill(characterPrefab, gurrenLagannSplitSkillDef);

            #endregion
        }

        private void CreateSkins()
        {
            GameObject model = characterPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;

            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            SkinDef defaultSkin = Modules.Skins.CreateSkinDef(TTGL_SurvivorPlugin.developerPrefix + "_GURRENLAGANN_BODY_DEFAULT_SKIN_NAME",
                Assets.mainAssetBundle.LoadAsset<Sprite>("DefaultSkinIcon"),
                defaultRenderers,
                mainRenderer,
                model);
            
            skins.Add(defaultSkin);
            #endregion
            
            skinController.skins = skins.ToArray();
        }

        internal virtual void InitializeItemDisplays()
        {
            GameObject model = characterPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            itemDisplayRuleSet = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
            itemDisplayRuleSet.name = "idrs" + "GurrenLagann";

            characterModel.itemDisplayRuleSet = itemDisplayRuleSet;
        }
        internal static void SetItemDisplays()
        {
            itemDisplayRules = new List<ItemDisplayRuleSet.KeyAssetRuleGroup>();

            // add item displays here
            //  HIGHLY recommend using KingEnderBrine's ItemDisplayPlacementHelper mod for this
            #region Item Displays

            if (Config.gurrenLaganItemDisplayEnabled.Value)
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
                           childName = "Chest",
localPos = new Vector3(-1.99378F, -1.25186F, -0.06758F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(2F, 2F, 2F),
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
childName = "LeftUpperArm",
localPos = new Vector3(1.39283F, -1.97193F, 0.29029F),
localAngles = new Vector3(290.7965F, 287.2191F, 76.40903F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
childName = "LeftUpperArm",
localPos = new Vector3(-0.14534F, -0.43121F, -0.71554F),
localAngles = new Vector3(1.80895F, 334.5503F, 180.3437F),
localScale = new Vector3(2F, 2F, 2F),
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
childName = "Head",
localPos = new Vector3(1.12899F, 0.33573F, 0F),
localAngles = new Vector3(0F, 90F, 180F),
localScale = new Vector3(1.5F, 1.5F, 1.5F),
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
childName = "Chest",
localPos = new Vector3(0.14887F, 1.75894F, -1.48037F),
localAngles = new Vector3(314.5482F, 0F, 0F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
childName = "LeftLowerArm",
localPos = new Vector3(0.66639F, 3.15118F, 0.76606F),
localAngles = new Vector3(0F, 44.25821F, 0F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
childName = "LeftUpperArm",
localPos = new Vector3(1.00446F, -3.94865F, -0.4547F),
localAngles = new Vector3(1.81924F, 330.8782F, 187.9597F),
localScale = new Vector3(1F, 1F, 1F),
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
childName = "Head",
localPos = new Vector3(0.18576F, 1.24089F, 0F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(5F, 5F, 5F),
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
childName = "RightLowerLeg",
localPos = new Vector3(-1.85576F, 5.65054F, -0.08062F),
localAngles = new Vector3(75.31042F, 99.00003F, 0.00002F),
localScale = new Vector3(1F, 1F, 1F),
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
childName = "Chest",
localPos = new Vector3(0.45347F, 1.55566F, 1.63821F),
localAngles = new Vector3(316.7962F, 15.1151F, 336.1019F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
childName = "LeftUpperArm",
localPos = new Vector3(1.50405F, 0.63478F, 1.55301F),
localAngles = new Vector3(0.16547F, 45.93884F, 182.107F),
localScale = new Vector3(5F, 5F, 1F),
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
childName = "RightUpperArm",
localPos = new Vector3(0.35606F, -0.71047F, -0.30192F),
localAngles = new Vector3(0F, 180F, 180F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
childName = "Head",
localPos = new Vector3(0.93366F, 0.5014F, -0.0193F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(5F, 5F, 5F),
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
childName = "RightLowerArm",
localPos = new Vector3(0.73586F, 3.38263F, -0.45336F),
localAngles = new Vector3(278.5149F, 14.42831F, 344.4992F),
localScale = new Vector3(5F, 5F, 5F),
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
childName = "Hips",
localPos = new Vector3(-2.84407F, 1.33546F, 0F),
localAngles = new Vector3(0F, 90F, 90F),
localScale = new Vector3(2F, 2F, 2F),
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
childName = "Chest",
localPos = new Vector3(-2.68256F, 0.19584F, -0.10739F),
localAngles = new Vector3(293.759F, 180F, 121.9427F),
localScale = new Vector3(2.5F, 2.5F, 2.5F),
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
childName = "Chest",
localPos = new Vector3(2.22056F, 1.26752F, -0.39332F),
localAngles = new Vector3(87.35188F, 61.70193F, 15.05279F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
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
childName = "Chest",
localPos = new Vector3(2.48833F, 1.25452F, -0.00002F),
localAngles = new Vector3(0F, 0F, 319.5971F),
localScale = new Vector3(1F, 1F, 1F),
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
childName = "RightUpperArm",
localPos = new Vector3(0.99294F, 0.04158F, -0.98957F),
localAngles = new Vector3(87.08234F, 336.6856F, 21.84134F),
localScale = new Vector3(5F, 5F, 5F),
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
childName = "Head",
localPos = new Vector3(0.32287F, 1.10795F, -0.08135F),
localAngles = new Vector3(352.8997F, 267.0539F, 359.5399F),
localScale = new Vector3(7F, 7F, 7F),
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
childName = "LeftUpperArm",
localPos = new Vector3(1.62177F, -2.41557F, 1.4972F),
localAngles = new Vector3(78.51366F, 60.79132F, 20.39089F),
localScale = new Vector3(5F, 5F, 5F),
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
childName = "Chest",
localPos = new Vector3(-2.39661F, 0.25476F, -0.00003F),
localAngles = new Vector3(0F, 270F, 0F),
localScale = new Vector3(1F, 1F, 1F),
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
childName = "RightUpperLeg",
localPos = new Vector3(0F, 3F, -0.15F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(4F, 4F, 8F),
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
childName = "LeftUpperLeg",
localPos = new Vector3(-0.00001F, 3.01942F, 0.15589F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(4F, 4F, 8F),
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
                            childName = "Chest",
localPos = new Vector3(-1.97574F, 1.11178F, 1.11929F),
localAngles = new Vector3(0F, 0F, 270F),
localScale = new Vector3(2F, 2F, 2F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAfterburnerShoulderRing"),
                            childName = "Chest",
localPos = new Vector3(-1.97574F, 1.11178F, 1.11929F),
localAngles = new Vector3(0F, 0F, 270F),
localScale = new Vector3(2F, 2F, 2F),
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
childName = "Chest",
localPos = new Vector3(0.4639F, 1.09201F, -1.7629F),
localAngles = new Vector3(0F, 0F, 0F),
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
childName = "Chest",
localPos = new Vector3(-2.70613F, -1.77582F, 2.63377F),
localAngles = new Vector3(36.73067F, 0.97875F, 83.07793F),
localScale = new Vector3(3F, 3F, 3F),
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
childName = "Chest",
localPos = new Vector3(1.89405F, 1.6083F, -0.00001F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
childName = "LeftLowerLeg",
localPos = new Vector3(-0.78161F, 3.7626F, 0.63738F),
localAngles = new Vector3(86.01985F, 90F, 0F),
localScale = new Vector3(5F, 5F, 5F),
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
childName = "RightUpperArm",
localPos = new Vector3(0.12685F, -1.61732F, -0.18251F),
localAngles = new Vector3(20.74401F, 43.63619F, 185.9096F),
localScale = new Vector3(4F, 4F, 4F),
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
childName = "Head",
localPos = new Vector3(-0.35043F, 1.11615F, 0.70182F),
localAngles = new Vector3(8.54165F, 22.14914F, 22.08137F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
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
childName = "Chest",
localPos = new Vector3(-2.64703F, 0.26602F, -0.05729F),
localAngles = new Vector3(43.53754F, 2.27878F, 0F),
localScale = new Vector3(4F, 4F, 4F),
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
childName = "RightLowerLeg",
localPos = new Vector3(-0.18237F, 5.79009F, -0.22755F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(4.5F, 4.5F, 4.5F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGravBoots"),
childName = "LeftLowerLeg",
localPos = new Vector3(-0.18237F, 5.79009F, 0.22755F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(4.5F, 4.5F, 4.5F),
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
childName = "Hips",
localPos = new Vector3(0F, -1.16505F, 0F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(2F, 2F, 2F),
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
childName = "Chest",
localPos = new Vector3(-2.02111F, 0.52713F, 1.93567F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
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
childName = "Head",
localPos = new Vector3(1F, 1F, -0.5F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(3F, 3F, 3F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDevilHorns"),
childName = "Head",
localPos = new Vector3(1F, 1F, 0.5F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(-3F, 3F, 3F),
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
childName = "LeftUpperLeg",
localPos = new Vector3(-0.94065F, -0.70893F, 1.24431F),
localAngles = new Vector3(14.28351F, 334.6211F, 179.7242F),
localScale = new Vector3(5F, 5F, 5F),
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
childName = "Chest",
localPos = new Vector3(0.81433F, 1.4449F, -1.6313F),
localAngles = new Vector3(1.39814F, 99.65938F, 162.2404F),
localScale = new Vector3(0.4907F, 0.4907F, 0.4907F),
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
childName = "Hips",
localPos = new Vector3(0.31051F, 1.43262F, 0F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(4.5F, 6F, 6F),
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
childName = "Chest",
localPos = new Vector3(-1.4323F, 2.2771F, -0.00001F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(3F, 3F, 3F),
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
childName = "Chest",
localPos = new Vector3(0.56382F, 1.43956F, -1.81764F),
localAngles = new Vector3(0F, 45F, 90F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
childName = "RightUpperLeg",
localPos = new Vector3(1.06613F, -0.04682F, -1.44357F),
localAngles = new Vector3(85.53331F, 0.00015F, 90.94305F),
localScale = new Vector3(5F, 5F, 5F),
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
childName = "Chest",
localPos = new Vector3(0F, 1.85247F, 1.51001F),
localAngles = new Vector3(315.4147F, 0F, 0F),
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
childName = "Chest",
localPos = new Vector3(-2.66519F, -0.276F, 0.15548F),
localAngles = new Vector3(65.83244F, 180.8443F, 268.7882F),
localScale = new Vector3(5F, 5F, 5F),
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
localPos = new Vector3(0F, 0F, 0F),
localAngles = new Vector3(90F, 90F, 0F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
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
childName = "Hips",
localPos = new Vector3(-1.83498F, 0.12135F, -2.50923F),
localAngles = new Vector3(357.5902F, 90.39589F, 7.8145F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
childName = "Chest",
localPos = new Vector3(1.85404F, 1.40364F, 0.74283F),
localAngles = new Vector3(270F, 251.0168F, 0F),
localScale = new Vector3(0.1655F, 0.1655F, 0.1655F),
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
childName = "RightUpperLeg",
localPos = new Vector3(0.45727F, 0.25934F, -1.52907F),
localAngles = new Vector3(346.3716F, 62.99254F, 170.1769F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
childName = "LeftUpperLeg",
localPos = new Vector3(-1.84505F, -0.13206F, 1.03097F),
localAngles = new Vector3(356.2848F, 40.94352F, 27.72095F),
localScale = new Vector3(2F, 2F, 2F),
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
childName = "Chest",
localPos = new Vector3(1.75203F, 1.37724F, 1.05017F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
childName = "LeftUpperLeg",
localPos = new Vector3(0.72368F, -0.86803F, 1.29096F),
localAngles = new Vector3(71.55792F, 165.97F, 166.6641F),
localScale = new Vector3(5F, 5F, 5F),
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
childName = "LeftLowerLeg",
localPos = new Vector3(-0.04388F, -1.30355F, 0.44989F),
localAngles = new Vector3(0F, 180F, 270F),
localScale = new Vector3(2F, 2F, 2F),
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
childName = "Chest",
localPos = new Vector3(0.01313F, 1.64602F, -1.94366F),
localAngles = new Vector3(301.8074F, 67.30911F, 358.0348F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
childName = "Head",
localPos = new Vector3(0.3079F, 0.90178F, 0.12147F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(2F, 2F, 2F),
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
childName = "LeftUpperArm",
localPos = new Vector3(0.30385F, -1.29505F, 0.07366F),
localAngles = new Vector3(0F, 0F, 180F),
localScale = new Vector3(2F, 2F, 2F),
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
localPos = new Vector3(1.43423F, 0.73268F, 0F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
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
childName = "Head",
localPos = new Vector3(0.21829F, 0.63399F, -0.90918F),
localAngles = new Vector3(0F, 180F, 0F),
localScale = new Vector3(2F, 2F, 2F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAntler"),
childName = "Head",
localPos = new Vector3(0.21829F, 0.63399F, 0.90918F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(-2F, 2F, 2F),
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
childName = "Hips",
localPos = new Vector3(-2.2884F, 0.6078F, -1.81728F),
localAngles = new Vector3(359.414F, 211.0664F, 33.7113F),
localScale = new Vector3(1.5F, 1.5F, 1.5F),
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
childName = "Head",
localPos = new Vector3(-0.59563F, 0.63824F, -0.02709F),
localAngles = new Vector3(0F, 270F, 0F),
localScale = new Vector3(2F, 2F, 2F),
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
childName = "Hips",
localPos = new Vector3(2.38045F, 0.00026F, -0.00001F),
localAngles = new Vector3(90F, 90F, 0F),
localScale = new Vector3(5F, 5F, 5F),
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
childName = "RightUpperLeg",
localPos = new Vector3(-1.7728F, -1.16548F, -0.1027F),
localAngles = new Vector3(3.54275F, 239.377F, 0.35598F),
localScale = new Vector3(3F, 3F, 3F),
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
childName = "Chest",
localPos = new Vector3(1.5379F, 0.26637F, 1.58031F),
localAngles = new Vector3(0.00001F, 107.2003F, 57.13564F),
localScale = new Vector3(2F, 2F, 2F),
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
childName = "RightLowerLeg",
localPos = new Vector3(0.26577F, 7.16907F, -0.44559F),
localAngles = new Vector3(75.57262F, 281.4891F, 281.2034F),
localScale = new Vector3(9F, 9F, 8F),
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
childName = "Chest",
localPos = new Vector3(1.65133F, 0.20495F, 0.96824F),
localAngles = new Vector3(270F, 47.30325F, 0F),
localScale = new Vector3(2F, 2F, 2F),
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
childName = "LeftLowerArm",
localPos = new Vector3(0.06011F, 2.21046F, -0.71548F),
localAngles = new Vector3(0F, 359.7433F, 0F),
localScale = new Vector3(1F, 1F, 1F),
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
childName = "RightLowerArm",
localPos = new Vector3(-0.42765F, 2.67078F, 0.30986F),
localAngles = new Vector3(30.55176F, 310.6439F, 307.4006F),
localScale = new Vector3(15F, 15F, 15F),
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
childName = "Chest",
localPos = new Vector3(2.30173F, 1.22445F, 0.27342F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
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
childName = "Chest",
localPos = new Vector3(-0.04971F, 1.75452F, 1.76234F),
localAngles = new Vector3(347.5089F, 88.00771F, 0F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
childName = "Head",
localPos = new Vector3(0.50866F, 0.448F, 0F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
childName = "Chest",
localPos = new Vector3(0.62891F, 1.49153F, 1.69884F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.05F, 0.05F, 0.05F),
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
childName = "Chest",
localPos = new Vector3(0.84903F, 1.40141F, -1.7617F),
localAngles = new Vector3(357.2213F, 357.3524F, 112.2944F),
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
childName = "RightUpperLeg",
localPos = new Vector3(-0.42084F, -0.00242F, -1.42974F),
localAngles = new Vector3(2.48716F, 280.3047F, 192.003F),
localScale = new Vector3(2F, 2F, 2F),
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
childName = "Head",
localPos = new Vector3(0.85741F, 0.40012F, 0.00001F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(1.5F, 1.5F, 1.5F),
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
childName = "LeftLowerLeg",
localPos = new Vector3(0.85211F, 0.16847F, 0.21644F),
localAngles = new Vector3(78.58171F, 90F, 180F),
localScale = new Vector3(2F, 2F, 2F),
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
childName = "RightLowerLeg",
localPos = new Vector3(0.88613F, -0.00001F, -0.20246F),
localAngles = new Vector3(90F, 270F, 0F),
localScale = new Vector3(2F, 2F, 2F),
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
childName = "Chest",
localPos = new Vector3(-2.57449F, 0F, 0F),
localAngles = new Vector3(0F, 0F, 90F),
localScale = new Vector3(2F, 2F, 2F),
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
childName = "Head",
localPos = new Vector3(0.35392F, 2.05664F, -0.52755F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(2F, 2F, 2F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayShieldBug"),
childName = "Head",
localPos = new Vector3(0.35392F, 2.05664F, 0.52755F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(2F, 2F, -2F),
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
childName = "Hips",
localPos = new Vector3(-2.05413F, 0.50539F, -1.9063F),
localAngles = new Vector3(297.6831F, 36.17311F, 244.2753F),
localScale = new Vector3(5F, 5F, 5F),
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
childName = "Hips",
localPos = new Vector3(0.65172F, -0.54133F, 0.01868F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(5F, 2F, 2F),
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
childName = "Head",
localPos = new Vector3(1.52031F, 1.44749F, -0.6584F),
localAngles = new Vector3(342.1512F, 276.0559F, 301.1084F),
localScale = new Vector3(3F, 3F, 3F),
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
childName = "Hips",
localPos = new Vector3(1.44052F, 2.82637F, -0.00001F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(1F, 1F, 1F),
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
childName = "Chest",
localPos = new Vector3(1.65123F, -0.96238F, 0.70402F),
localAngles = new Vector3(359.6064F, 48.96481F, 356.0063F),
localScale = new Vector3(30F, 30F, 30F),
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
localPos = new Vector3(0F, 0F, 0F),
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
localPos = new Vector3(0F, 0F, 0F),
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
childName = "Chest",
localPos = new Vector3(0.32238F, 1.46415F, 1.79202F),
localAngles = new Vector3(281.0408F, 60.27886F, 119.2289F),
localScale = new Vector3(0.0475F, 0.0475F, 0.0475F),
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
childName = "RightLowerLeg",
localPos = new Vector3(-1.66608F, -0.10859F, -0.17611F),
localAngles = new Vector3(0F, 0F, 90F),
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
childName = "Hips",
localPos = new Vector3(0F, -12.86282F, 0F),
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
localPos = new Vector3(0.00004F, 4.35816F, -1.73298F),
localAngles = new Vector3(0F, 90F, 0F),
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
childName = "Chest",
localPos = new Vector3(2.02716F, 1.67941F, -0.00001F),
localAngles = new Vector3(315F, 90F, 0F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
localPos = new Vector3(0.00003F, 4.38936F, 0F),
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
childName = "LeftUpperArm",
localPos = new Vector3(0.00014F, -0.77839F, 0.00009F),
localAngles = new Vector3(0F, 309.4523F, 0F),
localScale = new Vector3(0.55F, 0.5F, 0.55F),
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
localPos = new Vector3(1.95004F, 0.68158F, 0F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(0.32F, 0.3F, 0.3F),
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
childName = "Chest",
localPos = new Vector3(1.80948F, 1.38222F, 0.84485F),
localAngles = new Vector3(43.18613F, 228.8516F, 328.4307F),
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
childName = "RightUpperArm",
localPos = new Vector3(0.00002F, 1.42361F, 0.56711F),
localAngles = new Vector3(0F, 0F, 180F),
localScale = new Vector3(1F, 1F, 1F),
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
childName = "Chest",
localPos = new Vector3(0.61755F, 1.57069F, -1.55607F),
localAngles = new Vector3(315.59F, 47.91866F, 47.9424F),
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
childName = "Hips",
localPos = new Vector3(-3.58319F, -0.55045F, 0.01161F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(1F, 1F, 1F),
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
childName = "LeftUpperLeg",
localPos = new Vector3(0.47562F, -0.01526F, -1.73508F),
localAngles = new Vector3(1.78675F, 33.05307F, 146.9104F),
localScale = new Vector3(2F, 2F, 2F),
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
childName = "Head",
localPos = new Vector3(0F, 0F, 0F),
localAngles = new Vector3(0F, 270F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteHorn"),
childName = "Head",
localPos = new Vector3(0F, 0F, 0F),
localAngles = new Vector3(0F, 270F, 0F),
localScale = new Vector3(-1F, 1F, 1F),
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
childName = "Head",
localPos = new Vector3(0F, 0.2648F, 0F),
localAngles = new Vector3(315F, 90F, 0F),
localScale = new Vector3(2.5F, 2.5F, 2.5F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteRhinoHorn"),
childName = "Head",
localPos = new Vector3(0F, 0.3022F, 0F),
localAngles = new Vector3(300F, 90F, 0F),
localScale = new Vector3(2F, 2F, 2F),
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
childName = "Head",
localPos = new Vector3(0.2724F, 2.04752F, 0F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.2F, 0.2F, 0.2F),
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
childName = "Head",
localPos = new Vector3(0.22044F, 1.92486F, 0.06411F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
childName = "Head",
localPos = new Vector3(0.2511F, 1.77063F, 0.00253F),
localAngles = new Vector3(270F, 90F, 0F),
localScale = new Vector3(0.35F, 0.35F, 0.35F),
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
childName = "Chest",
localPos = new Vector3(3.02969F, 0.0984F, -0.85169F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(1F, 1F, 1F),
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
childName = "Chest",
localPos = new Vector3(1.78461F, 1.41871F, -0.81773F),
localAngles = new Vector3(314.4275F, 84.51066F, 12.5073F),
localScale = new Vector3(0.2641F, 0.2641F, 0.2641F),
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
childName = "LeftUpperLeg",
localPos = new Vector3(-2.22089F, -0.05762F, 0.91054F),
localAngles = new Vector3(313.2616F, 340.1821F, 185.1352F),
localScale = new Vector3(3F, 3F, 3F),
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
childName = "Chest",
localPos = new Vector3(1.85012F, 1.39333F, -0.80992F),
localAngles = new Vector3(357.4099F, 358.9051F, 24.20235F),
localScale = new Vector3(0.05F, 0.05F, 0.05F),
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
childName = "Chest",
localPos = new Vector3(1.70865F, 1.40454F, -1.06433F),
localAngles = new Vector3(51.64201F, 281.457F, 10.09651F),
localScale = new Vector3(1F, 1F, 1F),
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
childName = "Chest",
localPos = new Vector3(-1.33597F, 1.91072F, -1.44748F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
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
childName = "Chest",
localPos = new Vector3(1.94142F, 1.42114F, -0.86892F),
localAngles = new Vector3(7.84847F, 26.69731F, 359.0494F),
localScale = new Vector3(1F, 1F, 1F),
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
childName = "Chest",
localPos = new Vector3(-1.12448F, 1.86731F, -1.37409F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(0.3F, 0.3F, 0.3F),
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
childName = "Chest",
localPos = new Vector3(1.76384F, 1.40644F, -0.87008F),
localAngles = new Vector3(301.9247F, 284.344F, 87.41779F),
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
childName = "Chest",
localPos = new Vector3(-2.24341F, 0.09187F, 0F),
localAngles = new Vector3(0F, 270F, 0F),
localScale = new Vector3(1F, 1F, 1F),
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
childName = "Chest",
localPos = new Vector3(1.70251F, 1.45298F, -0.9388F),
localAngles = new Vector3(323.7031F, 107.4803F, 347.4525F),
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
childName = "Chest",
localPos = new Vector3(1.59739F, 1.59273F, -1.02687F),
localAngles = new Vector3(328.4292F, 127.3997F, 353.4717F),
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
localPos = new Vector3(0.00004F, 4.33704F, 1.95489F),
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
localPos = new Vector3(0.00008F, 4.33399F, 1.67836F),
localAngles = new Vector3(90F, 0F, 0F),
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
localPos = new Vector3(0.49837F, 4.20279F, 1.74312F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
childName = "RightUpperArm",
localPos = new Vector3(-0.63562F, 0.05134F, 0.37374F),
localAngles = new Vector3(82.11106F, 48.32478F, 8.36399F),
localScale = new Vector3(2.5F, 2.5F, 2.5F),
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
childName = "Chest",
localPos = new Vector3(1.76002F, 1.44636F, -0.88868F),
localAngles = new Vector3(297.6495F, 97.4698F, 353.9084F),
localScale = new Vector3(0.0596F, 0.0596F, 0.0596F),
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
childName = "LeftLowerArm",
localPos = new Vector3(-1.24846F, 0.68043F, 1.36405F),
localAngles = new Vector3(27.6094F, 125.9901F, 94.61322F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
childName = "Chest",
localPos = new Vector3(1.76677F, 1.48162F, -1.02178F),
localAngles = new Vector3(0.50217F, 114.6188F, 358.9177F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
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
    }
}