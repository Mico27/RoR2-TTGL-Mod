using R2API;
using System;

namespace TTGL_Survivor.Modules
{
    internal static class Tokens
    {
        internal static void AddTokens()
        {
            string prefix = TTGL_SurvivorPlugin.developerPrefix;

            string lagannDesc = "Lagann (羅顔) is Simon's personal Gunman and one of the components of Gurren Lagann (Gunmen) along with Gurren.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            lagannDesc = lagannDesc + "< ! > Lagann can harness the power of the spiral which its amount is displayed in a gauge." + Environment.NewLine +
                "The more spiral power Lagann has, the more dammage, health regen and movement speed is gained." + Environment.NewLine + 
                "The gauge can be filled 3 times before reaching maximum power, which trigger spiral power overflow giving an armor buff." + Environment.NewLine +
                "Spiral power rate goes up the more damage Lagann receives and the more enemies are around." + Environment.NewLine +
                "However, to keep the rate uptime, Lagann must deal or receive damage in the last 6 seconds." + Environment.NewLine + Environment.NewLine;
            lagannDesc = lagannDesc + "< ! > Lagann's can expand drills from its hands which can deal strong armor piercing melee attacks." + Environment.NewLine + Environment.NewLine;
            lagannDesc = lagannDesc + "< ! > Yoko can use her rifle to attack enemies at a distance." + Environment.NewLine +
                "Using the regular rounds can rebound to an enemy on crit," + Environment.NewLine + 
                "while using the explosive rounds deal more damage and will explode dealing AOE damage." + Environment.NewLine + Environment.NewLine;
            lagannDesc = lagannDesc + "< ! > You can use Lagann's spiral burst for extra mobility and iFrames or use its canopy for extra armor and CC immunity." + Environment.NewLine + Environment.NewLine;
            lagannDesc = lagannDesc + "< ! > Lagann can turn into Drill mode to deal massive damage in its path and can be used for massive mobility." + Environment.NewLine + Environment.NewLine;
            lagannDesc = lagannDesc + "< ! > If you start a run as Lagann, you may find and empty Gurren laying around somewhere on the map." + Environment.NewLine + 
                "Upon activating Gurren, Kamina will jump in and fight alongside you." + Environment.NewLine +
                "At this point, if you have at least 1 gauge of spiral energy, Lagann can combine with Gurren to turn into Gurren Lagann." + Environment.NewLine +
                "When you combine into Gurren Lagann, you will be restored to full health." + Environment.NewLine +
                "If Gurren dies at any point, Kamina will return to Lagann and it will be possible to find another Gurren in a later map." + Environment.NewLine + Environment.NewLine;

            LanguageAPI.Add(prefix + "_LAGANN_BODY_NAME", "Lagann");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_DESCRIPTION", lagannDesc);
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SUBTITLE", "Gunman");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_OUTRO_FLAVOR", "Later, Buddy");


            LanguageAPI.Add(prefix + "_LAGANN_BODY_DEFAULT_SKIN_NAME", "Default");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_WOOPS_SKIN_NAME", "Woops");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_PASSIVE_NAME", "Spiral Power");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_PASSIVE_DESCRIPTION", "When in a pinch, spiral power increases, also increasing Lagann's <style=cIsDamage>damage</style>, <style=cIsUtility>movement speed</style> and <style=cIsHealing>health regen</style>");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_PRIMARY_DRILL_NAME", "Drill Rush");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_PRIMARY_DRILL_DESCRIPTION", $"Lagann punches with his drills rapidly for <style=cIsDamage>{100f * SkillStates.LagannDrillRush.damageCoefficient}% damage</style>. <style=cIsUtility>Ignores armor.</style>");
            
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SECONDARY_RIFLE_NAME", "Yoko's Rifle");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_PRIMARY_DRILL_DESCRIPTION", $"Lagann punches with his drills rapidly for <style=cIsDamage>{100f * SkillStates.LagannDrillRush.damageCoefficient}% damage</style>. <style=cIsUtility>Ignores armor.</style>");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SECONDARY_RIFLE_DESCRIPTION", $"Yoko fires her rifle for <style=cIsDamage>{100f * SkillStates.YokoShootRifle.damageCoefficient}% damage</style>. Critical hits ricochet to other enemies up to { SkillStates.YokoShootRifle.maxRicochetCount } times.");

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
            LanguageAPI.Add(prefix + "_LAGANN_BODY_SPECIAL_IMPACT_DESCRIPTION", $"Lagann goes into drill mode and shoots itself in a line dealing <style=cIsDamage>{100f * SkillStates.LagannImpact.damageCoefficient}% damage</style> in its path. Lagann can bounce off walls up to {SkillStates.AimLagannImpact.maxRebound - 1} times.");

            LanguageAPI.Add(prefix + "_LAGANN_BODY_COMBINE_NAME", "Brotherly Combining! Gurren Lagann!");
            LanguageAPI.Add(prefix + "_LAGANN_BODY_COMBINE_DESCRIPTION", $"Combine into Gurren Lagann! Requires <style=cIsUtility>at least 1 spiral gauge</style> and <style=cIsUtility>Gurren must be in the party</style>.");
            
            string gurrenDesc = "Gurren (グレン), a modified Gunzar, is a Gunmen captured and piloted by Kamina.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            gurrenDesc = gurrenDesc + "< ! > Kamina fueled by fighting spirit gains extra damage for each allies close to him." + Environment.NewLine +
                "Repairing as much drones as possible or having friends to play with is a good way to take advantage of it." + Environment.NewLine +
                "Also, if any other allies close to Gurren are capable of spiral power, their spiral power rate is doubled." + Environment.NewLine + Environment.NewLine;
            gurrenDesc = gurrenDesc + "< ! > Gurren can use its shades as a melee weapon." + Environment.NewLine + 
                "Due to its massive size, enemies near can get pulled from the slipstream." + Environment.NewLine + Environment.NewLine;
            gurrenDesc = gurrenDesc + "< ! > Gurren can also shoot from its wrists, blasting streams of bullets to deal damage to enemies at long range." + Environment.NewLine + Environment.NewLine;
            gurrenDesc = gurrenDesc + "< ! > If you want to close a gap to get into melee range or extend the gap to attack at range" + Environment.NewLine +
                "or if you want to simply dodge a big attack or retreat, simply roll." + Environment.NewLine + Environment.NewLine;
            gurrenDesc = gurrenDesc + "< ! > Use Gurren's ultimate to throw a huge boulder at a group of enemies, stunning them in the process." + Environment.NewLine +
                "Good to use if you are starting to get overwhelmed by too many enemies. Can only be used while on the ground." + Environment.NewLine + Environment.NewLine;

            LanguageAPI.Add(prefix + "_GURREN_BODY_NAME", "Gurren");
            LanguageAPI.Add(prefix + "_GURREN_BODY_DESCRIPTION", gurrenDesc);
            LanguageAPI.Add(prefix + "_GURREN_BODY_SUBTITLE", "Gunman");
            LanguageAPI.Add(prefix + "_GURREN_BODY_OUTRO_FLAVOR", "Later, Buddy");

            LanguageAPI.Add(prefix + "_GURREN_BODY_DEFAULT_SKIN_NAME", "Default");

            LanguageAPI.Add(prefix + "_GURREN_BODY_PASSIVE_NAME", "Kamina's Spirit");
            LanguageAPI.Add(prefix + "_GURREN_BODY_PASSIVE_DESCRIPTION", "For each allies near Gurren, gain an additional <style=cIsDamage>25% damage</style>. Any allies capable of spiral power near Gurren gain <style=cIsUtility>double their spiral power rate</style>.");

            LanguageAPI.Add(prefix + "_GURREN_BODY_TRIPLESLASH_NAME", "Triple Slash");
            LanguageAPI.Add(prefix + "_GURREN_BODY_TRIPLESLASH_DESCRIPTION", $"Gurren deals 3 slashing attacks for <style=cIsDamage>3x {SkillStates.GurrenTripleSlash.c_DamageCoefficient}% damage</style>.");

            LanguageAPI.Add(prefix + "_GURREN_BODY_DRILLBLASTER_NAME", "Drill Blaster");
            LanguageAPI.Add(prefix + "_GURREN_BODY_DRILLBLASTER_DESCRIPTION", $"Gurren shoots piercing bullets from his wrists, dealing <style=cIsDamage>{100f * SkillStates.GurrenDrillBlaster.damageCoefficient}% damage</style> per bullets");

            LanguageAPI.Add(prefix + "_GURREN_BODY_ROLL_NAME", "Tactical Roll");
            LanguageAPI.Add(prefix + "_GURREN_BODY_ROLL_DESCRIPTION", $"Gurren does a diving roll. <style=cIsUtility>Cannot be hit during this.</style>");

            LanguageAPI.Add(prefix + "_GURREN_BODY_BOULDERTHROW_NAME", "Boulder Throw");
            LanguageAPI.Add(prefix + "_GURREN_BODY_BOULDERTHROW_DESCRIPTION", $"Gurren digs up a huge boulder from the ground and throws it, dealing <style=cIsDamage>{100f * SkillStates.GurrenThrowBoulder.damageCoefficient}% damage</style>, <style=cIsUtility> stunning enemies.</style>");

            LanguageAPI.Add(prefix + "_GURRENFOUND_ACHIEVEMENT_NAME", "I Said I'm Gonna Pilot That Thing!");
            LanguageAPI.Add(prefix + "_GURRENFOUND_UNLOCKABLE_NAME", "I Said I'm Gonna Pilot That Thing!");
            LanguageAPI.Add(prefix + "_GURRENFOUND_ACHIEVEMENT_DESC", "Find and pilot Gurren.");


            string gurrenLagannDesc = "Gurren Lagann (グレンラガン) is a combined Gunmen made out of Lagann and Gurren. The head, Lagann, is piloted by Simon, while the body, Gurren, is piloted by Kamina <color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            gurrenLagannDesc = gurrenLagannDesc + "< ! > Just like Lagann, Gurren Lagann can harness the power of the spiral. However, its spiral power rate is doubled." + Environment.NewLine + Environment.NewLine;
            gurrenLagannDesc = gurrenLagannDesc + "< ! > Gurren Lagann's spiraling combo can deal a varieties of melee moves." + Environment.NewLine +
                "Like Gurren, due to its massive size, enemies near can get pulled from the slipstream." + Environment.NewLine + Environment.NewLine;
            gurrenLagannDesc = gurrenLagannDesc + "< ! > For enemies that are at range, Gurren Langann can throw its shades like a boomerang which also does ticking damage on its path." + Environment.NewLine +
                "If you throw its shades at a boss, the boss will be contricted allowing Gurren Lagann to use its final move GIGA DRILL BREAK dealing MASSIVE damage." + Environment.NewLine +
                "However, upon using the final move, Gurren Lagann will split back to Lagann and Gurren." + Environment.NewLine + Environment.NewLine;
            gurrenLagannDesc = gurrenLagannDesc + "< ! > You can use Gurren Lagann's tornado kick the same way as you'd use Lagann's spiral burst." + Environment.NewLine +
                "But can deal 3x the amount of hits on the same targets on its path." + Environment.NewLine + Environment.NewLine;
            gurrenLagannDesc = gurrenLagannDesc + "< ! > Gurren Lagann can expend drills from all over its body to deal damage in a large radius at the cost of spiral power." + Environment.NewLine +
                "Can come handy when needing to clear up the cannon fodders." + Environment.NewLine + Environment.NewLine;
            gurrenLagannDesc = gurrenLagannDesc + "< ! > If you find yourself in a bind or need a more zoomed view or the mobility of Lagann," + Environment.NewLine +
                "you can do an emergency exit to slit back into Lagann and Gurren." + Environment.NewLine + 
                "However it comes at the cost of losing your remaining spiral power." + Environment.NewLine +
                "But just like when you combine, you will regain your health when spliting up." + Environment.NewLine + Environment.NewLine;

            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_NAME", "Gurren Lagann");
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_DESCRIPTION", gurrenLagannDesc);
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_SUBTITLE", "Gunman");
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_OUTRO_FLAVOR", "Later, Buddy");

            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_DEFAULT_SKIN_NAME", "Default");

            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_PASSIVE_NAME", "Spiral Power");
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_PASSIVE_DESCRIPTION", "When in a pinch, spiral power increases, also increasing Lagann's <style=cIsDamage>damage</style>, <style=cIsUtility>movement speed</style> and <style=cIsHealing>health regen</style>");

            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_SPIRALINGCOMBO_NAME", "Spiraling Combo");
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_SPIRALINGCOMBO_DESCRIPTION", $"Gurren Lagann deals a series of melee attacks for around <style=cIsDamage>{230f}-{540}% damage</style>. <style=cIsUtility>Drills ignores armor.</style>");

            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_THROWINGSHADES_NAME", "Throwin' Shades");
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_THROWINGSHADES_DESCRIPTION", $"Gurren Lagann throws his shades like a boomerang, dealing <style=cIsDamage>{100f * SkillStates.GurrenLagannThrowingShades.damageCoefficient}% damage</style>");

            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_TORNADOKICK_NAME", "Tornado Kick");
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_TORNADOKICK_DESCRIPTION", $"Gurren Lagann does a tornado kick, dealing <style=cIsDamage>3 x {100f * SkillStates.GurrenLagannTornadoKick.damageCoefficient}% damage</style> on its path. <style=cIsUtility>Cannot be hit during this.</style>");
            
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_GIGADRILLMAXIMUM_NAME", "Giga Drill Maximum");
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_GIGADRILLMAXIMUM_DESCRIPTION", $"Gurren Lagann expands numerous drills from its body, dealing <style=cIsDamage>{100f * SkillStates.GurrenLagannGigaDrillMaximum.c_DamageCoefficient}% damage</style> in a large radius,<style=cIsUtility> stunning enemies.</style> Costs <style=cIsUtility>half a spiral gauge.</style>");

            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_SPLIT_NAME", "Emergency Extraction");
            LanguageAPI.Add(prefix + "_GURRENLAGANN_BODY_SPLIT_DESCRIPTION", $"Splits back into Gurren and Lagann, taking back control of Lagann. Doing so depletes all your spiral energy.");

            LanguageAPI.Add(prefix + "_LAGANNCOMBINE_ACHIEVEMENT_NAME", "To Hell with Your Combi!");
            LanguageAPI.Add(prefix + "_LAGANNCOMBINE_UNLOCKABLE_NAME", "To Hell with Your Combi!");
            LanguageAPI.Add(prefix + "_LAGANNCOMBINE_ACHIEVEMENT_DESC", "Combine into Gurren Lagann.");

            LanguageAPI.Add(prefix + "_GURREN_INTERACTABLE_NAME", "Gurren");
            LanguageAPI.Add(prefix + "_GURREN_INTERACTABLE_CONTEXT", "Pilot the gunman");
            LanguageAPI.Add(prefix + "_GURREN_SPAWNER_USE_MESSAGE", "<style=cShrine>{0} has mounted Gurren.</color>");
            LanguageAPI.Add(prefix + "_GURREN_SPAWNER_USE_MESSAGE_2P", "<style=cShrine>You have mounted Gurren.</color>");

            LanguageAPI.Add(prefix + "_MAXSPIRALPOWER_BUFF_NAME", "Spiral power overflow");
            LanguageAPI.Add(prefix + "_MAXSPIRALPOWER_BUFF_DESCRIPTION", "+300 armor and spiral energy rate is hugely increased");
            LanguageAPI.Add(prefix + "_MAXSPIRALPOWER_DEBUFF_NAME", "Spiral power fatigue");
            LanguageAPI.Add(prefix + "_MAXSPIRALPOWER_DEBUFF_DESCRIPTION", "Spiral energy rate is decreased");
            LanguageAPI.Add(prefix + "_CANOPY_BUFF_NAME", "Canopy activated");
            LanguageAPI.Add(prefix + "_CANOPY_BUFF_DESCRIPTION", "+150 armor and is immune to movement impairing effects");

            LanguageAPI.Add("LAGANNDRILLRUSH_UPGRADE_DESCRIPTION",
                "Increase damage by 10% every level" + Environment.NewLine +
                "Increase drill size by 20% every level (Max 8x original size)" + Environment.NewLine +
                "Can gain extra spiral power on hit at level 4");

            LanguageAPI.Add("YOKOSHOOTRIFLE_UPGRADE_DESCRIPTION",
                "Increase ammo by 1 every level" + Environment.NewLine +
                "Increase max ricochet count by 3 every level" + Environment.NewLine +
                "Ricochet can hit back previously hit enemies at level 4");

            LanguageAPI.Add("YOKOEXPLOSIVERIFLE_UPGRADE_DESCRIPTION",
                "Increase ammo by 1 every level" + Environment.NewLine +
                "Increase explosion size by 10% every level (Max 3x original size)" + Environment.NewLine +
                "Explosion spawns a cluster of smaller explosions at level 4");

            LanguageAPI.Add("YOKOSCEPTERRIFLE_UPGRADE_DESCRIPTION",
               "Increase ammo by 2 every level" + Environment.NewLine +
               "Increase max ricochet count by 2 every level" + Environment.NewLine +
               "Increase explosion size by 15% every level (Max 3x original size)" + Environment.NewLine +
               "Ricochet can hit back previously hit enemies at level 4");

            LanguageAPI.Add("LAGANNSPIRALBURST_UPGRADE_DESCRIPTION",
                "Increase damage by 30% every level" + Environment.NewLine +
                "Increase jump velocity by 10% every level" + Environment.NewLine +
                "Get a 5 second armor buff on use at level 4");

            LanguageAPI.Add("LAGANNTOGGLECANOPY_UPGRADE_DESCRIPTION",
                "Increase armor from buff by 20% every level" + Environment.NewLine +
                "Restock yoko's rifle on use at level 4");

            LanguageAPI.Add("LAGANNIMPACT_UPGRADE_DESCRIPTION",
                "Increase maximum number of rebound by 1 every level" + Environment.NewLine +
                "Increase shooting distance by 25% every level" + Environment.NewLine +
                "Increase damage by 10% every level");

            LanguageAPI.Add("LAGANNCOMBINE_UPGRADE_DESCRIPTION",
                "Decrease energy requirement by 10% every level");


            LanguageAPI.Add("GURRENLAGANNSPIRALINGCOMBO_UPGRADE_DESCRIPTION",
                "Increase damage by 15% every level" + Environment.NewLine +
                "Increase pull radius by 10% every level" + Environment.NewLine +
                "Increase pull force by 10% every level" + Environment.NewLine +
                "All spiraling combo moves bypass armor at level 4");

            LanguageAPI.Add("GURRENLAGANNTHROWINGSHADES_UPGRADE_DESCRIPTION",
                "Increase Dot damage by 20% every level" + Environment.NewLine +
                "Increase shades size by 15% every level (Max 4x original size)");

            LanguageAPI.Add("GURRENLAGANNTORNADOKICK_UPGRADE_DESCRIPTION",
                "Increase damage by 15% every level" + Environment.NewLine +
                "Increase jump velocity by 25% every level" + Environment.NewLine +
                "Can control the direction mid move at level 4");

            LanguageAPI.Add("GURRENLAGANNGIGADRILLMAXIMUM_UPGRADE_DESCRIPTION",
                "Decrease spiral energy cost by 10% every level" + Environment.NewLine +
                "Increase damage by 10% every level" + Environment.NewLine +
                "Can bypass armor at level 4");

            LanguageAPI.Add("GURRENLAGANNSPLIT_UPGRADE_DESCRIPTION",
                "Increase spiral energy that can be carried over by 10% every level");

        }
    }
}