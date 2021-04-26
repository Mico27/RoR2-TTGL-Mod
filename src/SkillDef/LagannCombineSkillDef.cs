using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using TTGL_Survivor.Modules;
using UnityEngine;
using UnityEngine.UI;

namespace TTGL_Survivor.UI
{
    // just a class to run some custom code for things like weapon models
    public class LagannCombineSkillDef : SkillDef
    {
        private SpiralEnergyComponent source { get; set; }
        private bool hasRequiredTeammate { get; set; }

        public float energyCost { get; set; }

        public string requiredTeammateBodyName { get; set; }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            if (skillSlot.characterBody)
            {
                this.source = skillSlot.characterBody.GetComponent<SpiralEnergyComponent>();
            }
            TeamComponent.onJoinTeamGlobal += TeamComponent_onJoinTeamGlobal;
            this.hasRequiredTeammate = CheckRequiredTeammate();
            return base.OnAssigned(skillSlot);
        }

        public override void OnUnassigned([NotNull] GenericSkill skillSlot)
        {
            TeamComponent.onJoinTeamGlobal -= TeamComponent_onJoinTeamGlobal;
            base.OnUnassigned(skillSlot);
        }

        private bool CheckRequiredTeammate()
        {
            if (string.IsNullOrEmpty(requiredTeammateBodyName))
            {
                return true;
            }
            var bodyIndex = BodyCatalog.FindBodyIndex(requiredTeammateBodyName);
            var players = TeamComponent.GetTeamMembers(TeamIndex.Player);
            if (players != null)
            {
                foreach (var player in players)
                {
                    if (player.body && player.body.bodyIndex == bodyIndex)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
                
        private void TeamComponent_onJoinTeamGlobal(TeamComponent arg1, TeamIndex arg2)
        {
            this.hasRequiredTeammate = CheckRequiredTeammate();
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
            return (hasRequiredTeammate && source && source.energy >= energyCost);
        }
    }
}