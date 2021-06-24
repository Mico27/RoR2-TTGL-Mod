using RoR2;
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
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillModifiers
{
    [SkillLevelModifier("LagannDrillRush", typeof(LagannDrillRush))]
    class LagannDrillRushSkillModifier : SimpleSkillModifier<LagannDrillRush>
    {       
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            var capedLevel = Math.Min(25, level);
            LagannDrillRush.damageCoefficient = AdditiveScaling(3.0f, 0.30f, capedLevel);// increase damage by 10% every level (linear)
            LagannController.drillSizeMultiplier = AdditiveScaling(1.0f, 0.5f, capedLevel); // increase drill size by 50% every level (linear)
            LagannDrillRush.spiralEnergyPercentagePerHit = (capedLevel >= 4)? 0.01f: 0f; // can gain extra spiral power on hit at level 4
        }

    }

    [SkillLevelModifier("YokoShootRifle", typeof(YokoShootRifle))]
    class YokoShootRifleSkillModifier : SimpleSkillModifier<YokoShootRifle>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            var capedLevel = Math.Min(25, level);
            skillDef.baseMaxStock = AdditiveScaling(1, 1, capedLevel);// increase ammo by 1 every level
            YokoShootRifle.maxRicochetCount = AdditiveScaling(2, 3, capedLevel); // increase max ricochet count by 3 every level
            YokoShootRifle.resetBouncedObjects = (capedLevel >= 4); // ricochet can hit back previously hit enemies at level 4
        }
    }

    [SkillLevelModifier("YokoExplosiveRifle", typeof(YokoExplosiveRifle))]
    class YokoExplosiveRifleSkillModifier : SimpleSkillModifier<YokoExplosiveRifle>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            var capedLevel = Math.Min(25, level);
            skillDef.baseMaxStock = AdditiveScaling(1, 1, capedLevel);// increase ammo by 1 every level
            Projectiles.UpdateYokoExposionScale(AdditiveScaling(1.0f, 0.10f, capedLevel)); // increase explosion size by 10% every level (Linear)
            Projectiles.UpdateYokoExplosionCluster(capedLevel >= 4); // explosion spawns a cluster of smaller explosions at level 4
        }
    }

    [SkillLevelModifier("YokoScepterRifle", typeof(YokoScepterRifle))]
    class YokoScepterRifleSkillModifier : SimpleSkillModifier<YokoScepterRifle>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            var capedLevel = Math.Min(25, level);
            skillDef.baseMaxStock = AdditiveScaling(1, 2, capedLevel);// increase ammo by 2 every level
            YokoScepterRifle.maxRicochetCount = AdditiveScaling(2, 2, capedLevel); // increase max ricochet count by 2 every level
            YokoScepterRifle.explosionSizeMultiplier = AdditiveScaling(1.0f, 0.15f, capedLevel); // increase explosion size by 15% every level (Linear)
            YokoScepterRifle.resetBouncedObjects = (capedLevel >= 4); // ricochet can hit back previously hit enemies at level 4
            
        }
    }

    [SkillLevelModifier("LagannSpiralBurst", typeof(LagannSpiralBurst))]
    class LagannSpiralBurstSkillModifier : SimpleSkillModifier<LagannSpiralBurst>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            var capedLevel = Math.Min(25, level);
            LagannSpiralBurst.damageCoefficient = AdditiveScaling(2.5f, 0.75f, capedLevel);// increase damage by 30% every level (linear)
            LagannSpiralBurst.jumpVelocity = AdditiveScaling(7.0f, 0.70f, capedLevel); // increase jump velocity by 10% every level (linear)
        }
        
        public override void OnSkillExit(LagannSpiralBurst skillState, int level)
        {
            base.OnSkillExit(skillState, level);
            if (level >= 4 && skillState != null && skillState.characterBody && NetworkServer.active)
            {
                skillState.characterBody.AddTimedBuff(RoR2Content.Buffs.ArmorBoost, 5f);// get a 5 second armor buff on use at level 4
            }
        }
    }

    [SkillLevelModifier("LagannToggleCanopy", typeof(LagannToggleCanopy))]
    class LagannToggleCanopySkillModifier : SimpleSkillModifier<LagannToggleCanopy>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            var capedLevel = Math.Min(25, level);
            LagannToggleCanopy.armorBuffAmount = AdditiveScaling(150f, 15f, capedLevel);// increase armor from buff by 10% every level (linear)
            if (Lagann.explosiveRifleSkillDef)
            {
                Lagann.explosiveRifleSkillDef.fullRestockOnAssign = (capedLevel >= 4); // restock yoko's rifle on use
            }
            if (Lagann.shootRifleSkillDef)
            {
                Lagann.shootRifleSkillDef.fullRestockOnAssign = (capedLevel >= 4);
            }
            if (Lagann.scepterSkillDef)
            {
                Lagann.scepterSkillDef.fullRestockOnAssign = (capedLevel >= 4);
            }
        }
    }

    [SkillLevelModifier("LagannImpact", typeof(PrepareLagannImpact), typeof(AimLagannImpact), typeof(LagannImpact))]
    class LagannImpactSkillModifier : BaseSkillModifier
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            var capedLevel = Math.Min(25, level);
            AimLagannImpact.maxRebound = AdditiveScaling(2, 1, capedLevel);// increase maximum number of rebound by 1 every level
            AimLagannImpact.maxStepDistance = AdditiveScaling(100f, 25f, capedLevel);// increase shooting distance by 25% every level (linear)
            LagannImpact.damageCoefficient = AdditiveScaling(15f, 1.50f, capedLevel);// increase damage by 10% every level (linear)
        }
    }

    [SkillLevelModifier("LagannCombine", typeof(LagannCombine))]
    class LagannCombineSkillModifier : SimpleSkillModifier<LagannCombine>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            var capedLevel = Math.Min(25, level);
            LagannCombineSkillDef lagannCombineSkillDef = skillDef as LagannCombineSkillDef;
            if (lagannCombineSkillDef)
            {
                lagannCombineSkillDef.energyCost = MultScaling(100f, -0.10f, capedLevel); //decrease energy cost by 10% every level (exponential)
            }
        }
    }
}
