using EntityStates;
using EntityStates.Huntress;
using EntityStates.Huntress.HuntressWeapon;
using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using System;
using TTGL_Survivor.Modules;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenLagannGigaDrillMaximum : BaseSkillState
    {
        public static float energyCost = 50f;
        public static float c_DamageCoefficient = 7.5f;
        public static bool canBypassArmor = false;
        public int comboCounter;
        protected string hitboxName = "DammageHitbox";

        protected float baseDuration;
        protected float attackStartTime;
        protected float attackEndTime;
        protected float baseEarlyExitTime;
        protected float hitStopDuration;

        protected string playbackRateString = "skill4.playbackRate";
        protected GameObject hitEffectPrefab;
        protected NetworkSoundEventIndex impactSound;

        private float earlyExitTime;
        public float duration;
        private bool hasFired;
        private float hitPauseTimer;
        private OverlapAttack attack;
        private bool inHitPause;
        private float stopwatch;
        private Animator animator;
        private BaseState.HitStopCachedState hitStopCachedState;
        private Vector3 storedVelocity;

        protected void PlayAttackAnimation()
        {
            base.PlayAnimation("FullBody, Override", "GurrenLagannStandingTauntBattlecry", this.playbackRateString, this.duration);
        }

        protected OverlapAttack CreateAttack(HitBoxGroup hitBoxGroup)
        {
            var attack = new OverlapAttack();
            attack.damageType = (canBypassArmor)? (DamageType.Stun1s | DamageType.BypassArmor): DamageType.Stun1s;
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = base.GetTeam();
            attack.damage = c_DamageCoefficient * this.damageStat;
            attack.procCoefficient = 1.5f;
            attack.hitEffectPrefab = this.hitEffectPrefab;
            attack.forceVector = Vector3.zero;
            attack.pushAwayForce = 0f;
            attack.hitBoxGroup = hitBoxGroup;
            attack.isCrit = base.RollCrit();
            attack.impactSound = this.impactSound;
            return attack;
        }
        
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                var spiralEnergyComponent = base.characterBody.GetComponent<SpiralEnergyComponent>();
                if (spiralEnergyComponent)
                {
                    spiralEnergyComponent.NetworkEnergy -= energyCost;
                }
            }
            this.baseDuration = 3.0f;
            this.attackStartTime = 0.33f;
            this.attackEndTime = 0.75f;
            this.baseEarlyExitTime = 0.25f;
            this.hitStopDuration = 0.115f;
            this.hitEffectPrefab = Modules.Assets.punchImpactEffect;
            this.impactSound = Modules.Assets.drillRushHitSoundEvent.index;

            this.duration = this.baseDuration / this.attackSpeedStat;
            this.earlyExitTime = this.baseEarlyExitTime / this.attackSpeedStat;
            this.hasFired = false;
            this.animator = base.GetModelAnimator();


            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();

            if (modelTransform)
            {
                hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == this.hitboxName);
            }

            this.PlayAttackAnimation();

            this.attack = CreateAttack(hitBoxGroup);
        }

        public override void OnExit()
        {
            if (!this.hasFired) this.FireAttack();

            base.OnExit();
        }

        protected virtual void OnHitEnemyAuthority()
        {
            if (!this.inHitPause)
            {
                this.storedVelocity = base.characterMotor.velocity;
                this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, this.playbackRateString);
                this.hitPauseTimer = this.hitStopDuration / this.attackSpeedStat;
                this.inHitPause = true;
            }
        }

        private void FireAttack()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;
                Util.PlayAttackSpeedSound(ChargeArrow.chargeLoopStartSoundString, base.gameObject, this.attackSpeedStat);
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

            if (this.stopwatch >= (this.duration * this.attackStartTime) && this.stopwatch <= (this.duration * this.attackEndTime))
            {
                this.FireAttack();
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

    }
}