using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.Modules.Components
{
    // just a class to run some custom code for things like weapon models
    public class GurrenLagannController : MonoBehaviour
    {
        private CharacterBody body;
        private Animator animator;
        private bool hadFullSpiralPowerBuff;

        public void Awake()
        {
            this.body = base.GetComponent<CharacterBody>();
            var model = base.GetComponent<ModelLocator>();
            this.animator = model.modelTransform.GetComponent<Animator>();
        }


        private void FixedUpdate()
        {
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
            if (this.body.HasBuff(Modules.Buffs.maxSpiralPowerBuff))
            {
                if (!hadFullSpiralPowerBuff)
                {
                    hadFullSpiralPowerBuff = true;
                    this.animator.SetBool("spiralPowerOverflow", true);
                }
            }
            else
            {
                if (hadFullSpiralPowerBuff)
                {
                    hadFullSpiralPowerBuff = false;
                    this.animator.SetBool("spiralPowerOverflow", false);
                }
            }
        }
    }
}