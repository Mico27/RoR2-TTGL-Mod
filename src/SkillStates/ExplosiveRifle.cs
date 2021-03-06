using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace TTGL_Survivor.SkillStates
{
    public class ExplosiveRifle : BaseSkillState
    {
        public static float damageCoefficient = 1.5f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.65f;
        public static float throwForce = 200f;

        private float duration;
        private float fireTime;
        private bool hasFired;
        private Animator animator;
        private string muzzleString;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = ExplosiveRifle.baseDuration / this.attackSpeedStat;
            this.fireTime = 0.35f * this.duration;
            base.characterBody.SetAimTimer(2f);
            this.animator = base.GetModelAnimator();
            this.muzzleString = "Muzzle";

            base.PlayAnimation("Gesture, Override", "ShootRifle", "ShootRifle.playbackRate", this.duration);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void Fire()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;
                EffectManager.SimpleMuzzleFlash(Modules.Assets.yokoRifleMuzzleSmallEffect, base.gameObject, this.muzzleString, false);
                Util.PlaySound("TTGLTokoRifleFire", base.gameObject);

                if (base.isAuthority)
                {
                    Ray aimRay = base.GetAimRay();

                    ProjectileManager.instance.FireProjectile(TTGL_Survivor.Modules.Projectiles.explosiveRifleRoundPrefab, 
                        aimRay.origin, 
                        Util.QuaternionSafeLookRotation(aimRay.direction), 
                        base.gameObject,
                        ExplosiveRifle.damageCoefficient * this.damageStat, 
                        100f, 
                        base.RollCrit(), 
                        DamageColorIndex.Default, 
                        null,
                        ExplosiveRifle.throwForce);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.fireTime)
            {
                this.Fire();
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}