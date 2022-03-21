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
    public class LagannBurrowerStrike : BaseSkillState
    {
        public const float c_SpeedCoefficient = 64.0f;
        public static float maxDuration = 1.5f;
        public static float damageCoefficient = 10f;
        private string playbackRateString = "BurrowerStrike.playbackRate";
        public const float procCoefficient = 2f;

        public Vector3 spawnLocation;
        public Vector3 spawnRotation;
        private Transform rootTransform;

        private OverlapAttack attack;
        private float hitStopDuration = 0.25f;
        private float attackResetDuration = 0.5f;
        private BaseState.HitStopCachedState hitStopCachedState;
        private Vector3 storedVelocity;
        private float hitPauseTimer;
        private float attackResetTimer;
        private bool inHitPause;
        private Animator animator;
        private float stopwatch;
        private bool hasHitGround;

        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
            var childLocator = base.GetModelChildLocator();
            this.rootTransform = childLocator.FindChild("LagganArmature");
            base.characterMotor.Motor.SetPosition(this.spawnLocation + (this.spawnRotation * 4f));
            
            base.PlayAnimation("FullBody, Override", "BurrowerStrikePop", this.playbackRateString, LagannBurrowerStrike.maxDuration);            
            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "LagannImpactHitbox");
            }

            this.attack = new OverlapAttack
            {
                attacker = base.gameObject,
                damage = PrepareLagannBurrowerStrike.damageCoefficient * base.characterBody.damage,
                damageType = DamageType.BypassArmor,
                pushAwayForce = 300f,
                isCrit = base.RollCrit(),
                damageColorIndex = DamageColorIndex.WeakPoint,
                inflictor = base.gameObject,
                procChainMask = default(ProcChainMask),
                procCoefficient = procCoefficient,
                teamIndex = base.characterBody.teamComponent.teamIndex,
                hitBoxGroup = hitBoxGroup,
                hitEffectPrefab = Modules.Assets.punchImpactEffect,
                impactSound = Modules.Assets.drillRushHitSoundEvent.index,
            };
            base.characterMotor.useGravity = false;
            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
            if (base.isAuthority)
            {
                base.characterMotor.onMovementHit += this.OnMovementHit;
            }
            EffectManager.SpawnEffect(Assets.earthMoundEffect, new EffectData
            {
                origin = this.spawnLocation,
                rotation = Util.QuaternionSafeLookRotation(this.spawnRotation) * Quaternion.Euler(new Vector3(90, 0, 0))
        }, false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.ProcessHitLag();
            this.FireAttack();
            this.MoveUp();
            if (base.isAuthority && (UserCancelled() || this.hasHitGround || this.stopwatch > LagannBurrowerStrike.maxDuration))
            {
                base.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.characterMotor.useGravity = true;
            this.rootTransform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0));
            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
            if (base.isAuthority)
            {
               base.characterMotor.onMovementHit -= this.OnMovementHit;
            }
            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            base.OnExit();
        }

        private bool UserCancelled()
        {
            return base.inputBank && ((base.skillLocator && base.skillLocator.utility.IsReady() && base.inputBank.skill3.justPressed) || base.inputBank.interact.justPressed || base.inputBank.jump.justPressed);
        }

        private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            if (base.fixedAge > 0.2f)
            {
                this.hasHitGround = true;
            }            
        }
        
        private void FireAttack()
        {
            if (base.isAuthority)
            {
                attackResetTimer += Time.fixedDeltaTime;
                if (attackResetTimer >= this.attackResetDuration)
                {
                    attackResetTimer = 0f;
                    this.attack.isCrit = base.RollCrit();
                    this.attack.ResetIgnoredHealthComponents();
                }
                if (this.attack.Fire())
                {
                    this.OnHitEnemyAuthority();
                }
            }
        }

        private void MoveUp()
        {
            if (!this.inHitPause)
            {
                var newRotation = Util.QuaternionSafeLookRotation(this.spawnRotation) * Quaternion.Euler(new Vector3(-90, 0, 0));
                this.rootTransform.rotation = newRotation;
                if (base.isAuthority && base.characterMotor)
                {
                    base.characterMotor.rootMotion += this.spawnRotation * (c_SpeedCoefficient * Time.fixedDeltaTime);
                }
            }
        }

        private void ProcessHitLag()
        {
            this.hitPauseTimer -= Time.fixedDeltaTime;
            if (this.hitPauseTimer <= 0f && this.inHitPause)
            {
                base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                this.inHitPause = false;
                base.characterMotor.velocity = this.storedVelocity;
            }
            if (!this.inHitPause)
            {
                this.stopwatch += Time.fixedDeltaTime;
            }
            else
            {
                if (base.characterMotor) base.characterMotor.velocity = Vector3.zero;
                if (this.animator) this.animator.SetFloat(this.playbackRateString, 0f);
            }
        }

        protected virtual void OnHitEnemyAuthority()
        {
            if (!this.inHitPause)
            {
                this.storedVelocity = base.characterMotor.velocity;
                this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, this.playbackRateString);
                this.hitPauseTimer = this.hitStopDuration;
                this.inHitPause = true;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(this.spawnLocation);
            writer.Write(this.spawnRotation);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.spawnLocation = reader.ReadVector3();
            this.spawnRotation = reader.ReadVector3();
        }
    }
}