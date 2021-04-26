using EntityStates;
using EntityStates.Huntress;
using RoR2;
using RoR2.Projectile;
using TTGL_Survivor.Modules;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenLagannSplit : BaseSkillState
    {
        public static float baseDuration = 18f;

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnExit()
        {
            TransformToLagann();
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();        
            if (base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        private void TransformToLagann()
        {
            if (NetworkServer.active && base.characterBody && base.characterBody.master)
            {
                var master = base.characterBody.master;
                master.TransformBody("LagannBody");
            }
        }
    }
}