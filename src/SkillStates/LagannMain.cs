using UnityEngine;
using EntityStates;
using RoR2;
using UnityEngine.Networking;
using static RoR2.RoR2Content;

namespace TTGL_Survivor.SkillStates
{
    public class LagannMain : GenericCharacterMain
    {
        private Animator animator;
        private GenericSkill yokoSkill;
        private SetStateOnHurt setStateOnHurt;
        private bool defaultCanBeFrozen;
        private bool defaultCanBeStunned;
        private bool defaultCanBeHitStunned;
        private bool hadCanopyBuff;
        private bool hadFullSpiralPowerBuff;
        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
            this.setStateOnHurt = base.GetComponent<SetStateOnHurt>();
            if (this.setStateOnHurt)
            {
                this.defaultCanBeFrozen = setStateOnHurt.canBeFrozen;
                this.defaultCanBeStunned = setStateOnHurt.canBeStunned;
                this.defaultCanBeHitStunned = setStateOnHurt.canBeHitStunned;
            }
            if (base.skillLocator)
            {
                this.yokoSkill = base.skillLocator.secondary;
            }
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.CharacterBody.AddBuff_BuffDef += CharacterBody_AddBuff_BuffDef; ;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += CharacterBody_AddTimedBuff_BuffDef_float; ;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float_int += CharacterBody_AddTimedBuff_BuffDef_float_int;
        }
        
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (this.animator)
            {
                float i = 1;
                if (this.animator.GetBool("isGrounded")) i = 0;
                this.animator.SetFloat("inAir", i);

                UpdateMaxSpiralPowerBuffEffects();
                UpdateCanopyBuffEffects();
            }
            
        }

        private void UpdateMaxSpiralPowerBuffEffects()
        {
            if (this.HasBuff(Modules.Buffs.maxSpiralPowerBuff))
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
            if (this.HasBuff(Modules.Buffs.canopyBuff))
            {                
                if (!hadCanopyBuff)
                {
                    hadCanopyBuff = true;
                    if (this.yokoSkill)
                    {
                        yokoSkill.RemoveAllStocks();
                        yokoSkill.enabled = false;
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
                        yokoSkill.enabled = true;
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

        public override void OnExit()
        {
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
            On.RoR2.CharacterBody.AddBuff_BuffDef -= CharacterBody_AddBuff_BuffDef; ;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float -= CharacterBody_AddTimedBuff_BuffDef_float; ;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float_int -= CharacterBody_AddTimedBuff_BuffDef_float_int;
            if (this.setStateOnHurt)
            {
                setStateOnHurt.canBeFrozen = this.defaultCanBeFrozen;
                setStateOnHurt.canBeStunned = this.defaultCanBeStunned;
                setStateOnHurt.canBeHitStunned = this.defaultCanBeHitStunned;
            }
            base.OnExit();
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            // a simple stat hook, adds armor after stats are recalculated
            if (self == this.characterBody && this.HasBuff(Modules.Buffs.canopyBuff))
            {
                this.characterBody.armor += LagannToggleCanopy.c_ArmorBuffAmount;
            }
        }


        private void CharacterBody_AddTimedBuff_BuffDef_float_int(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float_int orig, CharacterBody self, BuffDef buffDef, float duration, int maxStacks)
        {
            if (this.CanCancelBuff(self, buffDef))
            {
                return;
            }
            orig(self, buffDef, duration, maxStacks);
        }

        private void CharacterBody_AddTimedBuff_BuffDef_float(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
            if (this.CanCancelBuff(self, buffDef))
            {
                return;
            }
            orig(self, buffDef, duration);
        }

        private void CharacterBody_AddBuff_BuffDef(On.RoR2.CharacterBody.orig_AddBuff_BuffDef orig, CharacterBody self, BuffDef buffDef)
        {
            if (this.CanCancelBuff(self, buffDef))
            {
                return;
            }
            orig(self, buffDef);
        }
        
        private bool CanCancelBuff(CharacterBody self, BuffDef buffDef)
        {
            return (self == this.characterBody && this.HasBuff(Modules.Buffs.canopyBuff) &&
                (buffDef.buffIndex == Buffs.Entangle.buffIndex ||
                    buffDef.buffIndex == Buffs.Nullified.buffIndex ||
                    buffDef.buffIndex == Buffs.Slow50.buffIndex ||
                    buffDef.buffIndex == Buffs.Slow60.buffIndex ||
                    buffDef.buffIndex == Buffs.Slow80.buffIndex ||
                    buffDef.buffIndex == Buffs.ClayGoo.buffIndex ||
                    buffDef.buffIndex == Buffs.Slow30.buffIndex ||
                    buffDef.buffIndex == Buffs.Cripple.buffIndex));
        }
    }
}