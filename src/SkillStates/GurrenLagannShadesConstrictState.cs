using UnityEngine;
using EntityStates;
using RoR2;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenLagannShadesConstrictState : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            this.FreezeCharacter(true);
        }

        public override void OnExit()
        {
            this.FreezeCharacter(false);
            base.OnExit();
        }

        private void FreezeCharacter(bool isEnabled)
        {            
            if (base.rigidbody && !base.rigidbody.isKinematic)
            {
                base.rigidbody.velocity = Vector3.zero;                
            }
            if (base.rigidbodyMotor)
            {
                base.rigidbodyMotor.enabled = !isEnabled;
                base.rigidbodyMotor.moveVector = Vector3.zero;
            }
            base.healthComponent.isInFrozenState = isEnabled;
            if (base.characterDirection)
            {
                base.characterDirection.moveVector = base.characterDirection.forward;
                base.characterDirection.enabled = !isEnabled;
            }
            if (base.characterMotor)
            {
                base.characterMotor.enabled = !isEnabled;
            }
            var modelAnimator = base.GetModelAnimator();
            if (modelAnimator)
            {
                modelAnimator.enabled = !isEnabled;
            }
        }


        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
        
        
    }
}