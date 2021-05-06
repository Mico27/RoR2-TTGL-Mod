using RoR2;
using TTGL_Survivor.Modules.Survivors;
using TTGL_Survivor.SkillStates;
using UnityEngine;

namespace TTGL_Survivor.Modules.Components
{
    // just a class to run some custom code for things like weapon models
    public class LagannController : MonoBehaviour
    {
        private CharacterBody body;
        private Animator animator;
        private GenericSkill yokoSkill;
        private SetStateOnHurt setStateOnHurt;
        private SkillLocator skillLocator;
        private bool defaultCanBeFrozen;
        private bool defaultCanBeStunned;
        private bool defaultCanBeHitStunned;
        private bool hadCanopyBuff;
        private bool hadFullSpiralPowerBuff;

        public void Awake()
        {
            this.body = base.GetComponent<CharacterBody>();
            var model = base.GetComponent<ModelLocator>();
            this.animator = model.modelTransform.GetComponent<Animator>();
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
            if (Interactables.gurrenFound)
            {
                this.animator.SetBool("hideKamina", true);
            }
            else
            {
                this.animator.SetBool("hideKamina", false);
            }
        }
        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self == this.body && self.HasBuff(Modules.Buffs.canopyBuff))
            {
                self.armor += LagannToggleCanopy.c_ArmorBuffAmount;
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
                (buffDef.buffIndex == RoR2Content.Buffs.Entangle.buffIndex ||
                    buffDef.buffIndex == RoR2Content.Buffs.Nullified.buffIndex ||
                    buffDef.buffIndex == RoR2Content.Buffs.Slow50.buffIndex ||
                    buffDef.buffIndex == RoR2Content.Buffs.Slow60.buffIndex ||
                    buffDef.buffIndex == RoR2Content.Buffs.Slow80.buffIndex ||
                    buffDef.buffIndex == RoR2Content.Buffs.ClayGoo.buffIndex ||
                    buffDef.buffIndex == RoR2Content.Buffs.Slow30.buffIndex ||
                    buffDef.buffIndex == RoR2Content.Buffs.Cripple.buffIndex));
        }

    }
}