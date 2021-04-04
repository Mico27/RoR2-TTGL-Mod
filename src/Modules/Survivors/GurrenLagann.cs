using BepInEx.Configuration;
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
    public class GurrenLagann: BaseSurvivor
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
            //characterEnabled = Modules.Config.CharacterEnableConfig("GurrenLagann");

            if (true)//characterEnabled.Value)
            {
                #region Body
                characterPrefab = CreatePrefab("GurrenLagannBody", "GurrenLagannPrefab");
                //Setup spiritEnergy components
                characterPrefab.AddComponent<SpiralEnergyComponent>();
                characterPrefab.AddComponent<GurrenLagannController>();
                characterPrefab.GetComponent<EntityStateMachine>().mainStateType = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenLagannMain));

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

                RegisterNewSurvivor(characterPrefab, displayPrefab, new Color(0.25f, 0.65f, 0.25f), "GURRENLAGANN", "");// TTGL_SurvivorPlugin.developerPrefix + "_HENRY_BODY_UNLOCKABLE_REWARD_ID");

                CreateHurtBoxes();
                CreateHitboxes();
                CreateSkills();
                CreateSkins();
                CreateItemDisplays();

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

            ContentPacks.bodyPrefabs.Add(newPrefab);
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

            bodyComponent.baseMaxHealth = 210f;
            bodyComponent.levelMaxHealth = 43f;

            bodyComponent.baseRegen = 2.5f;
            bodyComponent.levelRegen = 0.6f;

            bodyComponent.baseMaxShield = 0f;
            bodyComponent.levelMaxShield = 0f;

            bodyComponent.baseMoveSpeed = 7f;
            bodyComponent.levelMoveSpeed = 0f;
            
            bodyComponent.baseAcceleration = 40f;

            bodyComponent.baseJumpPower = 25f;
            bodyComponent.levelJumpPower = 0f;

            bodyComponent.baseDamage = 14f;
            bodyComponent.levelDamage = 2.8f;

            bodyComponent.baseAttackSpeed = 1f;
            bodyComponent.levelAttackSpeed = 0f;

            bodyComponent.baseArmor = 40f;
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
            spiralingComboSkillDef.skillName = prefix + "_GURRENLAGANN_BODY_SPIRALINGCOMBO_NAME";
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
            ContentPacks.skillDefs.Add(spiralingComboSkillDef);
            Modules.Skills.AddPrimarySkill(characterPrefab, spiralingComboSkillDef);

            #endregion

            #region Secondary
            SkillDef throwingShadesSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            throwingShadesSkillDef.skillName = prefix + "_GURRENLAGANN_BODY_THROWINGSHADES_NAME";
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
            ContentPacks.skillDefs.Add(throwingShadesSkillDef);
            Modules.Skills.AddSecondarySkill(characterPrefab, throwingShadesSkillDef);

            #endregion

            #region Utility
            SkillDef tornadoKickSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            tornadoKickSkillDef.skillName = prefix + "_GURRENLAGANN_BODY_TORNADOKICK_NAME";
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
            ContentPacks.skillDefs.Add(tornadoKickSkillDef);
            Modules.Skills.AddUtilitySkill(characterPrefab, tornadoKickSkillDef);

            #endregion

            #region Special

            SkillDef gigaDrillMaximumSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            gigaDrillMaximumSkillDef.skillName = prefix + "_GURRENLAGANN_BODY_GIGADRILLMAXIMUM_NAME";
            gigaDrillMaximumSkillDef.skillNameToken = prefix + "_GURRENLAGANN_BODY_GIGADRILLMAXIMUM_NAME";
            gigaDrillMaximumSkillDef.skillDescriptionToken = prefix + "_GURRENLAGANN_BODY_GIGADRILLMAXIMUM_DESCRIPTION";
            gigaDrillMaximumSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("GigaDrillMaximumIcon");
            gigaDrillMaximumSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenLagannGigaDrillMaximum));
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
            ContentPacks.skillDefs.Add(gigaDrillMaximumSkillDef);
            Modules.Skills.AddSpecialSkill(characterPrefab, gigaDrillMaximumSkillDef);

            SkillDef gigaDrillBreakerSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            gigaDrillBreakerSkillDef.skillName = prefix + "_GURRENLAGANN_BODY_GIGADRILLBREAK_NAME";
            gigaDrillBreakerSkillDef.skillNameToken = prefix + "_GURRENLAGANN_BODY_GIGADRILLBREAK_NAME";
            gigaDrillBreakerSkillDef.skillDescriptionToken = prefix + "_GURRENLAGANN_BODY_GIGADRILLBREAK_DESCRIPTION";
            gigaDrillBreakerSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("LagannImpactIcon");
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
            ContentPacks.skillDefs.Add(gigaDrillBreakerSkillDef);

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
            /*
            defaultSkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshHenrySword"),
                    renderer = defaultRenderers[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshHenryGun"),
                    renderer = defaultRenderers[1].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshHenry"),
                    renderer = defaultRenderers[bodyRendererIndex].renderer
                }
            };
            */
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