using EntityStates;
using EntityStates.Huntress.HuntressWeapon;
using RoR2;
using UnityEngine;
using RoR2.Projectile;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenThrowBoulder : BaseSkillState
    {
        public static float damageCoefficient = 10f;
        public static float procCoefficient = 1f;
        public static float baseThrowDuration = 2.2f;
        public static float throwForce = 80f;

        private float throwDuration;
        private float fireTime;
        private bool hasFired;
        private ChildLocator childLocator;

        public override void OnEnter()
        {
            base.OnEnter();
            this.throwDuration = GurrenThrowBoulder.baseThrowDuration / this.attackSpeedStat;
            this.fireTime = 0.5f * this.throwDuration;
            base.characterBody.SetAimTimer(2f);
            var animator = base.GetModelAnimator();
            animator.SetBool("isHoldingObject", false);
            base.PlayAnimation("FullBody, Override", "GURREN_ThrowingObject", "skill4.playbackRate", this.throwDuration);
            childLocator = base.GetModelChildLocator();
            if (base.inputBank && base.characterDirection)
            {
                base.characterDirection.forward = base.inputBank.aimDirection;
            }
        }

        public override void OnExit()
        {
            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            base.OnExit();
        }


        private void Fire()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;
                //Util.PlayAttackSpeedSound(ThrowGlaive.attackSoundString, base.gameObject, this.attackSpeedStat);

                if (base.isAuthority && childLocator)
                {
                    var origin = childLocator.FindChild("BigBoulderMuzzle");
                    Ray aimRay = base.GetAimRay();
                    ProjectileManager.instance.FireProjectile(TTGL_Survivor.Modules.Projectiles.bigBoulderPrefab,
                        (origin)? origin.position: aimRay.origin,
                        Util.QuaternionSafeLookRotation(aimRay.direction),
                        base.gameObject,
                        GurrenThrowBoulder.damageCoefficient * this.damageStat,
                        4000f,
                        base.RollCrit(),
                        DamageColorIndex.Default,
                        null,
                        GurrenThrowBoulder.throwForce);
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

            if (base.fixedAge >= this.throwDuration && base.isAuthority)
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