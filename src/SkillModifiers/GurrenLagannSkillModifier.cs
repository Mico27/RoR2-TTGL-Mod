using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using SkillsPlusPlus.Modifiers;
using System;
using System.Collections.Generic;
using System.Text;
using TTGL_Survivor.Modules;
using TTGL_Survivor.Modules.Components;
using TTGL_Survivor.Modules.Survivors;
using TTGL_Survivor.SkillStates;
using TTGL_Survivor.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillModifiers
{
    [SkillLevelModifier("GurrenLagannSpiralingCombo", typeof(GurrenLagannSpiralingCombo))]
    class GurrenLagannSpiralingComboSkillModifier : SimpleSkillModifier<GurrenLagannSpiralingCombo>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            var capedLevel = Math.Min(25, level);
            GurrenLagannBaseCombo.baseDamageCoeficient = AdditiveScaling(1.0f, 0.15f, capedLevel);// increase damage by 15% every level (linear)
            GurrenLagannBaseCombo.pullRadius = AdditiveScaling(20.0f, 1f, capedLevel); // increase pull radius by 5% every level (linear)
            GurrenLagannBaseCombo.pullForce = AdditiveScaling(80.0f, 4f, capedLevel); // increase pull force by 5% every level (linear)
            GurrenLagannBaseCombo.allBypassArmor = (capedLevel >= 4); // all spiraling combo moves bypass armor at level 4
        }
    }

    [SkillLevelModifier("GurrenLagannThrowingShades", typeof(GurrenLagannThrowingShades))]
    class GurrenLagannThrowingShadesSkillModifier : SimpleSkillModifier<GurrenLagannThrowingShades>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            var capedLevel = Math.Min(25, level);
            var shadesWhirlPrefab = Projectiles.shadesWhirlPrefab;
            if (shadesWhirlPrefab)
            {
                var projectileDotZone = shadesWhirlPrefab.GetComponent<ProjectileDotZone>();
                if (projectileDotZone)
                {
                    projectileDotZone.damageCoefficient = AdditiveScaling(0.10f, 0.02f, capedLevel); // increase Dot damage by 20% every level (linear)
                }
                ProjectileController projectileController = shadesWhirlPrefab.GetComponent<ProjectileController>();
                if (projectileController)
                {
                    projectileController.ghostPrefab.transform.localScale = Vector3.one * AdditiveScaling(4.0f, 0.6f, capedLevel);// increase shades size by 15% every level (linear)
                }
                var hitbox = shadesWhirlPrefab.GetComponentInChildren<HitBox>();
                if (hitbox)
                {
                    hitbox.transform.localScale = new Vector3(8.0f, 3.0f, 8.0f) * AdditiveScaling(1.0f, 0.15f, capedLevel);
                }
            }
        }
    }

    [SkillLevelModifier("GurrenLagannTornadoKick", typeof(GurrenLagannTornadoKick))]
    class GurrenLagannTornadoKickSkillModifier : SimpleSkillModifier<GurrenLagannTornadoKick>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            var capedLevel = Math.Min(25, level);
            GurrenLagannTornadoKick.damageCoefficient = AdditiveScaling(1.5f, 0.3f, capedLevel);// increase damage by 20% every level (linear)
            GurrenLagannTornadoKick.jumpVelocity = AdditiveScaling(7.0f, 2.1f, capedLevel); // increase jump velocity by 30% every level (linear)
            GurrenLagannTornadoKick.canControlDirection = (capedLevel >= 4); // Can control the direction mid move at level 4
        }        
    }

    [SkillLevelModifier("GurrenLagannGigaDrillMaximum", typeof(GurrenLagannGigaDrillMaximum))]
    class GurrenLagannGigaDrillMaximumSkillModifier : SimpleSkillModifier<GurrenLagannGigaDrillMaximum>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            var capedLevel = Math.Min(25, level);
            GurrenLagannGigaDrillMaximum.energyCost = MultScaling(50f, -0.10f, capedLevel);// decrease spiral energy cost by 10% every level (exponential)
            GurrenLagannGigaDrillMaximum.c_DamageCoefficient = AdditiveScaling(7.5f, 0.75f, capedLevel);// increase damage by 10% every level (linear)
            GurrenLagannGigaDrillMaximum.canBypassArmor = (capedLevel >= 4); // Can bypass armor at level 4
        }
    }

    [SkillLevelModifier("GurrenLagannSplit", typeof(GurrenLagannSplit))]
    class GurrenLagannSplitSkillModifier : SimpleSkillModifier<GurrenLagannSplit>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            var capedLevel = Math.Min(25, level);
            GurrenLagannSplit.carryOverEnergyPercent = Math.Max(0f, MultScaling(1f, -0.1f, capedLevel));// increase spiral energy that can be carried over by 10% every level (exponential)
        }
    }
}
