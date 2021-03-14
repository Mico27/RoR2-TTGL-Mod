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

            LanguageAPI.Add(prefix + "_LAGANN_BODY_SECONDARY_EXPLOSION_NAME", "Yoko's Rifle - Explosive rounds");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SECONDARY_EXPLOSION_DESCRIPTION", $"Yoko fires an explosive round dealing <style=cIsDamage>{100f * SkillStates.ExplosiveRifle.damageCoefficient}% damage</style> in a 20 unit radius.");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_SECONDARY_SCEPTER_RIFLE_NAME", "Yoko's Rifle - Ancient Scepter Version");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SECONDARY_SCEPTER_RIFLE_DESCRIPTION", $"Yoko fires an explosive round dealing <style=cIsDamage>{100f * SkillStates.ScepterRifle.damageCoefficient}% damage</style> in a 20 unit radius. Critical hits ricochet to other enemies.");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_UTILITY_SPIRALBURST_NAME", "Spiral Burst");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_UTILITY_SPIRALBURST_DESCRIPTION", $"Lagann launches itself upward or in any inputed direction, dealing <style=cIsDamage>{100f * SkillStates.SpiralBurst.damageCoefficient}% damage</style> on its path. <style=cIsUtility>Lagann cannot be hit during this.</style>");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_UTILITY_TOGGLECANOPY_NAME", "Toggle Canopy");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_UTILITY_TOGGLECANOPY_DESCRIPTION", $"Toggle on/off Lagann's canopy. When the canopy is on, you gain <style=cIsUtility>150 Armor</style> and <style=cIsUtility>is immune to movement impairing effects</style>. However, in this state, Yoko cannot use her rifle.");


            LanguageAPI.Add(prefix + "_LAGANN_BODY_SPECIAL_IMPACT_NAME", "Lagann Impact");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SPECIAL_IMPACT_DESCRIPTION", $"Lagann goes into drill mode and shoots itself in a line dealing <style=cIsDamage>{100f * SkillStates.LagannImpact.c_DamageCoefficient}% damage</style> in its path. Lagann can bounce off walls up to {SkillStates.AimLagannImpact.c_MaxRebound} times.");

        }
    }
}