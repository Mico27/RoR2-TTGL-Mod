using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using TTGL_Survivor.Modules;
using UnityEngine;
using UnityEngine.UI;

namespace TTGL_Survivor.UI
{
    // just a class to run some custom code for things like weapon models
    public class GroundSkillDef : SkillDef
    {
        
        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            return IsGrounded(skillSlot) && base.CanExecute(skillSlot);
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return IsGrounded(skillSlot) && base.IsReady(skillSlot);
        }

        private bool IsGrounded([NotNull] GenericSkill skillSlot)
        {
            return (skillSlot &&
                skillSlot.characterBody &&
                skillSlot.characterBody.characterMotor &&
                skillSlot.characterBody.characterMotor.isGrounded);
        }

    }
}