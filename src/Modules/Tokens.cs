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
            
            LanguageAPI.Add(prefix + "_LAGANN_BODY_NAME", "Lagann");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_DESCRIPTION", desc);
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SUBTITLE", "Gunman");


            LanguageAPI.Add(prefix + "_LAGANN_BODY_DEFAULT_SKIN_NAME", "Default");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_PASSIVE_NAME", "Spiral Power");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_PASSIVE_DESCRIPTION", "When in a pinch, spiral power increases, also increasing Lagann's <style=cIsDamage>damage</style>, <style=cIsUtility>movement speed</style> and <style=cIsHealing>health regen</style>");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_PRIMARY_DRILL_NAME", "Drill Rush");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_PRIMARY_DRILL_DESCRIPTION", $"Lagann punches with his drills rapidly for <style=cIsDamage>{100f * SkillStates.DrillRush.c_DamageCoefficient}% damage</style>. <style=cIsUtility>Ignores armor.</style>");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_SECONDARY_RIFLE_NAME", "Yoko's Rifle");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SECONDARY_RIFLE_DESCRIPTION", $"Yoko fires her rifle for <style=cIsDamage>{100f * SkillStates.ShootRifle.damageCoefficient}% damage</style>. Critical hits ricochet to other enemies.");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_UTILITY_SPIRALBURST_NAME", "Spiral Burst");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_UTILITY_SPIRALBURST_DESCRIPTION", $"Lagann launches itself upward or in any inputed direction, dealing <style=cIsDamage>{100f * SkillStates.SpiralBurst.damageCoefficient}% damage</style> on its path. <style=cIsUtility>Lagann cannot be hit during this.</style>");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_SPECIAL_IMPACT_NAME", "Lagann Impact");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SPECIAL_IMPACT_DESCRIPTION", $"Lagann goes into drill mode and shoots itself in a line dealing <style=cIsDamage>{100f * SkillStates.LagannImpact.damageCoefficient}% damage</style> in its path. Lagann can bounce off walls up to {SkillStates.LagannImpact.bounceCount} times.");

        }
    }
}