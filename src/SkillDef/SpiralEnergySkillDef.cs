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
        public float energyCost { get; set; }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new SpiralEnergySkillDef.SpiralEnergyInstanceData { source = skillSlot.characterBody.GetComponent<SpiralEnergyComponent>() };            
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
            var source = ((SpiralEnergySkillDef.SpiralEnergyInstanceData)skillSlot.skillInstanceData).source;
            return (source && source.energy >= energyCost);
        }
        protected class SpiralEnergyInstanceData : SkillDef.BaseSkillInstanceData
        {
            public SpiralEnergyComponent source { get; set; }
        }
    }
}