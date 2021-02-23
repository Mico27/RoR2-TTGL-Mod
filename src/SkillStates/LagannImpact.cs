using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace TTGL_Survivor.SkillStates
{
    public class LagannImpact : BaseSkillState
    {
        public static float damageCoefficient = 15f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.65f;
        public static float throwForce = 80f;
        public static int bounceCount = 3;

        private float duration;
        private float fireTime;
        private bool hasFired;
        private Animator animator;
        private string muzzleString;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = LagannImpact.baseDuration / this.attackSpeedStat;
            this.fireTime = 0.35f * this.duration;
            base.characterBody.SetAimTimer(2f);
            this.animator = base.GetModelAnimator();
            this.muzzleString = "Muzzle";

            base.PlayAnimation("FullBody, Override", "LagannImpact1", "LagannImpact.playbackRate", this.duration);
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
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol.effectPrefab, base.gameObject, this.muzzleString, false);
                Util.PlaySound("HenryBombThrow", base.gameObject);

                if (base.isAuthority)
                {
                    Ray aimRay = base.GetAimRay();

                    /*
                    ProjectileManager.instance.FireProjectile(Modules.Projectiles.bombPrefab, 
                        aimRay.origin, 
                        Util.QuaternionSafeLookRotation(aimRay.direction), 
                        base.gameObject, 
                        LagannImpact.damageCoefficient * this.damageStat, 
                        4000f, 
                        base.RollCrit(), 
                        DamageColorIndex.Default, 
                        null, 
                        LagannImpact.throwForce);
                        */
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