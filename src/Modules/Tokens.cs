using R2API;
using System;

namespace TTGL_Survivor.Modules
{
    internal static class Tokens
    {
        internal static void AddTokens()
        {
            string prefix = TTGL_SurvivorPlugin.developerPrefix;

            string lagannDesc = "Lagann<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            lagannDesc = lagannDesc + "< ! > Tip 1." + Environment.NewLine + Environment.NewLine;
            lagannDesc = lagannDesc + "< ! > Tip 2." + Environment.NewLine + Environment.NewLine;
            lagannDesc = lagannDesc + "< ! > Tip 3." + Environment.NewLine + Environment.NewLine;
            lagannDesc = lagannDesc + "< ! > Tip 4." + Environment.NewLine + Environment.NewLine;
            
            LanguageAPI.Add(prefix + "_LAGANN_BODY_NAME", "Lagann");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_DESCRIPTION", lagannDesc);
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SUBTITLE", "Gunman");


            LanguageAPI.Add(prefix + "_LAGANN_BODY_DEFAULT_SKIN_NAME", "Default");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_PASSIVE_NAME", "Spiral Power");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_PASSIVE_DESCRIPTION", "When in a pinch, spiral power increases, also increasing Lagann's <style=cIsDamage>damage</style>, <style=cIsUtility>movement speed</style> and <style=cIsHealing>health regen</style>");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_PRIMARY_DRILL_NAME", "Drill Rush");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_PRIMARY_DRILL_DESCRIPTION", $"Lagann punches with his drills rapidly for <style=cIsDamage>{100f * SkillStates.LagannDrillRush.c_DamageCoefficient}% damage</style>. <style=cIsUtility>Ignores armor.</style>");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_SECONDARY_RIFLE_NAME", "Yoko's Rifle");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SECONDARY_RIFLE_DESCRIPTION", $"Yoko fires her rifle for <style=cIsDamage>{100f * SkillStates.YokoShootRifle.damageCoefficient}% damage</style>. Critical hits ricochet to other enemies.");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_SECONDARY_EXPLOSION_NAME", "Yoko's Rifle - Explosive rounds");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SECONDARY_EXPLOSION_DESCRIPTION", $"Yoko fires an explosive round dealing <style=cIsDamage>{100f * SkillStates.YokoExplosiveRifle.damageCoefficient}% damage</style> in a 20 unit radius.");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_SECONDARY_SCEPTER_RIFLE_NAME", "Yoko's Rifle - Ancient Scepter Version");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SECONDARY_SCEPTER_RIFLE_DESCRIPTION", $"Yoko fires an explosive round dealing <style=cIsDamage>{100f * SkillStates.YokoScepterRifle.damageCoefficient}% damage</style> in a 20 unit radius. Critical hits ricochet to other enemies.");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_UTILITY_SPIRALBURST_NAME", "Spiral Burst");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_UTILITY_SPIRALBURST_DESCRIPTION", $"Lagann launches itself upward or in any inputed direction, dealing <style=cIsDamage>{100f * SkillStates.LagannSpiralBurst.damageCoefficient}% damage</style> on its path. <style=cIsUtility>Lagann cannot be hit during this.</style>");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_UTILITY_TOGGLECANOPY_NAME", "Toggle Canopy");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_UTILITY_TOGGLECANOPY_DESCRIPTION", $"Toggle on/off Lagann's canopy. When the canopy is on, you gain <style=cIsUtility>150 Armor</style> and <style=cIsUtility>is immune to movement impairing effects</style>. However, in this state, Yoko cannot use her rifle.");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_UTILITY_DISABLERIFLE_NAME", "Blocked Rifle");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_UTILITY_DISABLERIFLE_DESCRIPTION", $"Yoko cannot use her rifle due to the canopy.");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_SPECIAL_IMPACT_NAME", "Lagann Impact");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SPECIAL_IMPACT_DESCRIPTION", $"Lagann goes into drill mode and shoots itself in a line dealing <style=cIsDamage>{100f * SkillStates.LagannImpact.c_DamageCoefficient}% damage</style> in its path. Lagann can bounce off walls up to {SkillStates.AimLagannImpact.c_MaxRebound} times.");

            string gurrenLagannDesc = "Lagann<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            gurrenLagannDesc = gurrenLagannDesc + "< ! > Tip 1." + Environment.NewLine + Environment.NewLine;
            gurrenLagannDesc = gurrenLagannDesc + "< ! > Tip 2." + Environment.NewLine + Environment.NewLine;
            gurrenLagannDesc = gurrenLagannDesc + "< ! > Tip 3." + Environment.NewLine + Environment.NewLine;
            gurrenLagannDesc = gurrenLagannDesc + "< ! > Tip 4." + Environment.NewLine + Environment.NewLine;

            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_NAME", "Gurren Lagann");
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_DESCRIPTION", gurrenLagannDesc);
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_SUBTITLE", "Gunman");

            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_DEFAULT_SKIN_NAME", "Default");

            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_PASSIVE_NAME", "Spiral Power");
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_PASSIVE_DESCRIPTION", "When in a pinch, spiral power increases, also increasing Lagann's <style=cIsDamage>damage</style>, <style=cIsUtility>movement speed</style> and <style=cIsHealing>health regen</style>");

            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_SPIRALINGCOMBO_NAME", "Spiraling Combo");
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_SPIRALINGCOMBO_DESCRIPTION", $"Gurren Lagann deals a series of melee attacks for around <style=cIsDamage>{230f}-{340}% damage</style>. <style=cIsUtility>Drills ignores armor.</style>");

            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_THROWINGSHADES_NAME", "Throwin' Shades");
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_THROWINGSHADES_DESCRIPTION", $"Gurren Lagann throws his shades like a boomerang, dealing <style=cIsDamage>{100f * SkillStates.GurrenLagannThrowingShades.damageCoefficient}% damage</style>");

            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_TORNADOKICK_NAME", "Tornado Kick");
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_TORNADOKICK_DESCRIPTION", $"Gurren Lagann does a tornado kick, dealing <style=cIsDamage>3 x {100f * SkillStates.GurrenLagannTornadoKick.damageCoefficient}% damage</style> on its path. <style=cIsUtility>Cannot be hit during this.</style>");
            
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_GIGADRILLMAXIMUM_NAME", "Giga Drill Maximum");
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_GIGADRILLMAXIMUM_DESCRIPTION", $"Gurren Lagann expands numerous drills from its body, dealing <style=cIsDamage>{100f * SkillStates.GurrenLagannGigaDrillMaximum.c_DamageCoefficient}% damage</style> in a large radius,<style=cIsUtility> stunning enemies </style>");

        }
    }
}