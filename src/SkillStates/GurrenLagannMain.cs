using UnityEngine;
using EntityStates;
using RoR2;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenLagannMain : GenericCharacterMain
    {
        private Animator animator;
        private bool hadFullSpiralPowerBuff;
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

                UpdateMaxSpiralPowerBuffEffects();
            }
            
        }

        private void UpdateMaxSpiralPowerBuffEffects()
        {
            if (this.HasBuff(Modules.Buffs.maxSpiralPowerBuff))
            {
                if (!hadFullSpiralPowerBuff)
                {
                    hadFullSpiralPowerBuff = true;
                    //this.animator.SetBool("spiralPowerOverflow", true);
                }                
            }
            else
            {
                if (hadFullSpiralPowerBuff)
                {
                    hadFullSpiralPowerBuff = false;
                    //this.animator.SetBool("spiralPowerOverflow", false);
                }
            }
        }
      
        public override void OnExit()
        {
            base.OnExit();
        }
    }
}