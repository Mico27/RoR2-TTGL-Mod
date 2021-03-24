using R2API;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TTGL_Survivor.Modules
{
    public abstract class BaseSurvivor
    {
        // cache this just to give our ragdolls the same physic material as vanilla stuff
        private static PhysicMaterial ragdollMaterial;

        protected void RegisterNewSurvivor(GameObject bodyPrefab, GameObject displayPrefab, Color charColor, string namePrefix, string unlockString)
        {
            SurvivorDef survivorDef = new SurvivorDef
            {
                name = TTGL_SurvivorPlugin.developerPrefix + "_" + namePrefix + "_BODY_NAME",
                unlockableName = unlockString,
                descriptionToken = TTGL_SurvivorPlugin.developerPrefix + "_" + namePrefix + "_BODY_DESCRIPTION",
                primaryColor = charColor,
                bodyPrefab = bodyPrefab,
                displayPrefab = displayPrefab,
                outroFlavorToken = TTGL_SurvivorPlugin.developerPrefix + "_" + namePrefix + "_BODY_OUTRO_FLAVOR",
            };

            SurvivorAPI.AddSurvivor(survivorDef);
        }

        protected virtual GameObject CreateDisplayPrefab(string modelName, GameObject prefab)
        {
            GameObject newPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"), modelName);

            GameObject model = CreateModel(newPrefab, modelName);

            Transform modelBaseTransform = SetupModel(newPrefab, model.transform, true);

            model.AddComponent<CharacterModel>().baseRendererInfos = prefab.GetComponentInChildren<CharacterModel>().baseRendererInfos;

            return model.gameObject;
        }

        protected virtual GameObject CreatePrefab(string bodyName, string modelName, BodyInfo bodyInfo)
        {
            GameObject newPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"), bodyName);

            GameObject model = CreateModel(newPrefab, modelName);
            Transform modelBaseTransform = SetupModel(newPrefab, model.transform, false);

            #region CharacterBody
            CharacterBody bodyComponent = newPrefab.GetComponent<CharacterBody>();

            bodyComponent.bodyIndex = -1;
            bodyComponent.name = bodyInfo.bodyName;
            bodyComponent.baseNameToken = bodyInfo.bodyNameToken;
            bodyComponent.subtitleNameToken = bodyInfo.subtitleNameToken;
            bodyComponent.portraitIcon = bodyInfo.characterPortrait;
            bodyComponent.crosshairPrefab = bodyInfo.crosshair;

            bodyComponent.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            bodyComponent.rootMotionInMainState = false;

            bodyComponent.baseMaxHealth = bodyInfo.maxHealth;
            bodyComponent.levelMaxHealth = bodyInfo.healthGrowth;

            bodyComponent.baseRegen = bodyInfo.healthRegen;
            bodyComponent.levelRegen = bodyComponent.baseRegen * 0.2f;

            bodyComponent.baseMaxShield = bodyInfo.shield;
            bodyComponent.levelMaxShield = bodyInfo.shieldGrowth;

            bodyComponent.baseMoveSpeed = bodyInfo.moveSpeed;
            bodyComponent.levelMoveSpeed = bodyInfo.moveSpeedGrowth;

            bodyComponent.baseAcceleration = bodyInfo.acceleration;

            bodyComponent.baseJumpPower = bodyInfo.jumpPower;
            bodyComponent.levelJumpPower = bodyInfo.jumpPowerGrowth;

            bodyComponent.baseDamage = bodyInfo.damage;
            bodyComponent.levelDamage = bodyComponent.baseDamage * 0.2f;

            bodyComponent.baseAttackSpeed = bodyInfo.attackSpeed;
            bodyComponent.levelAttackSpeed = bodyInfo.attackSpeedGrowth;

            bodyComponent.baseArmor = bodyInfo.armor;
            bodyComponent.levelArmor = bodyInfo.armorGrowth;

            bodyComponent.baseCrit = bodyInfo.crit;
            bodyComponent.levelCrit = bodyInfo.critGrowth;

            bodyComponent.baseJumpCount = bodyInfo.jumpCount;

            bodyComponent.sprintingSpeedMultiplier = 1.45f;

            bodyComponent.hideCrosshair = false;
            bodyComponent.aimOriginTransform = modelBaseTransform.Find("AimOrigin");
            bodyComponent.hullClassification = HullClassification.Human;

            bodyComponent.preferredPodPrefab = bodyInfo.podPrefab;

            bodyComponent.isChampion = false;
            #endregion

            SetupCharacterDirection(newPrefab, modelBaseTransform, model.transform);
            SetupCameraTargetParams(newPrefab);
            SetupModelLocator(newPrefab, modelBaseTransform, model.transform);
            SetupRigidbody(newPrefab);
            SetupCapsuleCollider(newPrefab);
            SetupFootstepController(model);
            SetupRagdoll(model);
            SetupAimAnimator(newPrefab, model);

            BodyCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(newPrefab);
            };

            return newPrefab;
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

            return Modules.Assets.mainAssetBundle.LoadAsset<GameObject>(modelName);
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
            cameraTargetParams.cameraParams = Resources.Load<GameObject>("Prefabs/CharacterBodies/MercBody").GetComponent<CameraTargetParams>().cameraParams;
            cameraTargetParams.cameraPivotTransform = prefab.transform.Find("ModelBase").Find("CameraPivot");
            cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.idealLocalCameraPos = Vector3.zero;
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
            var footstepHandler = model.GetComponent<FootstepHandler>();
            if (!footstepHandler)
            {
                footstepHandler = model.AddComponent<FootstepHandler>();
                footstepHandler.enableFootstepDust = true;
                footstepHandler.baseFootstepString = "Play_player_footstep";
                footstepHandler.footstepDustPrefab = Resources.Load<GameObject>("Prefabs/GenericFootstepDust");
            }
        }

        protected virtual void SetupRagdoll(GameObject model)
        {
            RagdollController ragdollController = model.GetComponent<RagdollController>();

            if (!ragdollController) return;

            if (ragdollMaterial == null) ragdollMaterial = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<RagdollController>().bones[1].GetComponent<Collider>().material;

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
        #endregion
    }
}

// for simplifying characterbody creation
public class BodyInfo
{
    internal string bodyName = "";
    internal string bodyNameToken = "";
    internal string subtitleNameToken = "";

    internal Texture characterPortrait = null;

    internal GameObject crosshair = null;
    internal GameObject podPrefab = null;

    internal float maxHealth = 100f;
    internal float healthGrowth = 2f;

    internal float healthRegen = 2f;

    internal float shield = 0f;// base shield is a thing apparently. neat
    internal float shieldGrowth = 0f;

    internal float moveSpeed = 7f;
    internal float moveSpeedGrowth = 0f;

    internal float acceleration = 80f;

    internal float jumpPower = 15f;
    internal float jumpPowerGrowth = 0f;// jump power per level exists for some reason

    internal float damage = 12f;

    internal float attackSpeed = 1f;
    internal float attackSpeedGrowth = 0f;

    internal float armor = 0f;
    internal float armorGrowth = 0f;

    internal float crit = 1f;
    internal float critGrowth = 0f;

    internal int jumpCount = 1;
}

// for simplifying rendererinfo creation
public class CustomRendererInfo
{
    internal string childName;
    internal Material material;
    internal bool ignoreOverlays;
}