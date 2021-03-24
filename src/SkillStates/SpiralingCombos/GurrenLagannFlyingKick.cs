using RoR2;
using UnityEngine;
using EntityStates;
using RoR2.Audio;
using System;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenLagannFlyingKick : GurrenLagannBaseCombo
    {
        public const float c_DamageCoefficient = 3.0f;
        private float burstSpeed;
        private Vector3 burstDirection;
        private Vector3 previousPosition;

        protected override void PlayAttackAnimation()
        {
            base.PlayCrossfade("FullBody, Override", "GurrenLagannFlyingKick", this.playbackRateString, this.duration, 0.05f);
        }

        protected override OverlapAttack CreateAttack(HitBoxGroup hitBoxGroup)
        {
            var attack = new OverlapAttack();
            attack.damageType = DamageType.Generic;
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = base.GetTeam();
            attack.damage = c_DamageCoefficient * this.damageStat;
            attack.procCoefficient = 0.5f;
            attack.hitEffectPrefab = this.hitEffectPrefab;
            attack.forceVector = Vector3.zero;
            attack.pushAwayForce = 700f;
            attack.hitBoxGroup = hitBoxGroup;
            attack.isCrit = base.RollCrit();
            attack.impactSound = this.impactSound;
            return attack;
        }

        public override void OnEnter()
        {
            this.baseDuration = 1.3f;
            this.attackStartTime = 0.1f;
            this.attackEndTime = 0.9f;
            this.baseEarlyExitTime = 0.3f;
            this.hitStopDuration = 0.115f;
            base.OnEnter();
            if (base.characterDirection)
            {
                this.burstDirection = base.characterDirection.forward.normalized;
            }
            this.RecalculateBurstSpeed();
            if (base.characterMotor)
            {
                base.characterMotor.velocity.y = 0f;
                base.characterMotor.velocity = this.burstDirection * this.burstSpeed;
            }
            Vector3 b = base.characterMotor ? base.characterMotor.velocity : Vector3.zero;
            this.previousPosition = base.transform.position - b;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.RecalculateBurstSpeed();
            if (base.characterDirection) base.characterDirection.forward = this.burstDirection;
            Vector3 normalized = (base.transform.position - this.previousPosition).normalized;
            if (base.characterMotor && normalized != Vector3.zero)
            {
                Vector3 vector = normalized * this.burstSpeed;
                float d = Mathf.Max(Vector3.Dot(vector, this.burstDirection), 0f);
                vector = this.burstDirection * d;
                vector.y = 0f;

                base.characterMotor.velocity = vector;
            }
            this.previousPosition = base.transform.position;
        }

        private void RecalculateBurstSpeed()
        {
            this.burstSpeed = this.moveSpeedStat * Mathf.Lerp(LagannSpiralBurst.initialSpeedCoefficient, LagannSpiralBurst.finalSpeedCoefficient, base.fixedAge / this.duration);
        }
    }
}