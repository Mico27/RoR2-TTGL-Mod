using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TTGL_Survivor.Modules
{
    public abstract class BaseSurvivor
    {
        // cache this just to give our ragdolls the same physic material as vanilla stuff
        private static PhysicMaterial ragdollMaterial;

        protected void RegisterNewSurvivor(GameObject bodyPrefab, GameObject displayPrefab, Color charColor, string namePrefix, UnlockableDef unlockableDef, float desiredSortPosition)
        {
            SurvivorDef survivorDef = ScriptableObject.CreateInstance<SurvivorDef>();
            survivorDef.cachedName = TTGL_SurvivorPlugin.developerPrefix + "_" + namePrefix + "_BODY_NAME";
            survivorDef.descriptionToken = TTGL_SurvivorPlugin.developerPrefix + "_" + namePrefix + "_BODY_DESCRIPTION";
            survivorDef.primaryColor = charColor;
            survivorDef.bodyPrefab = bodyPrefab;
            survivorDef.displayPrefab = displayPrefab;
            survivorDef.outroFlavorToken = TTGL_SurvivorPlugin.developerPrefix + "_" + namePrefix + "_BODY_OUTRO_FLAVOR";
            survivorDef.desiredSortPosition = desiredSortPosition;
            survivorDef.unlockableDef = unlockableDef;
            TTGL_SurvivorPlugin.survivorDefinitions.Add(survivorDef);
        }

        protected abstract GameObject CreateDisplayPrefab(string modelName, GameObject prefab);

        protected abstract GameObject CreatePrefab(string bodyName, string modelName);

        protected virtual void SetupCharacterBody(string bodyName, GameObject newPrefab, Transform modelBaseTransform)
        {
            CharacterBody bodyComponent = newPrefab.GetComponent<CharacterBody>();

            bodyComponent.bodyIndex = BodyIndex.None;
            bodyComponent.name = bodyName;
            bodyComponent.bodyColor = new Color(0.25f, 0.65f, 0.25f);
            bodyComponent.baseNameToken = TTGL_SurvivorPlugin.developerPrefix + "_LAGANN_BODY_NAME";
            bodyComponent.subtitleNameToken = TTGL_SurvivorPlugin.developerPrefix + "_LAGANN_BODY_SUBTITLE";
            bodyComponent.portraitIcon = Modules.Assets.LoadAsset<Texture>("LagannIcon");

            bodyComponent.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            bodyComponent.rootMotionInMainState = false;

            bodyComponent.baseMaxHealth = 110f;
            bodyComponent.levelMaxHealth = 33f;

            bodyComponent.baseRegen = 1.5f;
            bodyComponent.levelRegen = 0.3f;

            bodyComponent.baseMaxShield = 0f;
            bodyComponent.levelMaxShield = 0f;

            bodyComponent.baseMoveSpeed = 7f;
            bodyComponent.levelMoveSpeed = 0f;

            bodyComponent.baseAcceleration = 80f;

            bodyComponent.baseJumpPower = 15f;
            bodyComponent.levelJumpPower = 0f;

            bodyComponent.baseDamage = 12f;
            bodyComponent.levelDamage = 2.4f;
            
            bodyComponent.baseAttackSpeed = 1f;
            bodyComponent.levelAttackSpeed = 0f;

            bodyComponent.baseArmor = 20f;
            bodyComponent.levelArmor = 0f;

            bodyComponent.baseCrit = 0f;
            bodyComponent.levelCrit = 0f;

            bodyComponent.baseJumpCount = 1;

            bodyComponent.sprintingSpeedMultiplier = 1.45f;

            bodyComponent.hideCrosshair = false;
            bodyComponent.aimOriginTransform = modelBaseTransform.Find("AimOrigin");
            bodyComponent.hullClassification = HullClassification.Human;

            bodyComponent.preferredPodPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");

            bodyComponent.isChampion = false;
        }

        protected virtual void SetupCharacterMotor(GameObject newPrefab)
        {
            CharacterMotor motorComponent = newPrefab.GetComponent<CharacterMotor>();
            motorComponent.mass = 1;
        }

        #region ModelSetup
        protected virtual Transform SetupModel(GameObject prefab, Transform modelTransform, bool isDisplay)
        {
            GameObject modelBase = new GameObject("ModelBase");
            modelBase.transform.parent = prefab.transform;
            modelBase.transform.localPosition = new Vector3(0f, -0.92f, 0f);
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

            return modelBase.transform;
        }

        protected virtual GameObject CreateModel(GameObject main, string modelName)
        {
            TTGL_SurvivorPlugin.DestroyImmediate(main.transform.Find("ModelBase").gameObject);
            TTGL_SurvivorPlugin.DestroyImmediate(main.transform.Find("CameraPivot").gameObject);
            TTGL_SurvivorPlugin.DestroyImmediate(main.transform.Find("AimOrigin").gameObject);

            return Modules.Assets.LoadAsset<GameObject>(modelName);
        }

        protected virtual void SetupCharacterModel(GameObject prefab, CustomRendererInfo[] rendererInfo, int mainRendererIndex)
        {
            CharacterModel characterModel = prefab.GetComponent<ModelLocator>().modelTransform.gameObject.AddComponent<CharacterModel>();
            ChildLocator childLocator = characterModel.GetComponent<ChildLocator>();
            characterModel.body = prefab.GetComponent<CharacterBody>();

            List<CharacterModel.RendererInfo> rendererInfos = new List<CharacterModel.RendererInfo>();

            for (int i = 0; i < rendererInfo.Length; i++)
            {
                var renderer = childLocator.FindChild(rendererInfo[i].childName).GetComponent<Renderer>();
                rendererInfos.Add(new CharacterModel.RendererInfo
                {
                    renderer = renderer,
                    defaultMaterial = rendererInfo[i].material ?? renderer.materials.FirstOrDefault(),
                    ignoreOverlays = rendererInfo[i].ignoreOverlays,
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On
                });
            }

            characterModel.baseRendererInfos = rendererInfos.ToArray();

            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();

            characterModel.mainSkinnedMeshRenderer = characterModel.baseRendererInfos[mainRendererIndex].renderer.GetComponent<SkinnedMeshRenderer>();
        }
        #endregion

        #region ComponentSetup
        protected virtual void SetupCharacterDirection(GameObject prefab, Transform modelBaseTransform, Transform modelTransform)
        {
            CharacterDirection characterDirection = prefab.GetComponent<CharacterDirection>();
            characterDirection.targetTransform = modelBaseTransform;
            characterDirection.overrideAnimatorForwardTransform = null;
            characterDirection.rootMotionAccumulator = null;
            characterDirection.modelAnimator = modelTransform.GetComponent<Animator>();
            characterDirection.driveFromRootRotation = false;
            characterDirection.turnSpeed = 720f;
        }

        protected virtual void SetupCameraTargetParams(GameObject prefab)
        {
            CameraTargetParams cameraTargetParams = prefab.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraPivotTransform = prefab.transform.Find("ModelBase").Find("CameraPivot");
            cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Standard);
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.dontRaycastToPivot = false;
        }

        protected virtual void SetupModelLocator(GameObject prefab, Transform modelBaseTransform, Transform modelTransform)
        {
            ModelLocator modelLocator = prefab.GetComponent<ModelLocator>();
            modelLocator.modelTransform = modelTransform;
            modelLocator.modelBaseTransform = modelBaseTransform;
        }

        protected virtual void SetupRigidbody(GameObject prefab)
        {
            Rigidbody rigidbody = prefab.GetComponent<Rigidbody>();
            rigidbody.mass = 100f;
        }

        protected virtual void SetupCapsuleCollider(GameObject prefab)
        {
            CapsuleCollider capsuleCollider = prefab.GetComponent<CapsuleCollider>();
            capsuleCollider.center = new Vector3(0f, 0f, 0f);
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.82f;
            capsuleCollider.direction = 1;
        }

        protected virtual void SetupFootstepController(GameObject model)
        {
            var footstepHandler = model.AddComponent<FootstepHandler>();
            footstepHandler.enableFootstepDust = true;
            footstepHandler.baseFootstepString = "Play_player_footstep";
            footstepHandler.footstepDustPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/GenericFootstepDust");
        }

        protected virtual void SetupRagdoll(GameObject model)
        {
            RagdollController ragdollController = model.GetComponent<RagdollController>();

            if (!ragdollController) return;

            if (ragdollMaterial == null) ragdollMaterial = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<RagdollController>().bones[1].GetComponent<Collider>().material;

            foreach (Transform i in ragdollController.bones)
            {
                if (i)
                {
                    i.gameObject.layer = LayerIndex.ragdoll.intVal;
                    Collider j = i.GetComponent<Collider>();
                    if (j)
                    {
                        j.material = ragdollMaterial;
                        j.sharedMaterial = ragdollMaterial;
                    }
                }
            }
        }

        protected virtual void SetupAimAnimator(GameObject prefab, GameObject model)
        {
            AimAnimator aimAnimator = model.AddComponent<AimAnimator>();
            aimAnimator.directionComponent = prefab.GetComponent<CharacterDirection>();
            aimAnimator.pitchRangeMax = 60f;
            aimAnimator.pitchRangeMin = -60f;
            aimAnimator.yawRangeMin = -80f;
            aimAnimator.yawRangeMax = 80f;
            aimAnimator.pitchGiveupRange = 30f;
            aimAnimator.yawGiveupRange = 10f;
            aimAnimator.giveupDuration = 3f;
            aimAnimator.inputBank = prefab.GetComponent<InputBankTest>();
        }

        protected virtual void SetupHitbox(GameObject prefab, Transform hitboxTransform, string hitboxGroupName)
        {
            HitBoxGroup hitBoxGroup = null;
            var hitBoxGroups = prefab.GetComponents<HitBoxGroup>();
            if (hitBoxGroups != null)
            {
                hitBoxGroup = hitBoxGroups.FirstOrDefault((x) => x.groupName == hitboxGroupName);
            }
            if (hitBoxGroup == null)
            {
                hitBoxGroup = prefab.AddComponent<HitBoxGroup>();
                hitBoxGroup.groupName = hitboxGroupName;
            }
            List<HitBox> hitBoxes = (hitBoxGroup.hitBoxes != null) ? hitBoxGroup.hitBoxes.ToList() : new List<HitBox>();
            HitBox hitBox = hitboxTransform.gameObject.AddComponent<HitBox>();
            hitboxTransform.gameObject.layer = LayerIndex.projectile.intVal;
            hitBoxes.Add(hitBox);
            hitBoxGroup.hitBoxes = hitBoxes.ToArray();
        }

        protected virtual void CreateGenericDoppelganger(GameObject bodyPrefab, string masterName, string masterToCopy)
        {
            var prefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/" + masterToCopy + "MonsterMaster");
            if (prefab == null)
            {
                TTGL_SurvivorPlugin.instance.Logger.LogError("Could not load Prefabs/CharacterMasters/" + masterToCopy + "MonsterMaster");
            }
            GameObject gameObject = PrefabAPI.InstantiateClone(prefab, masterName);
            gameObject.GetComponent<CharacterMaster>().bodyPrefab = bodyPrefab;
            TTGL_SurvivorPlugin.masterPrefabs.Add(gameObject);
        }

        #endregion
    }
}

// for simplifying rendererinfo creation
public class CustomRendererInfo
{
    internal string childName;
    internal Material material;
    internal bool ignoreOverlays;
}