using RoR2;
using System;
using TTGL_Survivor.Modules.Components;
using TTGL_Survivor.SkillStates;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TTGL_Survivor.Modules.Achievements
{
    internal class LagannCombineAchievement : ModdedUnlockable
    {
        public override string AchievementIdentifier { get; } = TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_ACHIEVEMENT_ID";
        public override string UnlockableIdentifier { get; } = TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_UNLOCKABLE_ID";
        public override string AchievementNameToken { get; } = TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_ACHIEVEMENT_NAME";
        public override string PrerequisiteUnlockableIdentifier { get; } = TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_PREREQUISITE_ID";
        public override string UnlockableNameToken { get; } = TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_UNLOCKABLE_NAME";
        public override string AchievementDescToken { get; } = TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_ACHIEVEMENT_DESC";
        public override Sprite Sprite { get; } = Modules.Assets.LoadAsset<Sprite>("GurrenLagannIcon");

        public override Func<string> GetHowToUnlock { get; } = (() => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new object[]
                            {
                                Language.GetString(TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_ACHIEVEMENT_NAME"),
                                Language.GetString(TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_ACHIEVEMENT_DESC")
                            }));
        public override Func<string> GetUnlocked { get; } = (() => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
                            {
                                Language.GetString(TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_ACHIEVEMENT_NAME"),
                                Language.GetString(TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_ACHIEVEMENT_DESC")
                            }));

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