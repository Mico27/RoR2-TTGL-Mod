using RoR2;
using UnityEngine;
using EntityStates;
using RoR2.Audio;
using System;
using UnityEngine.Networking;
using EntityStates.Merc;
using EntityStates.Huntress;
using System.Collections.Generic;
using System.Linq;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenTripleSlash : BaseSkillState
    {
        public int comboCounter;
        protected string hitboxName = "DammageHitbox";

        protected float baseDuration;
        protected float hitStopDuration;

        protected string playbackRateString = "skill1.playbackRate";
        protected GameObject hitEffectPrefab;
        protected NetworkSoundEventIndex impactSound;

        public float duration;
        private bool hasFired;
        private float hitPauseTimer;
        private OverlapAttack attack;
        private bool inHitPause;
        private float stopwatch;
        private float resetAttackStopwatch;
        private Animator animator;
        private BaseState.HitStopCachedState hitStopCachedState;
        private Vector3 storedVelocity;

        public Transform pullOrigin;
        public float pullRadius;
        public float pullForce;
        public AnimationCurve pullStrengthCurve;
        public int maximumPullCount = int.MaxValue;
        private List<CharacterBody> pullList = new List<CharacterBody>();
        private bool pulling;

        public const float c_DamageCoefficient = 4.0f;

        private List<Tuple<float, float>> damageWindows;

        public override void OnEnter()
        {
            base.OnEnter();
            damageWindows = new List<Tuple<float, float>>();
            damageWindows.Add(new Tuple<float, float>(0.25f, 0.30f));
            damageWindows.Add(new Tuple<float, float>(0.37f, 0.42f));
            damageWindows.Add(new Tuple<float, float>(0.49f, 0.80f));
            this.baseDuration = 4.0f;
            this.hitStopDuration = 0.115f;
            base.characterBody.SetAimTimer(2f);
            this.hitEffectPrefab = Modules.Assets.punchImpactEffect;
            this.impactSound = Modules.Assets.drillRushHitSoundEvent.index;

            this.duration = this.baseDuration / this.attackSpeedStat;
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
            this.pullRadius = 20f;
            this.pullStrengthCurve = AnimationCurve.EaseInOut(0.1f, 0f, 1f, 1f);
            this.pullForce = 80f;

            this.PlayAttackAnimation();

            this.attack = CreateAttack(hitBoxGroup);
        }

        public override void OnExit()
        {
            if (!this.hasFired) this.FireAttack();
            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            base.OnExit();
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();
            var currentDamageWindow = damageWindows.FirstOrDefault();
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
                this.resetAttackStopwatch += Time.fixedDeltaTime;
            }
            else
            {
                if (base.characterMotor) base.characterMotor.velocity = Vector3.zero;
                if (this.animator) this.animator.SetFloat(this.playbackRateString, 0f);
            }

            if (currentDamageWindow != null && 
                this.stopwatch >= (this.duration * currentDamageWindow.Item1) && 
                this.stopwatch <= (this.duration * currentDamageWindow.Item2))
            {
                this.PullEnemies(Time.fixedDeltaTime);
                this.FireAttack();
            }

            if (currentDamageWindow != null && this.resetAttackStopwatch >= (this.duration * currentDamageWindow.Item2))
            {
                damageWindows.Remove(currentDamageWindow);
                this.resetAttackStopwatch = 0f;
                this.attack.ResetIgnoredHealthComponents();
            }
            
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        protected void PlayAttackAnimation()
        {
            base.PlayCrossfade("FullBody, Override", "GURREN_MeleeComboAttack", this.playbackRateString, this.duration, 0.05f);
        }

        protected OverlapAttack CreateAttack(HitBoxGroup hitBoxGroup)
        {
            var attack = new OverlapAttack();
            attack.damageType = DamageType.BypassArmor;
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = base.GetTeam();
            attack.damage = c_DamageCoefficient * this.damageStat;
            attack.procCoefficient = 0.5f;
            attack.hitEffectPrefab = this.hitEffectPrefab;
            attack.forceVector = Vector3.zero;
            attack.pushAwayForce = 0f;
            attack.hitBoxGroup = hitBoxGroup;
            attack.isCrit = base.RollCrit();
            attack.impactSound = this.impactSound;
            return attack;
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
                    float d = this.pullStrengthCurve.Evaluate(vector.magnitude / this.pullRadius);
                    Vector3 b = vector.normalized * d * deltaTime * this.pullForce;
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
            Collider[] array = Physics.OverlapSphere(((this.pullOrigin) ? this.pullOrigin.position : base.transform.position), this.pullRadius, LayerIndex.defaultLayer.mask);
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