using RoR2;
using UnityEngine;
using EntityStates;
using RoR2.Audio;
using System;
using UnityEngine.Networking;
using EntityStates.Merc;
using EntityStates.Huntress;
using TTGL_Survivor.Modules;
using System.Collections.Generic;
using System.Linq;
using RoR2.Projectile;
using EntityStates.Loader;

namespace TTGL_Survivor.SkillStates
{
    public class LagannDrillSpike : BaseSkillState
    {
        public static float baseDuration = 1.5f;
        public static float maxDistance = 256f;
        public static float damageCoefficient = 4.0f;
        public static float attackStartTime = 0.2f;
        public static float baseEarlyExitTime = 0.3f;
        public static float procCoefficient = 0.9f;


        public float bonusMultiplier = 1f;
        public int swingIndex = 0;
        private float startingDistance = 0f;
        private float earlyExitTime;
        private bool hasFired;
        private float duration;
        private string playbackRateString = "LagannDrillSpike.playbackRate";

        private float previousAirControl;
        private GameObject leftFistEffectInstance;
        private GameObject rightFistEffectInstance;
        private bool detonateNextFrame;
        private bool falling;
        private bool fell;

        public override void OnEnter()
        {
            base.OnEnter();
            if (TTGL_SurvivorPlugin.rideMeExtendedInstalled)
            {
                TTGL_SurvivorPlugin.ExitSeat(base.gameObject);
            }
            this.duration = LagannDrillSpike.baseDuration / this.attackSpeedStat;
            this.earlyExitTime = LagannDrillSpike.baseEarlyExitTime / this.attackSpeedStat;
            this.hasFired = false;
            base.StartAimMode(0.5f + this.duration, false);            
            if (base.isGrounded)
            {
                base.PlayCrossfade("FullBody, Override", "LagannDrillSpike" + (1 + swingIndex), this.playbackRateString, this.duration, 0.05f);
            }
            else
            {
                base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                falling = true;
                fell = true;
                //Slam Down Raycast
                RaycastHit raycastHit;
                if (Physics.Raycast(new Ray(base.transform.position, Vector3.down), out raycastHit, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    startingDistance = Vector3.Distance(base.transform.position, raycastHit.point);
                }
                else
                {
                    startingDistance = maxDistance;
                }
                this.bonusMultiplier = 1f + ((startingDistance / maxDistance) * 10);
                base.PlayCrossfade("FullBody, Override", "LagannDrillSpikeFall", 0.2f);
                if (base.isAuthority)
                {
                    base.characterMotor.onMovementHit += this.OnMovementHit;
                    base.characterMotor.velocity.y = GroundSlam.initialVerticalVelocity;
                }
                Util.PlaySound(GroundSlam.enterSoundString, base.gameObject);
                this.previousAirControl = base.characterMotor.airControl;
                base.characterMotor.airControl = GroundSlam.airControl;
                this.leftFistEffectInstance = UnityEngine.Object.Instantiate<GameObject>(GroundSlam.fistEffectPrefab, base.FindModelChild("RightHandDrill"));
                this.rightFistEffectInstance = UnityEngine.Object.Instantiate<GameObject>(GroundSlam.fistEffectPrefab, base.FindModelChild("LeftHandDrill"));
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (falling)
            {
                this.FallDown();
                if (base.fixedAge >= GroundSlam.minimumDuration && (this.detonateNextFrame || base.isGrounded))
                {
                    this.DetonateAuthority();
                    base.PlayAnimation("FullBody, Override", "BufferEmpty");
                    falling = false;
                }
            }
            else
            {
                if (base.fixedAge >= (this.duration * LagannDrillSpike.attackStartTime))
                {
                    this.FireAttack();
                }

                if (base.fixedAge >= (this.duration - this.earlyExitTime) && base.isAuthority && base.inputBank.skill1.down)
                {
                    this.SetNextState();
                    return;
                }

                if (base.fixedAge >= this.duration && base.isAuthority)
                {
                    this.outer.SetNextStateToMain();
                    return;
                }
            }            
        }

        public override void OnExit()
        {
            this.FireAttack();
            if (fell)
            {
                base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
                if (base.isAuthority)
                {
                    base.characterMotor.onMovementHit -= this.OnMovementHit;
                    //base.characterMotor.Motor.ForceUnground();
                    //base.characterMotor.velocity *= GroundSlam.exitSlowdownCoefficient;
                    //base.characterMotor.velocity.y = GroundSlam.exitVerticalVelocity;
                }
                base.characterMotor.airControl = this.previousAirControl;
                EntityState.Destroy(this.leftFistEffectInstance);
                EntityState.Destroy(this.rightFistEffectInstance);
            }
            base.OnExit();
        }
        
        private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            this.detonateNextFrame = true;
        }

        private void SetNextState()
        {
            int index = this.swingIndex;
            if (index == 0) index = 1;
            else index = 0;

            this.outer.SetNextState(new LagannDrillSpike
            {
                swingIndex = index,
                bonusMultiplier = Math.Max(1f, this.bonusMultiplier / 2f),
            });
        }

        private void FireSpike(Vector3 origin, Quaternion rotation)
        {
            if (base.isAuthority)
            {
                EffectManager.SpawnEffect(TTGLAssets.drillPopEffect, new EffectData
                {
                    origin = origin,
                    rotation = rotation,
                    scale = this.bonusMultiplier
                }, true);
                new BlastAttack
                {
                    attacker = base.gameObject,
                    baseDamage = this.damageStat * damageCoefficient,
                    baseForce = GroundSlam.blastForce / 2,
                    bonusForce = GroundSlam.blastBonusForce / 2,
                    crit = base.RollCrit(),
                    damageType = DamageType.BypassArmor,
                    falloffModel = BlastAttack.FalloffModel.None,
                    procCoefficient = procCoefficient,
                    radius = this.bonusMultiplier * 5f,
                    position = origin,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    impactEffect = EffectCatalog.FindEffectIndexFromPrefab(GroundSlam.blastImpactEffectPrefab),
                    teamIndex = base.teamComponent.teamIndex
                }.Fire();
            }
        }

        private void FireAttack()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;
                Util.PlayAttackSpeedSound(BackflipState.dodgeSoundString, base.gameObject, this.attackSpeedStat);
                if (base.isAuthority)
                {
                    var aimRay = base.GetAimRay();
                    RaycastHit raycastHit;
                    //Drill Pop Raycast
                    if (Physics.Raycast(aimRay, out raycastHit, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                    {
                        this.FireSpike(raycastHit.point, Util.QuaternionSafeLookRotation(Vector3.forward, raycastHit.normal));
                    }
                }
            }
        }

        private void FallDown()
        {
            if (base.isAuthority && base.characterMotor)
            {
                base.characterMotor.moveDirection = base.inputBank.moveVector;
                base.characterDirection.moveVector = base.characterMotor.moveDirection;
                CharacterMotor characterMotor = base.characterMotor;
                characterMotor.velocity.y = characterMotor.velocity.y + GroundSlam.verticalAcceleration * Time.deltaTime;
            }
        }

        protected void DetonateAuthority()
        {
            if (base.isAuthority)
            {
                Vector3 footPosition = base.characterBody.footPosition;
                EffectManager.SpawnEffect(GroundSlam.blastEffectPrefab, new EffectData
                {
                    origin = footPosition,
                    scale = this.bonusMultiplier
                }, true);
                new BlastAttack
                {
                    attacker = base.gameObject,
                    baseDamage = this.damageStat * damageCoefficient,
                    baseForce = GroundSlam.blastForce / 2,
                    bonusForce = GroundSlam.blastBonusForce / 2,
                    crit = base.RollCrit(),
                    damageType = DamageType.Generic,
                    falloffModel = BlastAttack.FalloffModel.Linear,
                    procCoefficient = procCoefficient,
                    radius = this.bonusMultiplier * 5f,
                    position = footPosition,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    impactEffect = EffectCatalog.FindEffectIndexFromPrefab(GroundSlam.blastImpactEffectPrefab),
                    teamIndex = base.teamComponent.teamIndex
                }.Fire();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(this.swingIndex);
            writer.Write(this.bonusMultiplier);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.swingIndex = reader.ReadInt32();
            this.bonusMultiplier = reader.ReadSingle();
        }
    }
}