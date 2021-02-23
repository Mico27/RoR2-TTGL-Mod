using R2API;
using System;

namespace TTGL_Survivor.Modules
{
    internal static class Tokens
    {
        internal static void AddTokens()
        {
            string prefix = TTGL_SurvivorPlugin.developerPrefix;

            string desc = "Lagann<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Tip 1." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Tip 2." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Tip 3." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Tip 4." + Environment.NewLine + Environment.NewLine;

            string outro = "..and so he left, bottom text.";

            LanguageAPI.Add(prefix + "_LAGANN_BODY_NAME", "Lagann");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_DESCRIPTION", desc);
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SUBTITLE", "The Chosen One");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_LORE", "sample lore");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_OUTRO_FLAVOR", outro);


            LanguageAPI.Add(prefix + "_LAGANN_BODY_DEFAULT_SKIN_NAME", "Default");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_MASTERY_SKIN_NAME", "Alternate");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_PASSIVE_NAME", "Spiral Power");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_PASSIVE_DESCRIPTION", "Sample text.");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_PRIMARY_DRILL_NAME", "Drill Rush");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_PRIMARY_DRILL_DESCRIPTION", Helpers.agilePrefix + $"Punch rapidly for <style=cIsDamage>{100f * 2.4f}% damage</style>. <style=cIsUtility>Ignores armor.</style>");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_SECONDARY_RIFLE_NAME", "Yoko's Rifle");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SECONDARY_RIFLE_DESCRIPTION", Helpers.agilePrefix + $"Fire your handgun for <style=cIsDamage>{100f * SkillStates.ShootRifle.damageCoefficient}% damage</style>.");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_UTILITY_SPIRALBURST_NAME", "Spiral Burst");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_UTILITY_SPIRALBURST_DESCRIPTION", "Roll a short distance, gaining <style=cIsUtility>300 armor</style>. <style=cIsUtility>You cannot be hit during the roll.</style>");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_SPECIAL_IMPACT_NAME", "Lagann Impact");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SPECIAL_IMPACT_DESCRIPTION", $"Throw a bomb for <style=cIsDamage>{100f * SkillStates.LagannImpact.damageCoefficient}% damage</style>.");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_UNLOCKABLE_ACHIEVEMENT_NAME", "Prelude");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_UNLOCKABLE_ACHIEVEMENT_DESC", "Enter Titanic Plains.");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_UNLOCKABLE_UNLOCKABLE_NAME", "Prelude");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_MASTERYUNLOCKABLE_ACHIEVEMENT_NAME", "Lagann: Mastery");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_MASTERYUNLOCKABLE_ACHIEVEMENT_DESC", "As Lagann, beat the game or obliterate on Monsoon.");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_MASTERYUNLOCKABLE_UNLOCKABLE_NAME", "Lagann: Mastery");
        }
    }
}