using EntityStates;
using EntityStates.Huntress;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenLagannGigaDrillBreak : BaseSkillState
    {        
        public override void OnEnter()
        {
            base.OnEnter();
            
        }

        public override void OnExit()
        {
            
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

    }
}