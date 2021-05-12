using EntityStates;
using EntityStates.Huntress.HuntressWeapon;
using RoR2;
using UnityEngine;
using RoR2.Projectile;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenHoldBoulder : BaseSkillState
    {
        public static float maxHoldDuration = 5.0f;
        public override void OnEnter()
        {
            base.OnEnter();
            base.characterBody.SetAimTimer(2f);
            var animator = base.GetModelAnimator();
            animator.SetBool("isHoldingObject", true);
            base.PlayAnimation("FullBody, Override", "GURREN_HoldingObject");
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.characterMotor && base.characterDirection && base.inputBank)
            {
                base.characterMotor.velocity = Vector3.zero;
                base.characterDirection.forward = base.inputBank.aimDirection;
            }
            if (base.isAuthority && ((base.fixedAge >= GurrenHoldBoulder.maxHoldDuration) || (base.inputBank && (base.inputBank.skill1.justPressed || base.inputBank.skill2.justPressed || base.inputBank.skill3.justPressed || base.inputBank.skill4.justPressed))))
            {
                this.outer.SetNextState(new GurrenThrowBoulder());
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}