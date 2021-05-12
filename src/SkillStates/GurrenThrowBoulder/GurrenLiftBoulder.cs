using EntityStates;
using EntityStates.Huntress.HuntressWeapon;
using RoR2;
using UnityEngine;
using RoR2.Projectile;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenLiftBoulder : BaseSkillState
    {
        public static float baseLiftDuration = 2.5f;

        private float liftDuration;

        public override void OnEnter()
        {
            base.OnEnter();
            this.liftDuration = GurrenLiftBoulder.baseLiftDuration / this.attackSpeedStat;
            var animator = base.GetModelAnimator();
            animator.SetBool("isHoldingObject", true);
            base.PlayAnimation("FullBody, Override", "GURREN_LiftingObject", "skill4.playbackRate", this.liftDuration);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.liftDuration && base.isAuthority)
            {
                this.outer.SetNextState(new GurrenHoldBoulder());
            }
        }
        
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}