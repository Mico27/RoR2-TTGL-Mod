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
            GurrenLagannBaseCombo.baseDamageCoeficient = MultScaling(1.0f, 0.15f, level);// increase damage by 15% every level
            GurrenLagannBaseCombo.pullRadius = MultScaling(20.0f, 0.10f, level); // increase pull radius by 10% every level
            GurrenLagannBaseCombo.pullForce = MultScaling(80.0f, 0.10f, level); // increase pull force by 10% every level
            GurrenLagannBaseCombo.allBypassArmor = (level >= 4); // all spiraling combo moves bypass armor at level 4
        }
    }

    [SkillLevelModifier("GurrenLagannThrowingShades", typeof(GurrenLagannThrowingShades))]
    class GurrenLagannThrowingShadesSkillModifier : SimpleSkillModifier<GurrenLagannThrowingShades>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            var shadesWhirlPrefab = Projectiles.shadesWhirlPrefab;
            if (shadesWhirlPrefab)
            {
                var projectileDotZone = shadesWhirlPrefab.GetComponent<ProjectileDotZone>();
                if (projectileDotZone)
                {
                    projectileDotZone.damageCoefficient = MultScaling(0.20f, 0.20f, level); // increase Dot damage by 20% every level
                }
                ProjectileController projectileController = shadesWhirlPrefab.GetComponent<ProjectileController>();
                if (projectileController)
                {
                    projectileController.ghostPrefab.transform.localScale = Vector3.one * Math.Min(16f, MultScaling(4.0f, 0.15f, level));// increase shades size by 15% every level (Max 4x original size)
                }
                var hitbox = shadesWhirlPrefab.GetComponentInChildren<HitBox>();
                if (hitbox)
                {
                    hitbox.transform.localScale = new Vector3(8.0f, 3.0f, 8.0f) * Math.Min(4f, MultScaling(1.0f, 0.15f, level));
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
            GurrenLagannTornadoKick.damageCoefficient = MultScaling(1.5f, 0.15f, level);// increase damage by 15% every level
            GurrenLagannTornadoKick.jumpVelocity = MultScaling(7.0f, 0.25f, level); // increase jump velocity by 25% every level
            GurrenLagannTornadoKick.canControlDirection = (level >= 4); // Can control the direction mid move at level 4
        }        
    }

    [SkillLevelModifier("GurrenLagannGigaDrillMaximum", typeof(GurrenLagannGigaDrillMaximum))]
    class GurrenLagannGigaDrillMaximumSkillModifier : SimpleSkillModifier<GurrenLagannGigaDrillMaximum>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            GurrenLagannGigaDrillMaximum.energyCost = MultScaling(50f, -0.10f, level);// decrease spiral energy cost by 10% every level
            GurrenLagannGigaDrillMaximum.c_DamageCoefficient = MultScaling(7.5f, 0.10f, level);// increase damage by 10% every level
            GurrenLagannGigaDrillMaximum.canBypassArmor = (level >= 4); // Can bypass armor at level 4
        }
    }

    [SkillLevelModifier("GurrenLagannSplit", typeof(GurrenLagannSplit))]
    class GurrenLagannSplitSkillModifier : SimpleSkillModifier<GurrenLagannSplit>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            GurrenLagannSplit.carryOverEnergyPercent = Math.Max(0f, MultScaling(1f, -0.1f, level));// increase spiral energy that can be carried over by 10% every level
        }
    }
}
