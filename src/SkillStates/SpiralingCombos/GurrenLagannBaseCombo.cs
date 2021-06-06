using RoR2;
using UnityEngine;
using EntityStates;
using RoR2.Audio;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;
using EntityStates.Merc;
using EntityStates.Huntress;

namespace TTGL_Survivor.SkillStates
{
    public abstract class GurrenLagannBaseCombo : BaseSkillState
    {
        static public float baseDamageCoeficient = 1.0f;
        static public float pullRadius = 20f;
        static public float pullForce = 80f;
        static public bool allBypassArmor = false;
        public int comboCounter;
        protected string hitboxName = "DammageHitbox";

        protected float baseDuration;
        protected float attackStartTime;
        protected float attackEndTime;
        protected float baseEarlyExitTime;
        protected float hitStopDuration;

        protected string playbackRateString = "skill1.playbackRate";
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

        public Transform pullOrigin;   
        public AnimationCurve pullStrengthCurve;
        public int maximumPullCount = int.MaxValue;
        private List<CharacterBody> pullList = new List<CharacterBody>();
        private bool pulling;

        protected abstract void PlayAttackAnimation();

        protected abstract OverlapAttack CreateAttack(HitBoxGroup hitBoxGroup);

        protected virtual void FollowUpComboState()
        {
            this.outer.SetNextState(new GurrenLagannSpiralingCombo
            {
                comboCounter = this.comboCounter,
            });
        }

        public override void OnEnter()
        {
            base.OnEnter();
            base.characterBody.SetAimTimer(2f);
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
            var childLocator = this.GetModelChildLocator();
            this.pullOrigin = childLocator.FindChild(this.hitboxName);
            this.pullStrengthCurve = AnimationCurve.EaseInOut(0.1f, 0f, 1f, 1f);

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
                Util.PlayAttackSpeedSound(BackflipState.dodgeSoundString, base.gameObject, this.attackSpeedStat);
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
                this.PullEnemies(Time.fixedDeltaTime);
                this.FireAttack();
            }

            if (base.fixedAge >= (this.duration - this.earlyExitTime) && base.isAuthority)
            {
                if (base.inputBank && base.inputBank.skill1.down)
                {
                    if (!this.hasFired) this.FireAttack();
                    this.FollowUpComboState();
                    return;
                }
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        private void PullEnemies(float deltaTime)
        {
            if (!this.pulling)
            {
                InitializePull();
            }
            for (int i = 0; i < this.pullList.Count; i++)
            {
                CharacterBody characterBody = this.pullList[i];
                if (characterBody && characterBody.transform)
                {
                    Vector3 vector = ((this.pullOrigin) ? this.pullOrigin.position : base.transform.position) - characterBody.corePosition;
                    float d = this.pullStrengthCurve.Evaluate(vector.magnitude / pullRadius);
                    Vector3 b = vector.normalized * d * deltaTime * pullForce;
                    CharacterMotor component = characterBody.GetComponent<CharacterMotor>();
                    if (component)
                    {
                        component.rootMotion += b;
                        if (component.useGravity)
                        {
                            component.rootMotion.y -= (Physics.gravity.y * deltaTime * d);
                        }
                    }
                    else
                    {
                        Rigidbody component2 = characterBody.GetComponent<Rigidbody>();
                        if (component2)
                        {
                            component2.velocity += b;
                        }
                    }
                }
            }
        }

        private void InitializePull()
        {
            if (this.pulling)
            {
                return;
            }
            this.pulling = true;
            Collider[] array = Physics.OverlapSphere(((this.pullOrigin) ? this.pullOrigin.position : base.transform.position), pullRadius, LayerIndex.defaultLayer.mask);
            int num = 0;
            int num2 = 0;
            while (num < array.Length && num2 < this.maximumPullCount)
            {
                HealthComponent component = array[num].GetComponent<HealthComponent>();
                if (component)
                {
                    TeamComponent component2 = component.GetComponent<TeamComponent>();
                    bool flag = false;
                    if (component2)
                    {
                        flag = (component2.teamIndex == base.GetTeam());
                    }
                    if (!flag)
                    {
                        this.AddToPullList(component.gameObject);
                        num2++;
                    }
                }
                num++;
            }
        }

        private void AddToPullList(GameObject affectedObject)
        {
            CharacterBody component = affectedObject.GetComponent<CharacterBody>();
            if (!this.pullList.Contains(component))
            {
                this.pullList.Add(component);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

    }
}