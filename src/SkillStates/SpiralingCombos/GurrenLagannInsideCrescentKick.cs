﻿using RoR2;
using UnityEngine;
using EntityStates;
using RoR2.Audio;
using System;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenLagannInsideCrescentKick : GurrenLagannBaseCombo
    {
        public const float c_DamageCoefficient = 2.8f;
        public const float procCoefficient = 0.8f;

        protected override void PlayAttackAnimation()
        {
            base.PlayCrossfade("FullBody, Override", "GurrenLagannInsideCrescentKick", this.playbackRateString, this.duration, 0.05f);
        }

        protected override OverlapAttack CreateAttack(HitBoxGroup hitBoxGroup)
        {
            var attack = new OverlapAttack();
            attack.damageType = DamageType.Generic;
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = base.GetTeam();
            attack.damage = c_DamageCoefficient * baseDamageCoeficient * this.damageStat;
            attack.procCoefficient = procCoefficient;
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
            this.baseDuration = 2.4f;
            this.attackStartTime = 0.4f;
            this.attackEndTime = 0.6f;
            this.baseEarlyExitTime = 0.5f;
            this.hitStopDuration = 0.115f;
            base.OnEnter();
        }
    }
}