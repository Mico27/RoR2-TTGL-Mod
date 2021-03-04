using UnityEngine;
using EntityStates;
using RoR2;

namespace TTGL_Survivor.SkillStates
{
    public class LagannMain : GenericCharacterMain
    {
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (this.animator)
            {
                float i = 1;
                if (this.animator.GetBool("isGrounded")) i = 0;
                this.animator.SetFloat("inAir", i);

                if (this.HasBuff(Modules.Buffs.maxSpiralPowerBuff))
                {
                    this.animator.SetBool("spiralPowerOverflow", true);
                }
                else
                {
                    this.animator.SetBool("spiralPowerOverflow", false);
                }
            }
        }
    }
}