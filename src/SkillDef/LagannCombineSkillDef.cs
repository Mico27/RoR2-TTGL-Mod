using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using TTGL_Survivor.Modules;
using TTGL_Survivor.Modules.Components;
using UnityEngine;
using UnityEngine.UI;

namespace TTGL_Survivor.UI
{
    // just a class to run some custom code for things like weapon models
    public class LagannCombineSkillDef : SpiralEnergySkillDef
    {
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new LagannCombineSkillDef.LagannCombineInstanceData { source = skillSlot.characterBody.GetComponent<SpiralEnergyComponent>() };
        }

        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            return CanCombine(skillSlot) && base.CanExecute(skillSlot);
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return CanCombine(skillSlot) && base.IsReady(skillSlot);
        }

        private bool CanCombine([NotNull] GenericSkill skillSlot)
        {
            var instanceData = ((LagannCombineSkillDef.LagannCombineInstanceData)skillSlot.skillInstanceData);
            if (!instanceData.gurrenMinionCache && skillSlot && skillSlot.characterBody && skillSlot.characterBody.master)
            {
                instanceData.gurrenMinionCache = GurrenMinionCache.GetOrSetGurrenStatusCache(skillSlot.characterBody.master);
            }
            return instanceData.gurrenMinionCache && instanceData.gurrenMinionCache.gurrenMinion;
        }

        protected class LagannCombineInstanceData : SpiralEnergyInstanceData
        {
            public GurrenMinionCache gurrenMinionCache { get; set; }
        }
    }
}