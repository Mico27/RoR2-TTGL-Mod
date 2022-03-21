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
using TTGL_Survivor.UI;
using UnityEngine;

namespace TTGL_Survivor.Modules.Survivors
{
    public class Gurren: BaseSurvivor
    {
        internal static GameObject characterPrefab;
        internal static GameObject displayPrefab;
        internal static GameObject allyPrefab;

        internal static ConfigEntry<bool> characterEnabled;

        // item display stuffs
        internal static ItemDisplayRuleSet itemDisplayRuleSet { get; set; }
        internal static List<ItemDisplayRuleSet.KeyAssetRuleGroup> itemDisplayRules { get; set; }

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
                var entityStateMachine = characterPrefab.GetComponent<EntityStateMachine>();
                entityStateMachine.mainStateType = new SerializableEntityStateType(typeof(SkillStates.GurrenMain));
                entityStateMachine.initialStateType = new SerializableEntityStateType(typeof(SkillStates.GurrenEntering));
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
                //displayPrefab = CreateDisplayPrefab("GurrenMenuPrefab", characterPrefab);
                //var gurrenFoundUnlockable = Unlockables.AddUnlockable<GurrenFoundAchievement>(true);
                //RegisterNewSurvivor(characterPrefab, displayPrefab, new Color(0.25f, 0.65f, 0.25f), "GURREN", gurrenFoundUnlockable, 12.2f);
                CreateHurtBoxes();
                CreateHitboxes();
                CreateSkills();
                CreateSkins();
                InitializeItemDisplays();
                CreateGenericDoppelganger(characterPrefab, "GurrenMonsterMaster", "Merc");
                CreateAlly(characterPrefab, "GurrenAllyMaster", "Merc");
            }
        }

        protected override GameObject CreateDisplayPrefab(string modelName, GameObject prefab)
        {
            var commandoBody = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody");
            if (commandoBody == null)
            {
                TTGL_SurvivorPlugin.instance.Logger.LogError("Could not load Prefabs/CharacterBodies/CommandoBody");
            }
            GameObject newPrefab = PrefabAPI.InstantiateClone(commandoBody, modelName);

            GameObject model = CreateModel(newPrefab, modelName);

            Transform modelBaseTransform = SetupModel(newPrefab, model.transform, true);

            model.AddComponent<CharacterModel>().baseRendererInfos = prefab.GetComponentInChildren<CharacterModel>().baseRendererInfos;

            return model.gameObject;
        }

        protected override GameObject CreatePrefab(string bodyName, string modelName)
        {
            var commandoBody = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody");
            if (commandoBody == null)
            {
                TTGL_SurvivorPlugin.instance.Logger.LogError("Could not load Prefabs/CharacterBodies/CommandoBody");
            }
            GameObject newPrefab = PrefabAPI.InstantiateClone(commandoBody, bodyName);

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
            bodyComponent.portraitIcon = Modules.Assets.mainAssetBundle.LoadAsset<Texture>("GurrenIcon");

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

            bodyComponent.preferredPodPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
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
            modelBase.transform.localPosition = new Vector3(0f, -1f, 0f);
            modelBase.transform.localRotation = Quaternion.identity;
            modelBase.transform.localScale = new Vector3(1f, 1f, 1f);

            GameObject cameraPivot = new GameObject("CameraPivot");
            cameraPivot.transform.parent = modelBase.transform;
            cameraPivot.transform.localPosition = (isDisplay) ? new Vector3(0f, 2.6f, 0f): new Vector3(0f, 10.0f, 0f);
            cameraPivot.transform.localRotation = Quaternion.identity;
            cameraPivot.transform.localScale = Vector3.one;

            GameObject aimOrigin = new GameObject("AimOrigin");
            aimOrigin.transform.parent = modelBase.transform;
            aimOrigin.transform.localPosition = (isDisplay) ? new Vector3(0f, 2.6f, 0f) : new Vector3(0f, 10.0f, 0f);
            aimOrigin.transform.localRotation = Quaternion.identity;
            aimOrigin.transform.localScale = Vector3.one;
            prefab.GetComponent<CharacterBody>().aimOriginTransform = aimOrigin.transform;

            modelTransform.parent = modelBase.transform;
            modelTransform.localPosition = Vector3.zero;
            modelTransform.localRotation = Quaternion.identity;
            modelTransform.localScale = (isDisplay) ? new Vector3(0.2f, 0.2f, 0.2f) : new Vector3(0.8f, 0.8f, 0.8f);

            return modelBase.transform;
        }

        protected override void SetupCameraTargetParams(GameObject prefab)
        {
            CameraTargetParams cameraTargetParams = prefab.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraPivotTransform = prefab.transform.Find("ModelBase").Find("CameraPivot");
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.dontRaycastToPivot = false;
            var cameraParams = ScriptableObject.CreateInstance<CharacterCameraParams>();
            cameraParams.data.maxPitch = cameraTargetParams.cameraParams.data.maxPitch;
            cameraParams.data.minPitch = cameraTargetParams.cameraParams.data.minPitch;
            cameraParams.data.pivotVerticalOffset = cameraTargetParams.cameraParams.data.pivotVerticalOffset;
            cameraParams.data.idealLocalCameraPos = cameraTargetParams.cameraParams.data.idealLocalCameraPos.value * 2.2f;
            cameraParams.data.wallCushion = cameraTargetParams.cameraParams.data.wallCushion;
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
        
        private void CreateAlly(GameObject bodyPrefab, string masterName, string masterToCopy)
        {
            var prefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/" + masterToCopy + "MonsterMaster");
            if (prefab == null)
            {
                TTGL_SurvivorPlugin.instance.Logger.LogError("Could not load Prefabs/CharacterMasters/" + masterToCopy + "MonsterMaster");
            }
            allyPrefab = PrefabAPI.InstantiateClone(prefab, masterName);
            var characterMaster = allyPrefab.GetComponent<CharacterMaster>();
            characterMaster.bodyPrefab = bodyPrefab;
            characterMaster.name = masterName;
            allyPrefab.AddComponent<SetDontDestroyOnLoad>();
            TTGL_SurvivorPlugin.masterPrefabs.Add(allyPrefab);
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
            skillLocator.passiveSkill.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("KaminaSpiritIcon");

            #endregion

            #region Primary
            SkillDef tripleSlashSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            ((ScriptableObject)tripleSlashSkillDef).name = "GurrenTripleSlash";
            tripleSlashSkillDef.skillName = "GurrenTripleSlash";
            tripleSlashSkillDef.skillNameToken = prefix + "_GURREN_BODY_TRIPLESLASH_NAME";
            tripleSlashSkillDef.skillDescriptionToken = prefix + "_GURREN_BODY_TRIPLESLASH_DESCRIPTION";
            tripleSlashSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("TripleSlashIcon");
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
            SkillDef throwinShadesSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            ((ScriptableObject)throwinShadesSkillDef).name = "GurrenDrillBlaster";
            throwinShadesSkillDef.skillName = "GurrenDrillBlaster";
            throwinShadesSkillDef.skillNameToken = prefix + "_GURREN_BODY_DRILLBLASTER_NAME";
            throwinShadesSkillDef.skillDescriptionToken = prefix + "_GURREN_BODY_DRILLBLASTER_DESCRIPTION";
            throwinShadesSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("GurrenDrillBlasterIcon");
            throwinShadesSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenDrillBlaster));
            throwinShadesSkillDef.activationStateMachineName = "Weapon";
            throwinShadesSkillDef.baseMaxStock = 1;
            throwinShadesSkillDef.baseRechargeInterval = 1f;
            throwinShadesSkillDef.beginSkillCooldownOnSkillEnd = true;
            throwinShadesSkillDef.canceledFromSprinting = true;
            throwinShadesSkillDef.forceSprintDuringState = false;
            throwinShadesSkillDef.fullRestockOnAssign = true;
            throwinShadesSkillDef.interruptPriority = EntityStates.InterruptPriority.Skill;
            throwinShadesSkillDef.isCombatSkill = true;
            throwinShadesSkillDef.mustKeyPress = false;
            throwinShadesSkillDef.cancelSprintingOnActivation = true;
            throwinShadesSkillDef.rechargeStock = 1;
            throwinShadesSkillDef.requiredStock = 1;
            throwinShadesSkillDef.stockToConsume = 1;
            TTGL_SurvivorPlugin.skillDefs.Add(throwinShadesSkillDef);
            Modules.Skills.AddSecondarySkill(characterPrefab, throwinShadesSkillDef);

            #endregion

            #region Utility
            SkillDef rollSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            ((ScriptableObject)rollSkillDef).name = "GurrenRoll";
            rollSkillDef.skillName = "GurrenRoll";
            rollSkillDef.skillNameToken = prefix + "_GURREN_BODY_ROLL_NAME";
            rollSkillDef.skillDescriptionToken = prefix + "_GURREN_BODY_ROLL_DESCRIPTION";
            rollSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("RollIcon");
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

            GroundSkillDef rockThrowSkillDef = ScriptableObject.CreateInstance<GroundSkillDef>();
            ((ScriptableObject)rockThrowSkillDef).name = "GurrenLiftBoulder";
            rockThrowSkillDef.skillName = "GurrenLiftBoulder";
            rockThrowSkillDef.skillNameToken = prefix + "_GURREN_BODY_BOULDERTHROW_NAME";
            rockThrowSkillDef.skillDescriptionToken = prefix + "_GURREN_BODY_BOULDERTHROW_DESCRIPTION";
            rockThrowSkillDef.icon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("BoulderThrowIcon");
            rockThrowSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenLiftBoulder));
            rockThrowSkillDef.activationStateMachineName = "Body";
            rockThrowSkillDef.baseMaxStock = 1;
            rockThrowSkillDef.baseRechargeInterval = 12f;
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
        internal virtual void InitializeItemDisplays()
        {
            GameObject model = characterPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            itemDisplayRuleSet = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
            itemDisplayRuleSet.name = "idrs" + "Gurren";

            characterModel.itemDisplayRuleSet = itemDisplayRuleSet;
        }
        internal static void SetItemDisplays()
        {
            itemDisplayRules = new List<ItemDisplayRuleSet.KeyAssetRuleGroup>();

            // add item displays here
            //  HIGHLY recommend using KingEnderBrine's ItemDisplayPlacementHelper mod for this
            #region Item Displays

            if (Config.gurrenItemDisplayEnabled)
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
localPos = new Vector3(0F, 0F, 0F),
localAngles = new Vector3(0F, 0F, 0F),
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
childName = "RightUpperArm",
localPos = new Vector3(0.51858F, -3.18039F, 0.01764F),
localAngles = new Vector3(273.0313F, 163.032F, 18.49092F),
localScale = new Vector3(1F, 1F, 1F),
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
childName = "RightUpperArm",
localPos = new Vector3(-0.65484F, -0.81475F, 0.53579F),
localAngles = new Vector3(9.78661F, 131.1444F, 165.195F),
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
childName = "Chest",
localPos = new Vector3(0.02401F, 2.07608F, 2.03101F),
localAngles = new Vector3(0F, 0F, 180F),
localScale = new Vector3(3.8F, 3.5F, 3.5F),
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
localPos = new Vector3(-1.50878F, 3.54557F, 0.05221F),
localAngles = new Vector3(324.8086F, 21.45095F, 51.21915F),
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
localPos = new Vector3(-0.89716F, 3.42024F, 1.19888F),
localAngles = new Vector3(0.19094F, 325.989F, 0F),
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
localPos = new Vector3(0.5068F, -3.68617F, 0.73643F),
localAngles = new Vector3(358.322F, 249.8467F, 186.3914F),
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
childName = "Chest",
localPos = new Vector3(0.18576F, 1.00962F, -0.00004F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(7F, 7F, 7F),
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
localPos = new Vector3(-0.00005F, 3.98367F, -1.96123F),
localAngles = new Vector3(70.01967F, -0.00001F, 0.00002F),
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
localPos = new Vector3(0.90341F, 3.6487F, 1.63146F),
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
localPos = new Vector3(-1.28264F, 1.27855F, 1.20394F),
localAngles = new Vector3(0F, 315F, 182.0215F),
localScale = new Vector3(4F, 4F, 4F),
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
localPos = new Vector3(0.3562F, -1.08679F, 0.20365F),
localAngles = new Vector3(0F, 180F, 174.3962F),
localScale = new Vector3(0.3F, 0.3F, 0.3F),
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
childName = "Chest",
localPos = new Vector3(-0.03537F, 4.28434F, 0.89883F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
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
localPos = new Vector3(0.71342F, 3.23311F, 0.58454F),
localAngles = new Vector3(276.6561F, 232.5557F, 180.0001F),
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
localPos = new Vector3(0F, 3.08999F, -2.98603F),
localAngles = new Vector3(0F, 0F, 90F),
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
localPos = new Vector3(0.02948F, 0.3104F, -3.06322F),
localAngles = new Vector3(293.759F, 90F, 121.9427F),
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
localPos = new Vector3(0.7569F, 3.58418F, 1.8485F),
localAngles = new Vector3(87.35188F, 61.70191F, 15.0528F),
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
localPos = new Vector3(0.05246F, 3.4662F, 2.63464F),
localAngles = new Vector3(0F, 0F, 0F),
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
localPos = new Vector3(0.83997F, 0.80459F, 0.92443F),
localAngles = new Vector3(84.65411F, 62.85358F, 197.8814F),
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
childName = "Chest",
localPos = new Vector3(0.0217F, 3.65512F, 0.1752F),
localAngles = new Vector3(0F, 0F, 0F),
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
localPos = new Vector3(-1.40199F, -1.04376F, 1.51604F),
localAngles = new Vector3(83.73684F, 345.6811F, 34.59801F),
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
localPos = new Vector3(0F, 1.65958F, -2.50583F),
localAngles = new Vector3(0F, 180F, 0F),
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
localPos = new Vector3(0F, 1.42242F, -0.15589F),
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
localPos = new Vector3(0F, 1.42242F, 0.15589F),
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
localPos = new Vector3(-1.20927F, 2.71826F, -2.08394F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(2F, 2F, 2F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAfterburnerShoulderRing"),
                            childName = "Chest",
localPos = new Vector3(1.20927F, 2.71826F, -2.08394F),
localAngles = new Vector3(90F, 0F, 0F),
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
localPos = new Vector3(-0.05454F, 3.69128F, 1.7993F),
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
localPos = new Vector3(-2.64494F, -1.9897F, -2.7964F),
localAngles = new Vector3(36F, 270F, 90F),
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
localPos = new Vector3(0.02817F, 4.07092F, 1.334F),
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
localPos = new Vector3(-0.63694F, 3.06174F, -0.7657F),
localAngles = new Vector3(82.5377F, 353.4046F, 352.9645F),
localScale = new Vector3(5F, 5F, 4F),
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
localPos = new Vector3(0.36779F, -1.3025F, 0.16269F),
localAngles = new Vector3(6.24899F, 313.457F, 179.6768F),
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
childName = "Chest",
localPos = new Vector3(-1.35092F, 2.93435F, -1.25007F),
localAngles = new Vector3(358.3893F, 343.3838F, 46.11925F),
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
localPos = new Vector3(0F, 0.00012F, -2.77747F),
localAngles = new Vector3(315F, 90F, 0F),
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
childName = "FootR",
localPos = new Vector3(0F, 0.74403F, -0.10637F),
localAngles = new Vector3(20F, 0F, 0F),
localScale = new Vector3(4.5F, 4.5F, 4.5F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayGravBoots"),
childName = "FootL",
localPos = new Vector3(0F, 0.74403F, -0.10637F),
localAngles = new Vector3(20F, 0F, 0F),
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
localPos = new Vector3(0F, 0.52511F, 0F),
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
childName = "RightUpperLeg",
localPos = new Vector3(0.63147F, 0.37126F, 2.12815F),
localAngles = new Vector3(12.6131F, 44.82739F, 334.8642F),
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
childName = "Chest",
localPos = new Vector3(1.99591F, 2.88119F, 0.18283F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(5F, 5F, 5F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayDevilHorns"),
childName = "Chest",
localPos = new Vector3(-1.99591F, 2.88119F, 0.18283F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(-5F, 5F, 5F),
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
localPos = new Vector3(-1.16688F, -0.66383F, -0.50019F),
localAngles = new Vector3(11.73625F, 256.742F, 180.9196F),
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
localPos = new Vector3(-0.8739F, 3.52046F, 1.75697F),
localAngles = new Vector3(1.39814F, 99.65939F, 162.2404F),
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
localPos = new Vector3(0.31179F, 2.2745F, 0.44931F),
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
localPos = new Vector3(0F, 3.70872F, -1.41245F),
localAngles = new Vector3(0F, 0F, 0F),
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
localPos = new Vector3(-0.77328F, 3.59008F, 2.01642F),
localAngles = new Vector3(0.00001F, 45.00001F, 109.544F),
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
localPos = new Vector3(0.97176F, -0.41057F, 1.69524F),
localAngles = new Vector3(75.42065F, 210.8146F, 32.44687F),
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
localPos = new Vector3(1.7381F, 3.72577F, 0.07384F),
localAngles = new Vector3(313.6187F, 94.96318F, 2.27554F),
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
localPos = new Vector3(-0.32468F, -0.4723F, -3.06147F),
localAngles = new Vector3(66.59507F, 93.07054F, 274.1107F),
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
childName = "Chest",
localPos = new Vector3(-0.00001F, 3.70056F, -0.00005F),
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
localPos = new Vector3(1.07478F, 3.51809F, 1.60998F),
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
localPos = new Vector3(1.17389F, -0.12687F, 0.96937F),
localAngles = new Vector3(350.4991F, 330.4795F, 173.1461F),
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
localPos = new Vector3(-0.92128F, -0.46047F, -1.45148F),
localAngles = new Vector3(336.4685F, 29.70164F, 5.06397F),
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
localPos = new Vector3(1.74288F, 3.31732F, 0.95472F),
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
localPos = new Vector3(-1.05743F, -0.6254F, 1.50959F),
localAngles = new Vector3(79.14478F, 73.9527F, 155.9872F),
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
childName = "LeftUpperLeg",
localPos = new Vector3(0.59001F, -2.01309F, 0.00407F),
localAngles = new Vector3(6.85184F, 120.2581F, 259.2396F),
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
localPos = new Vector3(-0.99768F, 3.70181F, 1.43591F),
localAngles = new Vector3(302.7343F, 340.1493F, 354.9026F),
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
childName = "Chest",
localPos = new Vector3(-0.00001F, 3.50352F, 0.51558F),
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
childName = "Chest",
localPos = new Vector3(-0.01427F, 3.85506F, 2.01154F),
localAngles = new Vector3(359.9824F, 89.87183F, 321.3782F),
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
childName = "Chest",
localPos = new Vector3(-0.00002F, 3.77797F, -0.00001F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(2F, 2F, 2F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayAntler"),
childName = "Chest",
localPos = new Vector3(0F, 3.86538F, -0.00001F),
localAngles = new Vector3(0F, 270F, 0F),
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
childName = "Chest",
localPos = new Vector3(0F, 4.48825F, -0.00018F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
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
localPos = new Vector3(0.00101F, 1.54696F, 2.23921F),
localAngles = new Vector3(90F, 0F, 0F),
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
childName = "LeftLowerArm",
localPos = new Vector3(-0.78766F, 0.47987F, -0.73123F),
localAngles = new Vector3(3.53197F, 226.3589F, 359.5493F),
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
localPos = new Vector3(-1.63158F, 2.08809F, 1.98858F),
localAngles = new Vector3(0F, 0F, 0F),
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
localPos = new Vector3(-0.98765F, 2.01388F, 2.07206F),
localAngles = new Vector3(270F, 349.7873F, 0F),
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
localPos = new Vector3(1.32405F, 3.49259F, 1.53045F),
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
localPos = new Vector3(0.51837F, 3.86499F, 1.90248F),
localAngles = new Vector3(324.7246F, 8.33917F, 358.4588F),
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
childName = "Chest",
localPos = new Vector3(0.64801F, 4.08438F, 1.28797F),
localAngles = new Vector3(291.7788F, 203.6291F, 180F),
localScale = new Vector3(0.12F, 0.12F, 0.12F),
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
localPos = new Vector3(0.18006F, 3.71207F, 2.18215F),
localAngles = new Vector3(0F, 0F, 353.9816F),
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
localPos = new Vector3(1.22322F, 3.45491F, 1.71664F),
localAngles = new Vector3(327.8379F, 356.7745F, 114.6539F),
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
localPos = new Vector3(0.85236F, -0.55697F, -1.45513F),
localAngles = new Vector3(12.20064F, 207.2863F, 199.0443F),
localScale = new Vector3(2F, 2F, 2F),
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
localPos = new Vector3(-0.00002F, 0.02425F, 0.90593F),
localAngles = new Vector3(270F, 0F, 0F),
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
localPos = new Vector3(0F, -0.00002F, 0.31983F),
localAngles = new Vector3(270F, 0F, 0F),
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
childName = "LeftUpperLeg",
localPos = new Vector3(-0.67496F, 0.30953F, 2.20585F),
localAngles = new Vector3(25.6284F, 51.22541F, 99.67564F),
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
childName = "Chest",
localPos = new Vector3(0.8F, 3F, 0F),
localAngles = new Vector3(0F, 270F, 0F),
localScale = new Vector3(2F, 2F, 2F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayShieldBug"),
childName = "Chest",
localPos = new Vector3(-0.8F, 3F, 0F),
localAngles = new Vector3(0F, 270F, 0F),
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
localPos = new Vector3(0.01824F, 3.49579F, -0.4537F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(5F, 1.3F, 1F),
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
childName = "RightUpperLeg",
localPos = new Vector3(1.20759F, -0.6249F, -0.48307F),
localAngles = new Vector3(6.91825F, 121.5033F, 191.038F),
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
localPos = new Vector3(0F, 4.34768F, 1.50556F),
localAngles = new Vector3(270F, 0F, 0F),
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
localPos = new Vector3(-0.84267F, 0.67364F, 1.61309F),
localAngles = new Vector3(359.6064F, 48.96481F, 356.0063F),
localScale = new Vector3(32F, 32F, 32F),
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
childName = "Chest",
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
childName = "Chest",
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
localPos = new Vector3(0.31685F, 3.6608F, 2.07911F),
localAngles = new Vector3(281.0408F, 60.27885F, 119.2289F),
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
childName = "RightUpperLeg",
localPos = new Vector3(0.03448F, -1.2145F, -1.82992F),
localAngles = new Vector3(356.8618F, 243.0631F, 101.3187F),
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
localPos = new Vector3(0F, -7.22622F, 0F),
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
childName = "Chest",
localPos = new Vector3(1.91263F, 7.34556F, -0.35705F),
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
childName = "RightLowerArm",
localPos = new Vector3(0.88715F, 0.49863F, -0.83589F),
localAngles = new Vector3(0F, 315F, 0F),
localScale = new Vector3(0.8F, 0.8F, 0.8F),
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
childName = "Chest",
localPos = new Vector3(0.00003F, 5.51843F, 0.00002F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.1F, 0.1F, 0.1F),
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
childName = "Chest",
localPos = new Vector3(0.75556F, 0.60683F, 1.66437F),
localAngles = new Vector3(90F, 90F, 0F),
localScale = new Vector3(0.5F, 0.5F, 0.5F),
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
localPos = new Vector3(1.42503F, 3.39451F, 1.17107F),
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
localPos = new Vector3(-0.52042F, 3.77422F, 1.85749F),
localAngles = new Vector3(25.13319F, 46.62719F, 304.4857F),
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
localPos = new Vector3(0F, 1.29609F, -3.55601F),
localAngles = new Vector3(0F, 0F, 0F),
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
localPos = new Vector3(1.51133F, 0.69286F, 0.25632F),
localAngles = new Vector3(4.82647F, 309.5198F, 157.597F),
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
childName = "Chest",
localPos = new Vector3(1.00202F, 2.85193F, 1.22385F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteHorn"),
childName = "Chest",
localPos = new Vector3(-1.00202F, 2.85193F, 1.22385F),
localAngles = new Vector3(90F, 0F, 0F),
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
childName = "Chest",
localPos = new Vector3(0.01316F, 2.88209F, 2.57636F),
localAngles = new Vector3(315F, 0F, 0F),
localScale = new Vector3(2.5F, 2.5F, 2.5F),
                            limbMask = LimbFlags.None
                        },
                        new ItemDisplayRule
                        {
                            ruleType = ItemDisplayRuleType.ParentedPrefab,
                            followerPrefab = ItemDisplays.LoadDisplay("DisplayEliteRhinoHorn"),
childName = "Chest",
localPos = new Vector3(0.01161F, 3.14621F, 2.19975F),
localAngles = new Vector3(300F, 0F, 0F),
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
childName = "Chest",
localPos = new Vector3(0F, 6.32027F, 0F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(0.4F, 0.4F, 0.4F),
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
childName = "Chest",
localPos = new Vector3(0.21148F, 3.72826F, 0.14519F),
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
childName = "Chest",
localPos = new Vector3(-0.0185F, 3.93018F, 0.52878F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(0.7F, 0.7F, 0.7F),
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
localPos = new Vector3(0.98839F, 2.17633F, 3.14286F),
localAngles = new Vector3(0F, 0F, 0F),
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
localPos = new Vector3(-0.2707F, 3.72508F, 2.01849F),
localAngles = new Vector3(316.5517F, 353.9519F, 10.34232F),
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
childName = "LeftLowerArm",
localPos = new Vector3(1.02424F, 1.32331F, 0.84212F),
localAngles = new Vector3(298.8045F, 87.32227F, 193.5201F),
localScale = new Vector3(2F, 2F, 2F),
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
localPos = new Vector3(-0.454F, 3.68357F, 2.08656F),
localAngles = new Vector3(359.1644F, 2.83189F, 330.9473F),
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
localPos = new Vector3(-0.31504F, 3.63233F, 2.25855F),
localAngles = new Vector3(71.06286F, 190.4599F, 344.2984F),
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
localPos = new Vector3(1.71185F, 3.34646F, -1.28937F),
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
localPos = new Vector3(-0.44071F, 3.75247F, 2.07701F),
localAngles = new Vector3(352.8212F, 67.76262F, 4.5565F),
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
localPos = new Vector3(1.58841F, 3.44211F, -1.17854F),
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
localPos = new Vector3(-0.33804F, 3.69359F, 2.01736F),
localAngles = new Vector3(301.9247F, 284.344F, 87.41781F),
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
localPos = new Vector3(0F, -0.60865F, -2.66052F),
localAngles = new Vector3(0F, 180F, 0F),
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
localPos = new Vector3(-0.44064F, 3.7158F, 2.03215F),
localAngles = new Vector3(320.2693F, 359.5502F, 351.4159F),
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
localPos = new Vector3(-0.38493F, 3.88563F, 2.03467F),
localAngles = new Vector3(330.832F, 57.27143F, 353.8486F),
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
childName = "Chest",
localPos = new Vector3(-2.41302F, 7.11165F, 0.378F),
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
childName = "Chest",
localPos = new Vector3(-1.66906F, 7.09899F, 0.04934F),
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
childName = "Chest",
localPos = new Vector3(-1.74804F, 7.22914F, -0.04695F),
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
localPos = new Vector3(-0.77605F, 0.02417F, -0.03948F),
localAngles = new Vector3(82.11111F, 315F, 8.36402F),
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
localPos = new Vector3(-0.27342F, 3.78136F, 2.1013F),
localAngles = new Vector3(298.8091F, 10.24056F, 350.9224F),
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
childName = "LeftLowerArm",
localPos = new Vector3(1.45098F, 0.4183F, 1.0551F),
localAngles = new Vector3(7.88983F, 240.8824F, 119.2776F),
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
localPos = new Vector3(-0.347F, 3.76455F, 2.1252F),
localAngles = new Vector3(358.8325F, 11.6212F, 11.31001F),
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