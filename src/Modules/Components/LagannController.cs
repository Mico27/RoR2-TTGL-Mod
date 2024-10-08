﻿using RoR2;
using System;
using System.Runtime.CompilerServices;
using TTGL_Survivor.Modules.Survivors;
using TTGL_Survivor.SkillStates;
using UnityEngine;

namespace TTGL_Survivor.Modules.Components
{
    // just a class to run some custom code for things like weapon models
    public class LagannController : MonoBehaviour
    {
        public static float drillSizeMultiplier = 1.0f;

        private CharacterBody body;
        private Animator animator;
        private GenericSkill yokoSkill;
        private SetStateOnHurt setStateOnHurt;
        private SkillLocator skillLocator;
        private ChildLocator childLocator;
        private bool defaultCanBeFrozen;
        private bool defaultCanBeStunned;
        private bool defaultCanBeHitStunned;
        private bool hadCanopyBuff;
        private bool hadFullSpiralPowerBuff;
        private GurrenMinionCache gurrenMinionCache;

        private Transform rightDrillBone;
        private Transform leftDrillBone;
        private Transform drillRushHitboxPivot;

        public void Awake()
        {
            this.body = base.GetComponent<CharacterBody>();
            var model = base.GetComponent<ModelLocator>();
            var modelTransform = model.modelTransform;
            this.childLocator = modelTransform.GetComponent<ChildLocator>();
            this.rightDrillBone = childLocator.FindChild("RightHandDrill");
            this.leftDrillBone = childLocator.FindChild("LeftHandDrill");
            this.drillRushHitboxPivot = childLocator.FindChild("DrillRushHitboxPivot");
            this.animator = modelTransform.GetComponent<Animator>();            
            this.setStateOnHurt = base.GetComponent<SetStateOnHurt>();
            this.skillLocator = base.GetComponent<SkillLocator>();
            
            if (this.setStateOnHurt)
            {
                this.defaultCanBeFrozen = setStateOnHurt.canBeFrozen;
                this.defaultCanBeStunned = setStateOnHurt.canBeStunned;
                this.defaultCanBeHitStunned = setStateOnHurt.canBeHitStunned;
            }            
            if (this.skillLocator)
            {
                this.yokoSkill = this.skillLocator.secondary;
            }            
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.CharacterBody.AddBuff_BuffDef += CharacterBody_AddBuff_BuffDef;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += CharacterBody_AddTimedBuff_BuffDef_float;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float_int += CharacterBody_AddTimedBuff_BuffDef_float_int;
        }
        
        public void OnDestroy()
        {
            if (this.setStateOnHurt)
            {
                setStateOnHurt.canBeFrozen = this.defaultCanBeFrozen;
                setStateOnHurt.canBeStunned = this.defaultCanBeStunned;
                setStateOnHurt.canBeHitStunned = this.defaultCanBeHitStunned;
            }
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
            On.RoR2.CharacterBody.AddBuff_BuffDef -= CharacterBody_AddBuff_BuffDef;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float -= CharacterBody_AddTimedBuff_BuffDef_float;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float_int -= CharacterBody_AddTimedBuff_BuffDef_float_int;
        }

        private void LateUpdate()
        {
            if (drillSizeMultiplier != 1f)
            {
                if (this.drillRushHitboxPivot)
                {
                    this.drillRushHitboxPivot.localScale = (Vector3.one * drillSizeMultiplier);
                }
                if (this.rightDrillBone)
                {
                    this.rightDrillBone.localScale *= drillSizeMultiplier;
                }
                if (this.leftDrillBone)
                {
                    this.leftDrillBone.localScale *= drillSizeMultiplier;
                }
            }
        }

        private void FixedUpdate()
        {
            if (this.animator)
            {
                float i = 1;
                if (this.animator.GetBool("isGrounded")) i = 0;
                this.animator.SetFloat("inAir", i);

                UpdateMaxSpiralPowerBuffEffects();
                UpdateCanopyBuffEffects();
                UpdateDisplayKamina();
            }
        }

        private void UpdateMaxSpiralPowerBuffEffects()
        {
            if (this.body.HasBuff(Modules.Buffs.maxSpiralPowerBuff))
            {
                if (!hadFullSpiralPowerBuff)
                {
                    hadFullSpiralPowerBuff = true;
                    this.animator.SetBool("spiralPowerOverflow", true);
                }
            }
            else
            {
                if (hadFullSpiralPowerBuff)
                {
                    hadFullSpiralPowerBuff = false;
                    this.animator.SetBool("spiralPowerOverflow", false);
                }
            }
        }

        private void UpdateCanopyBuffEffects()
        {
            if (this.body.HasBuff(Modules.Buffs.canopyBuff))
            {
                if (!hadCanopyBuff)
                {
                    hadCanopyBuff = true;
                    if (this.yokoSkill)
                    {
                        this.yokoSkill.SetSkillOverride("CanopyBlock", Lagann.canopyOverrideSkillDef, GenericSkill.SkillOverridePriority.Contextual);
                    }
                    this.animator.SetBool("isCanopyClosed", true);
                    if (this.setStateOnHurt)
                    {
                        this.defaultCanBeFrozen = setStateOnHurt.canBeFrozen;
                        this.defaultCanBeHitStunned = setStateOnHurt.canBeHitStunned;
                        this.defaultCanBeStunned = setStateOnHurt.canBeStunned;
                        setStateOnHurt.canBeFrozen = false;
                        setStateOnHurt.canBeHitStunned = false;
                        setStateOnHurt.canBeStunned = false;
                    }
                }

            }
            else
            {
                if (hadCanopyBuff)
                {
                    hadCanopyBuff = false;
                    if (this.yokoSkill)
                    {
                        this.yokoSkill.UnsetSkillOverride("CanopyBlock", Lagann.canopyOverrideSkillDef, GenericSkill.SkillOverridePriority.Contextual);
                    }
                    this.animator.SetBool("isCanopyClosed", false);
                    if (this.setStateOnHurt)
                    {
                        setStateOnHurt.canBeFrozen = this.defaultCanBeFrozen;
                        setStateOnHurt.canBeHitStunned = this.defaultCanBeHitStunned;
                        setStateOnHurt.canBeStunned = this.defaultCanBeStunned;
                    }
                }
            }
        }

        private void UpdateDisplayKamina()
        {
            if (!this.gurrenMinionCache && this.body && this.body.master)
            {
                this.gurrenMinionCache = GurrenMinionCache.GetOrSetGurrenStatusCache(this.body.master);
            }
            if (this.gurrenMinionCache && this.gurrenMinionCache.gurrenMinion)
            {
                this.animator.SetBool("hideKamina", true);
                this.SetCanRide(true);
            }
            else
            {
                this.animator.SetBool("hideKamina", false);
                this.SetCanRide(false);
            }
        }

        private void SetCanRide(bool canRide)
        {
            if (TTGL_SurvivorPlugin.rideMeExtendedInstalled)
            {
                this.DoSetCanRide(canRide);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void DoSetCanRide(bool canRide)
        {
            var rideableController = genericRideableController as RideMeExtended.RideableController;
            if (!rideableController)
            {
                rideableController = base.GetComponent<RideMeExtended.RideableController>();
            }
            if (rideableController && rideableController.CanRide != canRide)
            {
                rideableController.SetCanRide(canRide);
            }
        }
        MonoBehaviour genericRideableController;

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self == this.body && self.HasBuff(Modules.Buffs.canopyBuff))
            {
                self.armor += LagannToggleCanopy.armorBuffAmount;
            }
        }


        private void CharacterBody_AddTimedBuff_BuffDef_float_int(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float_int orig, CharacterBody self, BuffDef buffDef, float duration, int maxStacks)
        {
            if (CanCancelBuff(self, buffDef))
            {
                return;
            }
            orig(self, buffDef, duration, maxStacks);
        }

        private void CharacterBody_AddTimedBuff_BuffDef_float(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
            if (CanCancelBuff(self, buffDef))
            {
                return;
            }
            orig(self, buffDef, duration);
        }

        private void CharacterBody_AddBuff_BuffDef(On.RoR2.CharacterBody.orig_AddBuff_BuffDef orig, CharacterBody self, BuffDef buffDef)
        {
            if (CanCancelBuff(self, buffDef))
            {
                return;
            }
            orig(self, buffDef);
        }

        private bool CanCancelBuff(CharacterBody self, BuffDef buffDef)
        {
            return (self == this.body && self.HasBuff(Modules.Buffs.canopyBuff) &&
                (buffDef.isDebuff && !buffDef.isHidden));
        }

    }
}