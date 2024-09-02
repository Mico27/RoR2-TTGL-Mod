using RoR2;
using UnityEngine;
using EntityStates;
using RoR2.Audio;
using System;
using UnityEngine.Networking;
using EntityStates.Merc;
using EntityStates.Huntress;
using TTGL_Survivor.Modules;

namespace TTGL_Survivor.SkillStates
{
    public class LagannDrillRush : BaseSkillState
    {
        public static float damageCoefficient = 3.0f;        
        public static float spiralEnergyPercentagePerHit = 0f;
        public static float procCoefficient = 0.75f;

        public int swingIndex;

        protected string hitboxName = "DrillRushHitbox";

        protected DamageType damageType = DamageType.BypassArmor;        
        protected float pushForce = 500f;
        protected Vector3 bonusForce = Vector3.zero;
        protected float baseDuration = 0.8f;
        protected float attackStartTime = 0.2f;
        protected float attackEndTime = 0.4f;
        protected float baseEarlyExitTime = 0.3f;
        protected float hitStopDuration = 0.115f;
        protected float attackRecoil = 0.75f;
        protected float hitHopVelocity = 6f;

        protected string muzzleString = "";
        protected string playbackRateString = "DrillRush.playbackRate";
        protected GameObject swingEffectPrefab;
        protected GameObject hitEffectPrefab;
        protected NetworkSoundEventIndex impactSound;

        private float earlyExitTime;
        public float duration;
        private bool hasFired;
        private float hitPauseTimer;
        private OverlapAttack attack;
        private bool inHitPause;
        private bool hasHopped;
        private float stopwatch;
        private Animator animator;
        private BaseState.HitStopCachedState hitStopCachedState;
        private Vector3 storedVelocity;
        private SpiralEnergyComponent spiralEnergy;

        protected virtual void PlayAttackAnimation()
        {
            base.PlayCrossfade("Gesture, Override", "DrillRush" + (1 + swingIndex), this.playbackRateString, this.duration, 0.05f);
        }

        protected virtual void SetNextState()
        {
            int index = this.swingIndex;
            if (index == 0) index = 1;
            else index = 0;

            this.outer.SetNextState(new LagannDrillRush
            {
                swingIndex = index
            });
        }

        public override void OnEnter()
        {
            base.OnEnter();
            this.spiralEnergy = base.GetComponent<SpiralEnergyComponent>();
            this.muzzleString = swingIndex % 2 == 0 ? "LeftDrillMuzzle" : "RightDrillMuzzle";
            this.hitEffectPrefab = Modules.TTGLAssets.punchImpactEffect;
            this.impactSound = Modules.TTGLAssets.drillRushHitSoundEvent.index;
            
            this.duration = this.baseDuration / this.attackSpeedStat;
            this.earlyExitTime = this.baseEarlyExitTime / this.attackSpeedStat;
            this.hasFired = false;
            this.animator = base.GetModelAnimator();
            base.StartAimMode(0.5f + this.duration, false);


            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();

            if (modelTransform)
            {
                hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == this.hitboxName);
            }

            this.PlayAttackAnimation();

            this.attack = new OverlapAttack();
            this.attack.damageType = this.damageType;
            this.attack.attacker = base.gameObject;
            this.attack.inflictor = base.gameObject;
            this.attack.teamIndex = base.GetTeam();
            this.attack.damage = damageCoefficient * this.damageStat;
            this.attack.procCoefficient = procCoefficient;
            this.attack.hitEffectPrefab = this.hitEffectPrefab;
            this.attack.forceVector = this.bonusForce;
            this.attack.pushAwayForce = this.pushForce;
            this.attack.hitBoxGroup = hitBoxGroup;
            this.attack.isCrit = base.RollCrit();
            this.attack.impactSound = this.impactSound;
        }

        public override void OnExit()
        {
            if (!this.hasFired) this.FireAttack();

            base.OnExit();
        }

        protected virtual void OnHitEnemyAuthority()
        {
            if (!this.hasHopped)
            {
                if (base.characterMotor && !base.characterMotor.isGrounded)
                {
                    base.SmallHop(base.characterMotor, this.hitHopVelocity);
                }

                this.hasHopped = true;
            }

            if (!this.inHitPause)
            {
                this.storedVelocity = base.characterMotor.velocity;
                this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, this.playbackRateString);
                this.hitPauseTimer = this.hitStopDuration / this.attackSpeedStat;
                this.inHitPause = true;
            }

            if (LagannDrillRush.spiralEnergyPercentagePerHit != 0f && this.spiralEnergy)
            {
                this.spiralEnergy.AddSpiralEnergyAuthority(LagannDrillRush.spiralEnergyPercentagePerHit * SpiralEnergyComponent.C_SPIRALENERGYCAP);
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
                    base.AddRecoil(-1f * this.attackRecoil, -2f * this.attackRecoil, -0.5f * this.attackRecoil, 0.5f * this.attackRecoil);
                }
            }

            if (base.isAuthority)
            {
                if (this.attack.Fire())
                {
                    this.OnHitEnemyAuthority();
                }
            }
        }
        
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.hitPauseTimer -= Time.deltaTime;

            if (this.hitPauseTimer <= 0f && this.inHitPause)
            {
                base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                this.inHitPause = false;
                base.characterMotor.velocity = this.storedVelocity;
            }

            if (!this.inHitPause)
            {
                this.stopwatch += Time.deltaTime;
            }
            else
            {
                if (base.characterMotor) base.characterMotor.velocity = Vector3.zero;
                if (this.animator) this.animator.SetFloat(this.playbackRateString, 0f);
            }

            if (this.stopwatch >= (this.duration * this.attackStartTime) && this.stopwatch <= (this.duration * this.attackEndTime))
            {
                this.FireAttack();
            }

            if (base.fixedAge >= (this.duration - this.earlyExitTime) && base.isAuthority)
            {
                if (base.inputBank.skill1.down)
                {
                    if (!this.hasFired) this.FireAttack();
                    this.SetNextState();
                    return;
                }
            }

            if (base.fixedAge >= this.duration)
            {
                base.PlayAnimation("Gesture, Override", "BufferEmpty");
                if (base.isAuthority)
                {
                    this.outer.SetNextStateToMain();
                }
                return;
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
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.swingIndex = reader.ReadInt32();
        }
    }
}