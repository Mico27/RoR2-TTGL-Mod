using EntityStates;
using EntityStates.Huntress.HuntressWeapon;
using RoR2;
using UnityEngine;
using RoR2.Projectile;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenLagannThrowingShades : BaseSkillState
    {
        public static float damageCoefficient = 3.0f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 2.3f;
        public static float throwForce = 200f;
        // public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");

        private float duration;
        private float fireTime;
        private bool hasFired;
        private Animator animator;
        private string muzzleString;
        private Transform yokoTargetBase;
        private Vector3 yokoTargetBaseDefaultLocalPos;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = GurrenLagannThrowingShades.baseDuration / this.attackSpeedStat;
            this.fireTime = 0.4f * this.duration;
            base.characterBody.SetAimTimer(2f);
            this.animator = base.GetModelAnimator();
            this.muzzleString = "ShadesMuzzle";
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
                    ProjectileManager.instance.FireProjectile(TTGL_Survivor.Modules.Projectiles.shadesWhirlPrefab,
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