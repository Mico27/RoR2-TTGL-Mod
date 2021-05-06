using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using TTGL_Survivor.Modules;
using UnityEngine;
using UnityEngine.UI;

namespace TTGL_Survivor.UI
{
    // just a class to run some custom code for things like weapon models
    public class SpiralEnergySkillDef : SkillDef
    {
        private SpiralEnergyComponent source { get; set; }

        public float energyCost { get; set; }
                
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            if (skillSlot.characterBody)
            {
                this.source = skillSlot.characterBody.GetComponent<SpiralEnergyComponent>();
            }
            return base.OnAssigned(skillSlot);
        }

        public override void OnUnassigned([NotNull] GenericSkill skillSlot)
        {
            base.OnUnassigned(skillSlot);
        }
        
        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            return HasEnergy(skillSlot) && base.CanExecute(skillSlot);
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return HasEnergy(skillSlot) && base.IsReady(skillSlot);
        }

        private bool HasEnergy([NotNull] GenericSkill skillSlot)
        {
            return (source && source.energy >= energyCost);
        }

    }
}