using BepInEx.Configuration;
using RoR2;
using RoR2.Skills;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TTGL_Survivor.UI;
using UnityEngine;

namespace TTGL_Survivor.Modules.Survivors
{
    public class GurrenLagann: BaseSurvivor
    {
        internal static GameObject characterPrefab;
        internal static GameObject displayPrefab;

        internal static ConfigEntry<bool> characterEnabled;

        public const string bodyName = "GurrenLagannBody";

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
                characterPrefab = CreatePrefab(bodyName, "GurrenLagannPrefab", new BodyInfo
                {
                    armor = 20f,
                    armorGrowth = 0f,
                    bodyName = bodyName,
                    bodyNameToken = TTGL_SurvivorPlugin.developerPrefix + "_GURRENLAGANN_BODY_NAME",
                    characterPortrait = Modules.Assets.mainAssetBundle.LoadAsset<Texture>("LagannIcon"),
                    crosshair = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair"),
                    damage = 12f,
                    //crit = 100f,
                    healthGrowth = 33f,
                    healthRegen = 1.5f,
                    jumpCount = 1,
                    maxHealth = 110f,
                    subtitleNameToken = TTGL_SurvivorPlugin.developerPrefix + "_GURRENLAGANN_BODY_SUBTITLE",
                    podPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod")
                });
                //Setup spiritEnergy components
                characterPrefab.AddComponent<SpiralEnergyComponent>();
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
            cameraTargetParams.cameraParams = Resources.Load<GameObject>("Prefabs/CharacterBodies/MercBody").GetComponent<CameraTargetParams>().cameraParams;
            cameraTargetParams.cameraPivotTransform = prefab.transform.Find("ModelBase").Find("CameraPivot");
            cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.idealLocalCameraPos = Vector3.zero;
            cameraTargetParams.dontRaycastToPivot = false;
            cameraTargetParams.cameraParams.standardLocalCameraPos *= 3;
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
            Modules.Skills.AddPassiveSkill(characterPrefab, new SkillDefInfo
            {
                skillNameToken = prefix + "_GURRENLAGANN_BODY_PASSIVE_NAME",
                skillDescriptionToken = prefix + "_GURRENLAGANN_BODY_PASSIVE_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("SpiralPowerIcon"),
            });
            #endregion

            #region Primary

            SkillDef spiralingComboSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GURRENLAGANN_BODY_SPIRALINGCOMBO_NAME",
                skillNameToken = prefix + "_GURRENLAGANN_BODY_SPIRALINGCOMBO_NAME",
                skillDescriptionToken = prefix + "_GURRENLAGANN_BODY_SPIRALINGCOMBO_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("DrillRushIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenLagannSpiralingCombo)),
                activationStateMachineName = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Any,
                isBullets = false,
                isCombatSkill = true,
                mustKeyPress = false,
                noSprint = true,
                rechargeStock = 1,
                requiredStock = 0,
                shootDelay = 0f,
                stockToConsume = 0,
            });

            Modules.Skills.AddPrimarySkill(characterPrefab, spiralingComboSkillDef);

            #endregion

            #region Secondary
            SkillDef throwingShadesSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GURRENLAGANN_BODY_THROWINGSHADES_NAME",
                skillNameToken = prefix + "_GURRENLAGANN_BODY_THROWINGSHADES_NAME",
                skillDescriptionToken = prefix + "_GURRENLAGANN_BODY_THROWINGSHADES_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("YokoRifleIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenLagannThrowingShades)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 2,
                baseRechargeInterval = 3f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                isBullets = false,
                isCombatSkill = true,
                mustKeyPress = false,
                noSprint = false,
                rechargeStock = 2,
                requiredStock = 1,
                shootDelay = 0f,
                stockToConsume = 1,
                keywordTokens = new string [] { "KEYWORD_AGILE" }
            });

            Modules.Skills.AddSecondarySkill(characterPrefab, throwingShadesSkillDef);

            #endregion

            #region Utility
            SkillDef tornadoKickSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GURRENLAGANN_BODY_TORNADOKICK_NAME",
                skillNameToken = prefix + "_GURRENLAGANN_BODY_TORNADOKICK_NAME",
                skillDescriptionToken = prefix + "_GURRENLAGANN_BODY_TORNADOKICK_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("SpiralBurstIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenLagannTornadoKick)),
                activationStateMachineName = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = 4f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = true,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                isBullets = false,
                isCombatSkill = true,
                mustKeyPress = false,
                noSprint = false,
                rechargeStock = 1,
                requiredStock = 1,
                shootDelay = 0f,
                stockToConsume = 1
            });

            Modules.Skills.AddUtilitySkill(characterPrefab, tornadoKickSkillDef);

            #endregion

            #region Special

            SkillDef gigaDrillMaximumSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GURRENLAGANN_BODY_GIGADRILLMAXIMUM_NAME",
                skillNameToken = prefix + "_GURRENLAGANN_BODY_GIGADRILLMAXIMUM_NAME",
                skillDescriptionToken = prefix + "_GURRENLAGANN_BODY_GIGADRILLMAXIMUM_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("LagannImpactIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenLagannGigaDrillMaximum)),
                activationStateMachineName = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = 8f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                isBullets = false,
                isCombatSkill = true,
                mustKeyPress = false,
                noSprint = true,
                rechargeStock = 1,
                requiredStock = 1,
                shootDelay = 0f,
                stockToConsume = 1
            });

            Modules.Skills.AddSpecialSkill(characterPrefab, gigaDrillMaximumSkillDef);

            SkillDef gigaDrillBreakerSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_GURRENLAGANN_BODY_GIGADRILLBREAK_NAME",
                skillNameToken = prefix + "_GURRENLAGANN_BODY_GIGADRILLBREAK_NAME",
                skillDescriptionToken = prefix + "_GURRENLAGANN_BODY_GIGADRILLBREAK_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("LagannImpactIcon"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.GurrenLagannGigaDrillBreak)),
                activationStateMachineName = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = 60f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                isBullets = false,
                isCombatSkill = true,
                mustKeyPress = true,
                noSprint = true,
                rechargeStock = 1,
                requiredStock = 1,
                shootDelay = 0f,
                stockToConsume = 1
            });
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