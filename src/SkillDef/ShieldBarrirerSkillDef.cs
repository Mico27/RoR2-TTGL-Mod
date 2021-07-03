using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using TTGL_Survivor.Modules;
using UnityEngine;
using UnityEngine.UI;

namespace TTGL_Survivor.UI
{
    // just a class to run some custom code for things like weapon models
    public class ShieldBarrirerSkillDef : SkillDef
    {
        
        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            return HasShieldOrBarrier(skillSlot) && base.CanExecute(skillSlot);
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return HasShieldOrBarrier(skillSlot) && base.IsReady(skillSlot);
        }

        private bool HasShieldOrBarrier([NotNull] GenericSkill skillSlot)
        {
            return (skillSlot &&
                skillSlot.characterBody &&
                skillSlot.characterBody.healthComponent &&
                (skillSlot.characterBody.healthComponent.shield > 0f ||
                skillSlot.characterBody.healthComponent.barrier > 0f));
        }

    }
}