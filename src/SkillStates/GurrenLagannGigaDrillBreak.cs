using System;
using System.Collections.ObjectModel;
using EntityStates;
using EntityStates.Huntress;
using EntityStates.VagrantMonster;
using RoR2;
using RoR2.Projectile;
using RoR2.UI;
using TTGL_Survivor.Modules;
using TTGL_Survivor.Modules.Components;
using TTGL_Survivor.Modules.Survivors;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{   
    public class GurrenLagannGigaDrillBreak : BaseSkillState
    {
        public static float damageCoefficient = 1000000f;
        public static float radius = 1000000f;
        public static int explosionCount = 2;
        public static float explosionInterval = 2f;
        public static string soundString = "TTGLGigaDrillBreak";
        protected Animator animator;
        protected CharacterBody gigaDrillBreakTarget;
        private float duration;
        private float durationBeforeExplosion;
        private int explosionIndex;
        private Vector3 previousPosition;
        private Vector3 previousTargetPosition;
        private Transform specialMoveTargetPosition;
        private Transform specialMoveCameraSource;
        private SkippableCamera forcedCamera;
        private bool fired;
        private uint currentSound;

        public override void OnEnter()
        {
            base.OnEnter();
            this.fired = false;
            this.animator = base.GetModelAnimator();
            this.explosionIndex = 0;
            duration = 22.5f;
            durationBeforeExplosion = 16f;
            var gurrenLagannController = base.characterBody.GetComponent<GurrenLagannController>();
            if (gurrenLagannController && gurrenLagannController.gigaDrillBreakTarget)
            {
                gigaDrillBreakTarget = gurrenLagannController.gigaDrillBreakTarget.GetComponent<CharacterBody>();
                ConstrictBoss(gigaDrillBreakTarget);
            }            
            base.PlayAnimation("FullBody, Override", "GurrenLagannGigaDrillBreak", "skill4.playbackRate", this.duration);
            if (CameraRigController.IsObjectSpectatedByAnyCamera(base.gameObject))
            {
                this.currentSound = Util.PlaySound(GurrenLagannGigaDrillBreak.soundString, base.gameObject);
            }
            AllowOutOfBound(true);
            this.SetAntiGravity(base.characterBody, true);
            this.previousPosition = base.characterMotor.Motor.transform.position;
            this.SetPosition(base.gameObject, base.characterMotor.Motor.transform.position + new Vector3(50000f, 0, 0f));
            if (gigaDrillBreakTarget)
            {
                var targetDirection = (gigaDrillBreakTarget.transform.position - base.characterDirection.targetTransform.position).normalized;
                base.characterDirection.forward = targetDirection;
                this.SetAntiGravity(gigaDrillBreakTarget, true);
                this.previousTargetPosition = gigaDrillBreakTarget.transform.position;
                this.SetPosition(gigaDrillBreakTarget.gameObject, gigaDrillBreakTarget.transform.position + new Vector3(50000f, 0f, 0f));
            }
            var childSelector = base.GetModelChildLocator();
            if (childSelector)
            {
                this.specialMoveTargetPosition = childSelector.FindChild("SpecialMoveTargetPosition");
                this.specialMoveCameraSource = childSelector.FindChild("SpecialMoveCameraSource");
                this.forcedCamera = specialMoveCameraSource.GetComponent<SkippableCamera>();
            }
            UpdateCameraOverride();
            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2.RoR2Content.Buffs.HiddenInvincibility);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            UpdateCameraOverride();
            if (this.explosionIndex < GurrenLagannGigaDrillBreak.explosionCount &&
                base.fixedAge >= this.durationBeforeExplosion)
            {
                this.Explode((this.gigaDrillBreakTarget) ? this.gigaDrillBreakTarget.transform.position : base.transform.position);
                this.durationBeforeExplosion += GurrenLagannGigaDrillBreak.explosionInterval;
                this.explosionIndex++;
            }
            if (this.specialMoveTargetPosition && this.gigaDrillBreakTarget)
            {
                this.SetPosition(gigaDrillBreakTarget.gameObject, this.specialMoveTargetPosition.position);
            }
            if (base.isAuthority)
            {
                if (base.fixedAge >= this.duration || (base.inputBank && base.inputBank.interact.down))
                {
                    this.outer.SetNextStateToMain();
                }
            }
        }

        public override void OnExit()
        {
            AllowOutOfBound(false);
            DisableCameraOverride();
            //this.SetPosition(base.gameObject, this.previousPosition);
            this.SetAntiGravity(base.characterBody, false);
            if (gigaDrillBreakTarget)
            {
                this.SetPosition(gigaDrillBreakTarget.gameObject, this.previousTargetPosition);
                this.SetAntiGravity(gigaDrillBreakTarget, false);
            }
            //UnConstrictBoss(gigaDrillBreakTarget);            
            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            if (!this.fired)
            {
                explosionIndex = 0;
                Explode((this.previousTargetPosition != null) ? this.previousTargetPosition : this.previousPosition);
                explosionIndex = 1;
                Explode((this.previousTargetPosition != null) ? this.previousTargetPosition : this.previousPosition);
            }
            if (NetworkServer.active) base.characterBody.RemoveBuff(RoR2.RoR2Content.Buffs.HiddenInvincibility);
            if (this.currentSound != 0)
            {
                AkSoundEngine.StopPlayingID(this.currentSound);
            }
            this.TransformToLagann(this.previousPosition);
            base.OnExit();
        }

        private void Explode(Vector3 target)
        {            
            if (explosionIndex == 0)
            {
                EffectManager.SpawnEffect(Assets.specialExplosion, new EffectData
                {
                    origin = target,
                    scale = 1f
                }, false);
            }
            else
            {
                this.fired = true;
                if (NetworkServer.active)
                {
                    new BlastAttack
                    {
                        damageType = (DamageType.BypassArmor | DamageType.BypassOneShotProtection | DamageType.WeakPointHit),
                        attacker = base.gameObject,
                        inflictor = base.gameObject,
                        teamIndex = TeamComponent.GetObjectTeam(base.gameObject),
                        baseDamage = this.damageStat * GurrenLagannGigaDrillBreak.damageCoefficient,
                        baseForce = ExplosionAttack.force,
                        position = target,
                        radius = GurrenLagannGigaDrillBreak.radius,
                        falloffModel = BlastAttack.FalloffModel.None,
                        attackerFiltering = AttackerFiltering.NeverHit
                    }.Fire();
                }
            }
        }

        private void ConstrictBoss(CharacterBody bossBody)
        {
            if (bossBody)
            {
                var existingConstrict = bossBody.GetComponentInChildren<GurrenLagannShadesConstrictComponent>();
                if (!existingConstrict || existingConstrict.visualState == 1)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(TemporaryVisualEffects.gurrenLagannShadesBindingEffect, bossBody.corePosition, Quaternion.identity);
                    var component = gameObject.GetComponent<GurrenLagannShadesConstrictComponent>();
                    component.duration = this.duration;
                    component.parentTransform = bossBody.transform;
                    gameObject.transform.SetParent(bossBody.transform);
                }
                else
                {
                    existingConstrict.durationCounter = 0f;
                    existingConstrict.duration = this.duration;
                }
            }
        }

        private void UnConstrictBoss(CharacterBody bossBody)
        {
            if (bossBody)
            {
                var existingConstrict = bossBody.GetComponentInChildren<GurrenLagannShadesConstrictComponent>();
                if (existingConstrict && existingConstrict.visualState == 0)
                {
                    existingConstrict.durationCounter = 0f;
                    existingConstrict.duration = 0f;
                }
            }
        }
        
        private void SetAntiGravity(CharacterBody character, bool enabled)
        {
            if (character && character.characterMotor)
            {
                CharacterGravityParameters gravityParameters = character.characterMotor.gravityParameters;
                gravityParameters.channeledAntiGravityGranterCount += (enabled ? 1 : -1);
                character.characterMotor.gravityParameters = gravityParameters;
            }
        }

        private void AllowOutOfBound(bool enabled)
        {
            if (enabled)
            {
                On.RoR2.MapZone.TryZoneStart += MapZone_TryZoneStart;
            }
            else
            {
                On.RoR2.MapZone.TryZoneStart -= MapZone_TryZoneStart;
                var characterCollider = base.characterBody.GetComponent<Collider>();
                if (characterCollider)
                {
                    MapZone.collidersToCheckInFixedUpdate.Enqueue(characterCollider);
                }
                if (gigaDrillBreakTarget)
                {
                    var gigaDrillBreakTargetCollider = gigaDrillBreakTarget.GetComponent<Collider>();
                    if (gigaDrillBreakTargetCollider)
                    {
                        MapZone.collidersToCheckInFixedUpdate.Enqueue(gigaDrillBreakTargetCollider);
                    }
                }
            }
        }

        private void MapZone_TryZoneStart(On.RoR2.MapZone.orig_TryZoneStart orig, MapZone self, Collider other)
        {
            CharacterBody component = other.GetComponent<CharacterBody>();
            if (component && (component == gigaDrillBreakTarget || component == base.characterBody))
            {
                self.queuedCollisions.Add(new MapZone.CollisionInfo(self, other));
                return;
            }
            orig(self, other);
        }

        private void SetPosition(GameObject character, Vector3 newPosition)
        {
            CharacterMotor characterMotor = character.GetComponent<CharacterMotor>();
            if (characterMotor && characterMotor.Motor)
            {
                characterMotor.Motor.SetPosition(newPosition, true);
                characterMotor.Motor.BaseVelocity = Vector3.zero;
                characterMotor.velocity = Vector3.zero;
                characterMotor.rootMotion = Vector3.zero;
                return;
            }
            RigidbodyMotor rigidbodyMotor = character.GetComponent<RigidbodyMotor>();
            if (rigidbodyMotor && rigidbodyMotor.rigid)
            {
                rigidbodyMotor.rigid.MovePosition(newPosition);
                rigidbodyMotor.rootMotion = Vector3.zero;
                return;
            }
            gameObject.transform.position = newPosition;
        }

        private void UpdateCameraOverride()
        {
            if (this.specialMoveCameraSource && this.forcedCamera)
            {
                if (CameraRigController.IsObjectSpectatedByAnyCamera(base.gameObject))
                {
                    this.specialMoveCameraSource.gameObject.SetActive(true);
                    this.forcedCamera.allowUserControl = base.isAuthority;
                }
                else
                {
                    this.specialMoveCameraSource.gameObject.SetActive(false);
                }
            }
        }

        private void DisableCameraOverride()
        {
            if (this.specialMoveCameraSource)
            {
                this.specialMoveCameraSource.gameObject.SetActive(false);
            }
        }


        private void TransformToLagann(Vector3 newPosition)
        {
            Util.PlaySound(BaseBeginArrowBarrage.blinkSoundString, base.gameObject);
            if (NetworkServer.active && base.characterBody && base.characterBody.master)
            {
                var master = base.characterBody.master;
                master.TransformBody("LagannBody");
                SpawnGurren(newPosition);
                var body = master.GetBodyObject();
                Popup(body, newPosition + Vector3.up * 5f);
            }
        }

        private void SpawnGurren(Vector3 newPosition)
        {
            float d = 0f;
            CharacterMaster characterMaster = new MasterSummon
            {
                masterPrefab = Gurren.allyPrefab,
                position = newPosition + Vector3.up * d,
                rotation = base.characterBody.transform.rotation,
                summonerBodyObject = ((base.characterBody != null) ? base.characterBody.gameObject : null),
                ignoreTeamMemberLimit = true,
                useAmbientLevel = new bool?(true),
                inventoryToCopy = base.characterBody.inventory,
            }.Perform();

        }

        private void Popup(GameObject character, Vector3 newPosition)
        {
            if (character)
            {
                var velocity = Vector3.up * 20f;
                CharacterMotor characterMotor = character.GetComponent<CharacterMotor>();
                if (characterMotor && characterMotor.Motor)
                {
                    characterMotor.Motor.SetPosition(newPosition, true);
                    characterMotor.Motor.BaseVelocity = velocity;
                    characterMotor.velocity = velocity;
                    return;
                }
                gameObject.transform.position = newPosition;
            }
        }
    }
}