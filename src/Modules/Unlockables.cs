

using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Achievements;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TTGL_Survivor.Modules
{
    internal static class Unlockables
    {
        public static UnlockableDef lagannCombineUnlockable;

        public static void AddUnlockables()
        {
            lagannCombineUnlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            lagannCombineUnlockable.nameToken = TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_UNLOCKABLE_ID";
            lagannCombineUnlockable.cachedName = TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_UNLOCKABLE_ID";
            lagannCombineUnlockable.getHowToUnlockString = (() => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new object[]
                            {
                                Language.GetString("ACHIEVEMENT_" +TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_ACHIEVEMENT_ID_NAME"),
                                Language.GetString("ACHIEVEMENT_" +TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_ACHIEVEMENT_ID_DESCRIPTION")
                            }));
            lagannCombineUnlockable.getUnlockedString = (() => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
                            {
                                Language.GetString("ACHIEVEMENT_" +TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_ACHIEVEMENT_ID_NAME"),
                                Language.GetString("ACHIEVEMENT_" +TTGL_SurvivorPlugin.developerPrefix + "_LAGANNCOMBINE_ACHIEVEMENT_ID_DESCRIPTION")
                            }));
            lagannCombineUnlockable.sortScore = 200;
            lagannCombineUnlockable.achievementIcon = Modules.TTGLAssets.LoadAsset<Sprite>("GurrenLagannIcon");


            TTGL_SurvivorPlugin.unlockableDefs.Add(lagannCombineUnlockable);
            
        }
    }
}