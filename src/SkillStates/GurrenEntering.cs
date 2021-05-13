using UnityEngine;
using EntityStates;
using RoR2;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenEntering : BaseState
    {
        public const float duration = 6.1f;
        public override void OnEnter()
        {
            base.OnEnter();
            base.PlayAnimation("Body", "GURREN_Interact_Activate");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && (base.fixedAge >= GurrenEntering.duration))
            {
                this.outer.SetNextStateToMain();
            }
        }
    }
}