using EntityStates;
using EntityStates.ClayBruiser.Weapon;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.GoldGat;
using EntityStates.Mage.Weapon;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using TTGL_Survivor.Orbs;
using UnityEngine;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenDrillBlaster : BaseSkillState
    {
        public const float damageCoefficient = 0.50f;
        public const float procCoefficient = 0.5f;
        public const float maxDuration = 3f;
        Animator animator;
        ChildLocator childLocator;
        List<Tuple<string, Transform>> muzzles;

        public float totalStopwatch;
        private float stopwatch;
        private float fireFrequency;
        private uint loopSoundID;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = base.GetModelAnimator();
            animator.SetBool("isFiring", true);
            childLocator = base.GetModelChildLocator();
            if (childLocator)
            {
                muzzles = new List<Tuple<string, Transform>>();
                muzzles.Add(new Tuple<string, Transform>("DrillMuzzle1", childLocator.FindChild("DrillMuzzle1")));
                muzzles.Add(new Tuple<string, Transform>("DrillMuzzle2", childLocator.FindChild("DrillMuzzle2")));
                muzzles.Add(new Tuple<string, Transform>("DrillMuzzle3", childLocator.FindChild("DrillMuzzle3")));
                muzzles.Add(new Tuple<string, Transform>("DrillMuzzle4", childLocator.FindChild("DrillMuzzle4")));
            }
            this.loopSoundID = Util.PlaySound(GoldGatFire.windUpSoundString, base.gameObject);
            this.FireBullet();
        }

        private void FireBullet()
        {
            base.StartAimMode(2f);
            float t = Mathf.Clamp01(this.totalStopwatch / GoldGatFire.windUpDuration);
            this.fireFrequency = Mathf.Lerp(GoldGatFire.minFireFrequency, GoldGatFire.maxFireFrequency, t) * base.attackSpeedStat;
            float num = Mathf.Lerp(GoldGatFire.minSpread, GoldGatFire.maxSpread, t);
            Util.PlaySound(FirePistol2.firePistolSoundString, base.gameObject);
            if (animator)
            {
                animator.SetFloat("skill2.playbackRate", this.fireFrequency);
            }
            foreach (var muzzle in muzzles)
            {
                if (base.isAuthority)
                {
                    new BulletAttack
                    {
                        owner = base.gameObject,
                        aimVector = base.inputBank.aimDirection,
                        origin = muzzle.Item2.position,
                        falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                        force = FirePistol2.force,
                        damage = base.damageStat * GurrenDrillBlaster.damageCoefficient,
                        damageColorIndex = DamageColorIndex.Item,
                        bulletCount = 1U,
                        minSpread = 0f,
                        maxSpread = num,
                        radius = 2f,
                        tracerEffectPrefab = FirePistol2.tracerEffectPrefab,
                        hitEffectPrefab = FirePistol2.hitEffectPrefab,
                        isCrit = base.RollCrit(),
                        procCoefficient = GurrenDrillBlaster.procCoefficient,
                        muzzleName = muzzle.Item1,
                        weapon = base.gameObject
                    }.Fire();
                }
                EffectManager.SimpleMuzzleFlash(FirePistol2.muzzleEffectPrefab, base.gameObject, muzzle.Item1, false);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.totalStopwatch += Time.deltaTime;
            this.stopwatch += Time.deltaTime;
            AkSoundEngine.SetRTPCValueByPlayingID(GoldGatFire.windUpRTPC, Mathf.InverseLerp(GoldGatFire.minFireFrequency, GoldGatFire.maxFireFrequency, this.fireFrequency) * 100f, this.loopSoundID);
            
            if (this.stopwatch > 1f / this.fireFrequency)
            {
                this.stopwatch = 0f;
                this.FireBullet();
            }
            if (base.isAuthority && base.fixedAge >= maxDuration)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            animator.SetBool("isFiring", false);
            AkSoundEngine.StopPlayingID(this.loopSoundID);
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}