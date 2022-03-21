using EntityStates;
using EntityStates.Huntress.HuntressWeapon;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using TTGL_Survivor.Modules;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenLagannThrowingShades : BaseSkillState
    {
        public static float damageCoefficient = 2.5f;
        public static float procCoefficient = 0.3f;
        public static float baseDuration = 2.3f;
        public static float throwForce = 200f;

        private float duration;
        private float fireTime;
        private bool hasFired;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = GurrenLagannThrowingShades.baseDuration / this.attackSpeedStat;
            this.fireTime = 0.4f * this.duration;
            base.characterBody.SetAimTimer(2f);
            var animationString = "GurrenLagannThrowShadesRight";
            if (this.skillLocator && this.skillLocator.secondary && this.skillLocator.secondary.stock % 2 == 1)
            {
                animationString = "GurrenLagannThrowShadesLeft";
            }
            if (this.isGrounded && this.characterMotor && this.characterMotor.velocity.magnitude <= 1.0f)
            {
                base.PlayAnimation("FullBody, Override", animationString, "skill2.playbackRate", this.duration);
            }
            else
            {
                base.PlayAnimation("Gesture, Override", animationString, "skill2.playbackRate", this.duration);
            }
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
                Util.PlayAttackSpeedSound(ThrowGlaive.attackSoundString, base.gameObject, this.attackSpeedStat);

                if (base.isAuthority)
                {
                    Ray aimRay = base.GetAimRay();
                    GameObject prefab = (Config.useLegacyGigaDrillBreak) ?
                        TTGL_Survivor.Modules.Projectiles.gigaDrillProjectilePrefab :
                        TTGL_Survivor.Modules.Projectiles.shadesWhirlPrefab;
                    ProjectileManager.instance.FireProjectile(prefab,
                        aimRay.origin,
                        Util.QuaternionSafeLookRotation(aimRay.direction),
                        base.gameObject,
                        GurrenLagannThrowingShades.damageCoefficient * this.damageStat,
                        100f,
                        base.RollCrit(),
                        DamageColorIndex.Default,
                        null,
                        GurrenLagannThrowingShades.throwForce);
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