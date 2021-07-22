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
    public class PrepareLagannBurrowerStrike : BaseSkillState
    {
        public static float damageCoefficient = 5f;
        private string playbackRateString = "BurrowerStrike.playbackRate";

        private bool hasHitGround;
        private bool falling;
        private bool fell;
        private OverlapAttack attack;
        private float hitStopDuration = 0.25f;
        private float attackResetDuration = 0.5f;
        private float burrowingDuration = 0.5f;
        private BaseState.HitStopCachedState hitStopCachedState;
        private Vector3 storedVelocity;
        private float hitPauseTimer;
        private float attackResetTimer;
        private float burrowingTimer;
        private bool inHitPause;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            if (TTGL_SurvivorPlugin.rideMeExtendedInstalled)
            {
                TTGL_SurvivorPlugin.ExpulseAnyRider(base.gameObject);
                TTGL_SurvivorPlugin.ExitSeat(base.gameObject);
            }
            this.animator = base.GetModelAnimator();
            if (base.isGrounded)
            {
                base.PlayCrossfade("FullBody, Override", "LagannBurrowerStrikeBurrow", this.playbackRateString, burrowingDuration, 0.2f);
                DisplayMound();
            }
            else
            {
                falling = true;
                fell = true;
                base.PlayCrossfade("FullBody, Override", "LagannBurrowerStrikeFall", 0.2f);
                if (NetworkServer.active)
                {
                    base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
                }                
                if (base.isAuthority)
                {
                    base.characterMotor.onMovementHit += this.OnMovementHit;
                    base.characterMotor.velocity.y = GroundSlam.initialVerticalVelocity;
                }
            }

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
                pushAwayForce = 300f,
                isCrit = base.RollCrit(),
                damageColorIndex = DamageColorIndex.Default,
                inflictor = base.gameObject,
                procChainMask = default(ProcChainMask),
                procCoefficient = 1f,
                teamIndex = base.characterBody.teamComponent.teamIndex,
                hitBoxGroup = hitBoxGroup,
                hitEffectPrefab = Modules.Assets.punchImpactEffect,
                impactSound = Modules.Assets.drillRushHitSoundEvent.index,
            };
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.ProcessHitLag();
            this.FireAttack();
            if (falling)
            {
                this.FallDown();
                if (this.hasHitGround || base.isGrounded)
                {
                    base.PlayCrossfade("FullBody, Override", "LagannBurrowerStrikeBurrow", this.playbackRateString, burrowingDuration, 0.2f);
                    DisplayMound();
                    falling = false;
                }
            }
            else
            {
                burrowingTimer += Time.fixedDeltaTime;
                if (burrowingTimer >= burrowingDuration)
                {
                    this.SetNextState();
                }                
            }            
        }

        public override void OnExit()
        {
            if (fell)
            {
                if (NetworkServer.active)
                {
                    base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
                }
                if (base.isAuthority)
                {
                    base.characterMotor.onMovementHit -= this.OnMovementHit;
                }
            }           
            base.OnExit();
        }
        
        private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            this.hasHitGround = true;
        }

        private void SetNextState()
        {
            if (base.isAuthority)
            {
                this.outer.SetNextState(new AimLagannBurrowerStrike());
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

        private void FallDown()
        {
            if (!this.inHitPause && base.isAuthority && base.characterMotor)
            {
                base.characterMotor.moveDirection = base.inputBank.moveVector;
                base.characterDirection.moveVector = base.characterMotor.moveDirection;
                CharacterMotor characterMotor = base.characterMotor;
                characterMotor.velocity.y = characterMotor.velocity.y + GroundSlam.verticalAcceleration * Time.fixedDeltaTime;
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

            if (this.inHitPause)
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

        private void DisplayMound()
        {
            EffectManager.SpawnEffect(Assets.earthMoundEffect, new EffectData
            {
                origin = base.characterBody.footPosition,
                rotation = base.transform.rotation
            }, false);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

    }
}