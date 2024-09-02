using EntityStates;
using EntityStates.Commando;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.RoR2Content;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenLagannTornadoKick : BaseSkillState
    {
        public static float horizontal_duration = 1.5f;
        public static float vertical_duration = 2.0f;
        public static float initialSpeedCoefficient = 2.5f;
        public static float finalSpeedCoefficient = 2.0f;
        public static float damageCoefficient = 1.5f;
        public static float jumpVelocity = 7f;
        public static bool canControlDirection = false;
        public static float procCoefficient = 1f;

        public static string hitboxName = "DammageHitbox";

        public static float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;

        private float burstSpeed;
        private Vector3 burstDirection;
        private Animator animator;
        private Vector3 previousPosition;


        public int swingIndex;

        protected float duration = 0f;
        protected DamageType damageType = DamageType.Generic;
        
        protected float pushForce = 300f;
        protected Vector3 bonusForce = Vector3.zero;
        protected float attackStartTime = 0.0f;
        protected float attackEndTime = 1.0f;
        protected float baseEarlyExitTime = 0.4f;
        protected float hitStopDuration = 0.012f;
        protected float attackRecoil = 0.75f;
        protected float hitHopVelocity = 4f;

        protected string swingSoundString = "";
        protected string hitSoundString = "";
        protected string muzzleString = "";
        protected string playbackRateString = "";
        protected GameObject swingEffectPrefab;
        protected GameObject hitEffectPrefab;

        private bool hasFired;
        private float hitPauseTimer;
        private OverlapAttack attack;
        private bool inHitPause;
        private bool hasHopped;
        private float stopwatch;
        private BaseState.HitStopCachedState hitStopCachedState;
        private Vector3 storedVelocity;

        private float hitIntervalStopwatch = 0.0f;
        private float hitInterval = 0.5f;

        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
            this.playbackRateString = "skill3.playbackRate";

            if (base.inputBank)
            {
                this.burstDirection = ((base.inputBank.moveVector == Vector3.zero) ? Vector3.up : base.inputBank.moveVector).normalized;
            }
            bool isVertical = (this.burstDirection == Vector3.up);
            this.duration = (isVertical) ? GurrenLagannTornadoKick.vertical_duration : GurrenLagannTornadoKick.horizontal_duration;
            this.RecalculateBurstSpeed(isVertical);
            
            this.damageType = DamageType.BypassArmor;
            this.pushForce = 500f;
            this.bonusForce = Vector3.zero;
            this.attackStartTime = 0.1f;
            this.attackEndTime = 0.9f;
            this.baseEarlyExitTime = 0.3f;
            this.hitStopDuration = 0.115f;
            this.attackRecoil = 0.75f;
            this.hitHopVelocity = 6f;
            this.hitInterval = 0.5f * this.attackSpeedStat;

            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();

            if (modelTransform)
            {
                hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == GurrenLagannTornadoKick.hitboxName);
            }

            this.attack = new OverlapAttack();
            this.attack.damageType = this.damageType;
            this.attack.attacker = base.gameObject;
            this.attack.inflictor = base.gameObject;
            this.attack.teamIndex = base.GetTeam();
            this.attack.damage = GurrenLagannTornadoKick.damageCoefficient * this.damageStat;
            this.attack.procCoefficient = procCoefficient;
            this.attack.hitEffectPrefab = this.hitEffectPrefab;
            this.attack.forceVector = this.bonusForce;
            this.attack.pushAwayForce = this.pushForce;
            this.attack.hitBoxGroup = hitBoxGroup;
            this.attack.isCrit = base.RollCrit();
            //this.attack.impactSound = Modules.Assets.drillRushHitSoundEvent.index;

            if (base.characterMotor)
            {
                if (isVertical)
                {
                    base.SmallHop(base.characterMotor, this.burstSpeed);
                }
                else
                {
                    base.characterMotor.velocity.y = 0f;
                    base.characterMotor.velocity = this.burstDirection * this.burstSpeed;
                }
            }

            Vector3 b = base.characterMotor ? base.characterMotor.velocity : Vector3.zero;
            this.previousPosition = base.transform.position - b;

            base.PlayAnimation("FullBody, Override", "GurrenLagannHurricaneKick", this.playbackRateString, this.duration);
            Util.PlaySound(SlideState.soundString, base.gameObject);
            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(Buffs.HiddenInvincibility);
            }
        }

        private void RecalculateBurstSpeed(bool isVertical)
        {
            this.burstSpeed = ((!isVertical)?this.moveSpeedStat: jumpVelocity) * Mathf.Lerp(GurrenLagannTornadoKick.initialSpeedCoefficient, GurrenLagannTornadoKick.finalSpeedCoefficient, base.fixedAge / this.duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (canControlDirection && base.inputBank)
            {
                this.burstDirection = ((base.inputBank.moveVector == Vector3.zero) ? Vector3.up : base.inputBank.moveVector).normalized;
            }
            bool isVertical = (this.burstDirection == Vector3.up);
            this.RecalculateBurstSpeed(isVertical);
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
                this.hitIntervalStopwatch += Time.deltaTime;
            }
            else
            {
                if (base.characterMotor) base.characterMotor.velocity = Vector3.zero;
                if (this.animator) this.animator.SetFloat(this.playbackRateString, 0f);
            }

            if (this.stopwatch >= (this.duration * this.attackStartTime) && this.stopwatch <= (this.duration * this.attackEndTime))
            {
                if (this.hitIntervalStopwatch >= this.hitInterval)
                {
                    this.hitIntervalStopwatch = 0.0f;
                    this.attack.ignoredHealthComponentList.Clear();
                }
                this.FireAttack();
            }
            if (!isVertical && base.characterDirection) base.characterDirection.forward = this.burstDirection;
            //if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = Mathf.Lerp(SpiralBurst.dodgeFOV, 60f, base.fixedAge / this.duration);
            if (!isVertical)
            {
                Vector3 normalized = (base.transform.position - this.previousPosition).normalized;
                if (base.characterMotor && normalized != Vector3.zero)
                {
                    Vector3 vector = normalized * this.burstSpeed;
                    float d = Mathf.Max(Vector3.Dot(vector, this.burstDirection), 0f);
                    vector = this.burstDirection * d;
                    vector.y = 0f;

                    base.characterMotor.velocity = vector;
                }
            }
            this.previousPosition = base.transform.position;

            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }
        
        protected virtual void OnHitEnemyAuthority()
        {
            Util.PlaySound("TTGLDrillRushHit", base.gameObject);

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
                Util.PlayAttackSpeedSound(this.swingSoundString, base.gameObject, this.attackSpeedStat);

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

        public override void OnExit()
        {
            //if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = -1f;
            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            base.OnExit();

            if (NetworkServer.active) base.characterBody.RemoveBuff(Buffs.HiddenInvincibility);
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(this.burstDirection);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.burstDirection = reader.ReadVector3();
        }
    }
}