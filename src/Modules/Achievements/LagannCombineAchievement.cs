using RoR2;
using RoR2.Achievements;
using System;
using TTGL_Survivor.Modules.Components;
using TTGL_Survivor.SkillStates;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TTGL_Survivor.Modules.Achievements
{
    [RegisterAchievement(TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_ACHIEVEMENT_ID",
           TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_UNLOCKABLE_ID",
           TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_PREREQUISITE_ID", 5)]
    internal class LagannCombineAchievement : BaseAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyIndex.None; //BodyCatalog.FindBodyIndex(Modules.Survivors.MyCharacter.instance.fullBodyName);
        }

        public override void OnInstall()
        {
            base.OnInstall();
            LagannCombine.onLagannCombineGlobal += LagannCombine_onLagannCombineGlobal;
        }

        private void LagannCombine_onLagannCombineGlobal()
        {
            base.Grant();
        }

        public override void OnUninstall()
        {
            base.OnUninstall();
            LagannCombine.onLagannCombineGlobal -= LagannCombine_onLagannCombineGlobal;
        }
    }
}