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
            LagannDrillRush.damageCoefficient = MultScaling(3.0f, 0.10f, level);// increase damage by 10% every level
            LagannController.drillSizeMultiplier = Math.Min(8f, MultScaling(1.0f, 0.20f, level)); // increase drill size by 20% every level (Max 8x original size)
            LagannDrillRush.spiralEnergyPercentagePerHit = (level >= 4)? 0.01f: 0f; // can gain extra spiral power on hit at level 4
        }

    }

    [SkillLevelModifier("YokoShootRifle", typeof(YokoShootRifle))]
    class YokoShootRifleSkillModifier : SimpleSkillModifier<YokoShootRifle>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            skillDef.baseMaxStock = AdditiveScaling(1, 1, level);// increase ammo by 1 every level
            YokoShootRifle.maxRicochetCount = AdditiveScaling(2, 3, level); // increase max ricochet count by 3 every level
            YokoShootRifle.resetBouncedObjects = (level >= 4); // ricochet can hit back previously hit enemies at level 4
        }
    }

    [SkillLevelModifier("YokoExplosiveRifle", typeof(YokoExplosiveRifle))]
    class YokoExplosiveRifleSkillModifier : SimpleSkillModifier<YokoExplosiveRifle>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            skillDef.baseMaxStock = AdditiveScaling(1, 1, level);// increase ammo by 1 every level
            Projectiles.UpdateYokoExposionScale(Math.Min(3f, MultScaling(1.0f, 0.10f, level))); // increase explosion size by 10% every level (Max 3x original size)
            Projectiles.UpdateYokoExplosionCluster(level >= 4); // explosion spawns a cluster of smaller explosions at level 4
        }
    }

    [SkillLevelModifier("YokoScepterRifle", typeof(YokoScepterRifle))]
    class YokoScepterRifleSkillModifier : SimpleSkillModifier<YokoScepterRifle>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            skillDef.baseMaxStock = AdditiveScaling(1, 2, level);// increase ammo by 2 every level
            YokoScepterRifle.maxRicochetCount = AdditiveScaling(2, 2, level); // increase max ricochet count by 2 every level
            YokoScepterRifle.explosionSizeMultiplier = Math.Min(3f, MultScaling(1.0f, 0.15f, level)); // increase explosion size by 15% every level (Max 3x original size)
            YokoScepterRifle.resetBouncedObjects = (level >= 4); // ricochet can hit back previously hit enemies at level 4
            
        }
    }

    [SkillLevelModifier("LagannSpiralBurst", typeof(LagannSpiralBurst))]
    class LagannSpiralBurstSkillModifier : SimpleSkillModifier<LagannSpiralBurst>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            LagannSpiralBurst.damageCoefficient = MultScaling(2.5f, 0.30f, level);// increase damage by 30% every level
            LagannSpiralBurst.jumpVelocity = MultScaling(7.0f, 0.10f, level); // increase jump velocity by 10% every level
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
            LagannToggleCanopy.armorBuffAmount = MultScaling(150f, 0.20f, level);// increase armor from buff by 20% every level
            if (Lagann.explosiveRifleSkillDef)
            {
                Lagann.explosiveRifleSkillDef.fullRestockOnAssign = (level >= 4); // restock yoko's rifle on use
            }
            if (Lagann.shootRifleSkillDef)
            {
                Lagann.shootRifleSkillDef.fullRestockOnAssign = (level >= 4);
            }
            if (Lagann.scepterSkillDef)
            {
                Lagann.scepterSkillDef.fullRestockOnAssign = (level >= 4);
            }
        }
    }

    [SkillLevelModifier("LagannImpact", typeof(PrepareLagannImpact), typeof(AimLagannImpact), typeof(LagannImpact))]
    class LagannImpactSkillModifier : BaseSkillModifier
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            AimLagannImpact.maxRebound = AdditiveScaling(2, 1, level);// increase maximum number of rebound by 1 every level
            AimLagannImpact.maxStepDistance = MultScaling(100f, 0.25f, level);// increase shooting distance by 25% every level
            LagannImpact.damageCoefficient = MultScaling(15f, 0.10f, level);// increase damage by 10% every level
        }
    }

    [SkillLevelModifier("LagannCombine", typeof(LagannCombine))]
    class LagannCombineSkillModifier : SimpleSkillModifier<LagannCombine>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            LagannCombineSkillDef lagannCombineSkillDef = skillDef as LagannCombineSkillDef;
            if (lagannCombineSkillDef)
            {
                lagannCombineSkillDef.energyCost = MultScaling(100f, -0.10f, level); //decrease energy cost by 10% every level
            }
        }
    }
}
