using BepInEx.Configuration;
using KinematicCharacterController;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TTGL_Survivor.Modules.Components;
using TTGL_Survivor.UI;
using UnityEngine;

namespace TTGL_Survivor.Modules.Survivors
{
    public class Gurren: BaseSurvivor
    {
        internal static GameObject characterPrefab;
        internal static GameObject displayPrefab;

        internal static ConfigEntry<bool> characterEnabled;
        
        // item display stuffs
        internal static ItemDisplayRuleSet itemDisplayRuleSet;
        internal static List<ItemDisplayRuleSet.NamedRuleGroup> itemRules;
        internal static List<ItemDisplayRuleSet.NamedRuleGroup> equipmentRules;

        public void CreateCharacter()
        {
            // this creates a config option to enable the character- feel free to remove if the character is the only thing in your mod
            //characterEnabled = Modules.Config.CharacterEnableConfig("Gurren");

            if (true)//characterEnabled.Value)
            {
                #region Body
                characterPrefab = CreatePrefab("GurrenBody", "GurrenPrefab");
                //Setup spiritEnergy components
                characterPrefab.AddComponent<TTGLMusicRemote>();
                characterPrefab.AddComponent<GurrenController>();
                characterPrefab.GetComponent<EntityStateMachine>().mainStateType = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenMain));

                //Fix interaction distance because Gurren is too big
                var interactor = characterPrefab.GetComponent<Interactor>();
                interactor.maxInteractionDistance = 20f;
                #endregion

                #region Model

                SetupCharacterModel(characterPrefab, new CustomRendererInfo[] {
                new CustomRendererInfo
                {
                    childName = "Gurren",
                }}, 0);
                #endregion

                displayPrefab = CreateDisplayPrefab("GurrenMenuPrefab", characterPrefab);

                RegisterNewSurvivor(characterPrefab, displayPrefab, new Color(0.25f, 0.65f, 0.25f), "GURREN", "");

                CreateHurtBoxes();
                CreateHitboxes();
                CreateSkills();
                CreateSkins();
                CreateItemDisplays();
                CreateGenericDoppelganger(characterPrefab, "GurrenMonsterMaster", "Merc");
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
            
            TTGL_SurvivorPlugin.bodyPrefabs.Add(newPrefab);
            return newPrefab;
        }
        protected override void SetupCharacterBody(string bodyName, GameObject newPrefab, Transform modelBaseTransform)
        {
            CharacterBody bodyComponent = newPrefab.GetComponent<CharacterBody>();

            bodyComponent.bodyIndex = BodyIndex.None;
            bodyComponent.name = bodyName;
            bodyComponent.bodyColor = new Color(0.25f, 0.65f, 0.25f);
            bodyComponent.baseNameToken = TTGL_SurvivorPlugin.developerPrefix + "_GURREN_BODY_NAME";
            bodyComponent.subtitleNameToken = TTGL_SurvivorPlugin.developerPrefix + "_GURREN_BODY_SUBTITLE";
            bodyComponent.portraitIcon = Modules.Assets.mainAssetBundle.LoadAsset<Texture>("GurrenLagannIcon");
            bodyComponent.crosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");

            bodyComponent.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            bodyComponent.rootMotionInMainState = false;

            bodyComponent.baseMaxHealth = 220f;
            bodyComponent.levelMaxHealth = 66f;

            bodyComponent.baseRegen = 3.0f;
            bodyComponent.levelRegen = 0.6f;

            bodyComponent.baseMaxShield = 0f;
            bodyComponent.levelMaxShield = 0f;

            bodyComponent.baseMoveSpeed = 7f;
            bodyComponent.levelMoveSpeed = 0f;
            
            bodyComponent.baseAcceleration = 60f;

            bodyComponent.baseJumpPower = 20f;
            bodyComponent.levelJumpPower = 0f;

            bodyComponent.baseDamage = 12f;
            bodyComponent.levelDamage = 2.4f;

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
            motorComponent.mass = 1900f;
            KinematicCharacterMotor kinematicCharacterMotor = newPrefab.GetComponent<KinematicCharacterMotor>();
            if (kinematicCharacterMotor)
            {
                kinematicCharacterMotor.MaxStepHeight = 9f;
            }
        }

        protected override Transform SetupModel(GameObject prefab, Transform modelTransform, bool isDisplay)
        {
            GameObject modelBase = new GameObject("ModelBase");
            modelBase.transform.parent = prefab.transform;
            modelBase.transform.localPosition = new Vector3(0f, 0f, 0f);
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
            modelTransform.localScale = (isDisplay) ? new Vector3(0.1f, 0.1f, 0.1f) : new Vector3(0.8f, 0.8f, 0.8f);

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
            cameraParams.standardLocalCameraPos = cameraTargetParams.cameraParams.standardLocalCameraPos * 2.8f;
            cameraParams.wallCushion = cameraTargetParams.cameraParams.wallCushion;
            cameraTargetParams.cameraParams = cameraParams;
        }
        protected override void SetupRigidbody(GameObject prefab)
        {
            Rigidbody rigidbody = prefab.GetComponent<Rigidbody>();
            rigidbody.mass = 1900f;
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
                "HurtboxRightLowerArm",
                "HurtboxRightUpperArm",
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
                hurtbox.isBullseye = false;
                hurtbox.damageModifier = HurtBox.DamageModifier.Normal;
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

            string prefix = TTGL_SurvivorPlugin.developerPrefix;

            #region Passive

            SkillLocator skillLocator = characterPrefab.GetComponent<SkillLocator>();
            skillLocator.passiveSkill.enabled = true;
            skillLocator.passiveSkill.skillNameToken = prefix + "_GURREN_BODY_PASSIVE_NAME";
            skillLocator.passiveSkill.skillDescriptionToken = prefix + "_GURREN_BODY_PASSIVE_DESCRIPTION";
            skillLocator.passiveSkill.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("SpiralPowerIcon");

            #endregion

            #region Primary
            SkillDef tripleSlashSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            tripleSlashSkillDef.skillName = prefix + "_GURREN_BODY_TRIPLESLASH_NAME";
            tripleSlashSkillDef.skillNameToken = prefix + "_GURREN_BODY_TRIPLESLASH_NAME";
            tripleSlashSkillDef.skillDescriptionToken = prefix + "_GURREN_BODY_TRIPLESLASH_DESCRIPTION";
            tripleSlashSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("SpiralComboIcon");
            tripleSlashSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenTripleSlash));
            tripleSlashSkillDef.activationStateMachineName = "Body";
            tripleSlashSkillDef.baseMaxStock = 1;
            tripleSlashSkillDef.baseRechargeInterval = 0f;
            tripleSlashSkillDef.beginSkillCooldownOnSkillEnd = false;
            tripleSlashSkillDef.canceledFromSprinting = false;
            tripleSlashSkillDef.forceSprintDuringState = false;
            tripleSlashSkillDef.fullRestockOnAssign = true;
            tripleSlashSkillDef.interruptPriority = EntityStates.InterruptPriority.Any;
            tripleSlashSkillDef.cancelSprintingOnActivation = false;
            tripleSlashSkillDef.isCombatSkill = true;
            tripleSlashSkillDef.mustKeyPress = false;
            tripleSlashSkillDef.rechargeStock = 1;
            tripleSlashSkillDef.requiredStock = 0;
            tripleSlashSkillDef.stockToConsume = 0;
            TTGL_SurvivorPlugin.skillDefs.Add(tripleSlashSkillDef);
            Modules.Skills.AddPrimarySkill(characterPrefab, tripleSlashSkillDef);

            #endregion

            #region Secondary
            SkillDef diveBombSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            diveBombSkillDef.skillName = prefix + "_GURREN_BODY_DIVEBOMB_NAME";
            diveBombSkillDef.skillNameToken = prefix + "_GURREN_BODY_DIVEBOMB_NAME";
            diveBombSkillDef.skillDescriptionToken = prefix + "_GURREN_BODY_DIVEBOMB_DESCRIPTION";
            diveBombSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("GurrenLagannShadeThrowIcon");
            diveBombSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenLagannThrowingShades));
            diveBombSkillDef.activationStateMachineName = "Weapon";
            diveBombSkillDef.baseMaxStock = 1;
            diveBombSkillDef.baseRechargeInterval = 3f;
            diveBombSkillDef.beginSkillCooldownOnSkillEnd = false;
            diveBombSkillDef.canceledFromSprinting = false;
            diveBombSkillDef.forceSprintDuringState = false;
            diveBombSkillDef.fullRestockOnAssign = true;
            diveBombSkillDef.interruptPriority = EntityStates.InterruptPriority.Skill;
            diveBombSkillDef.isCombatSkill = true;
            diveBombSkillDef.mustKeyPress = false;
            diveBombSkillDef.cancelSprintingOnActivation = false;
            diveBombSkillDef.rechargeStock = 1;
            diveBombSkillDef.requiredStock = 1;
            diveBombSkillDef.stockToConsume = 1;
            TTGL_SurvivorPlugin.skillDefs.Add(diveBombSkillDef);
            Modules.Skills.AddSecondarySkill(characterPrefab, diveBombSkillDef);

            #endregion

            #region Utility
            SkillDef rollSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            rollSkillDef.skillName = prefix + "_GURREN_BODY_ROLL_NAME";
            rollSkillDef.skillNameToken = prefix + "_GURREN_BODY_ROLL_NAME";
            rollSkillDef.skillDescriptionToken = prefix + "_GURREN_BODY_ROLL_DESCRIPTION";
            rollSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("HuricaneKickIcon");
            rollSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenRoll));
            rollSkillDef.activationStateMachineName = "Body";
            rollSkillDef.baseMaxStock = 1;
            rollSkillDef.baseRechargeInterval = 4f;
            rollSkillDef.beginSkillCooldownOnSkillEnd = true;
            rollSkillDef.canceledFromSprinting = false;
            rollSkillDef.forceSprintDuringState = true;
            rollSkillDef.fullRestockOnAssign = true;
            rollSkillDef.interruptPriority = EntityStates.InterruptPriority.PrioritySkill;
            rollSkillDef.isCombatSkill = true;
            rollSkillDef.mustKeyPress = false;
            rollSkillDef.cancelSprintingOnActivation = false;
            rollSkillDef.rechargeStock = 1;
            rollSkillDef.requiredStock = 1;
            rollSkillDef.stockToConsume = 1;
            TTGL_SurvivorPlugin.skillDefs.Add(rollSkillDef);
            Modules.Skills.AddUtilitySkill(characterPrefab, rollSkillDef);

            #endregion

            #region Special

            SkillDef rockThrowSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            rockThrowSkillDef.skillName = prefix + "_GURREN_BODY_ROCKTHROW_NAME";
            rockThrowSkillDef.skillNameToken = prefix + "_GURREN_BODY_ROCKTHROW_NAME";
            rockThrowSkillDef.skillDescriptionToken = prefix + "_GURREN_BODY_ROCKTHROW_DESCRIPTION";
            rockThrowSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("GigaDrillMaximumIcon");
            rockThrowSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenLagannGigaDrillMaximum));
            rockThrowSkillDef.activationStateMachineName = "Body";
            rockThrowSkillDef.baseMaxStock = 1;
            rockThrowSkillDef.baseRechargeInterval = 8f;
            rockThrowSkillDef.beginSkillCooldownOnSkillEnd = true;
            rockThrowSkillDef.canceledFromSprinting = false;
            rockThrowSkillDef.forceSprintDuringState = false;
            rockThrowSkillDef.fullRestockOnAssign = true;
            rockThrowSkillDef.interruptPriority = EntityStates.InterruptPriority.Skill;
            rockThrowSkillDef.isCombatSkill = true;
            rockThrowSkillDef.mustKeyPress = false;
            rockThrowSkillDef.cancelSprintingOnActivation = true;
            rockThrowSkillDef.rechargeStock = 1;
            rockThrowSkillDef.requiredStock = 1;
            rockThrowSkillDef.stockToConsume = 1;
            TTGL_SurvivorPlugin.skillDefs.Add(rockThrowSkillDef);
            Modules.Skills.AddSpecialSkill(characterPrefab, rockThrowSkillDef);

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

        private void CreateItemDisplays()
        {
            GameObject model = characterPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            itemDisplayRuleSet = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();

            itemRules = new List<ItemDisplayRuleSet.NamedRuleGroup>();
            equipmentRules = new List<ItemDisplayRuleSet.NamedRuleGroup>();


            // add item displays here
            //  HIGHLY recommend using KingEnderBrine's ItemDisplayPlacementHelper mod for this

            ItemDisplayRuleSet.NamedRuleGroup[] item = itemRules.ToArray();
            ItemDisplayRuleSet.NamedRuleGroup[] equip = equipmentRules.ToArray();
            itemDisplayRuleSet.namedItemRuleGroups = item;
            itemDisplayRuleSet.namedEquipmentRuleGroups = equip;

            characterModel.itemDisplayRuleSet = null;
        }        
    }
}