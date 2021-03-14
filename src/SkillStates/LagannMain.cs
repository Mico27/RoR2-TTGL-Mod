using UnityEngine;
using EntityStates;
using RoR2;
using UnityEngine.Networking;

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
            On.RoR2.CharacterBody.AddBuff += CharacterBody_AddBuff;
            On.RoR2.CharacterBody.AddTimedBuff += CharacterBody_AddTimedBuff;
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
            On.RoR2.CharacterBody.AddBuff -= CharacterBody_AddBuff;
            On.RoR2.CharacterBody.AddTimedBuff -= CharacterBody_AddTimedBuff;
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
                this.characterBody.armor += ToggleCanopy.c_ArmorBuffAmount;
            }
        }

        private void CharacterBody_AddTimedBuff(On.RoR2.CharacterBody.orig_AddTimedBuff orig, CharacterBody self, BuffIndex buffType, float duration)
        {
            if (self == this.characterBody && this.HasBuff(Modules.Buffs.canopyBuff) &&
                (buffType == BuffIndex.Entangle ||
                    buffType == BuffIndex.Nullified ||
                    buffType == BuffIndex.Slow50 ||
                    buffType == BuffIndex.Slow60 ||
                    buffType == BuffIndex.Slow80 ||
                    buffType == BuffIndex.ClayGoo ||
                    buffType == BuffIndex.Slow30 ||
                    buffType == BuffIndex.Cripple))
            {
                return;
            }
            orig(self, buffType, duration);
        }

        private void CharacterBody_AddBuff(On.RoR2.CharacterBody.orig_AddBuff orig, CharacterBody self, BuffIndex buffType)
        {
            if (self == this.characterBody && this.HasBuff(Modules.Buffs.canopyBuff) &&
                (buffType == BuffIndex.Entangle ||
                    buffType == BuffIndex.Nullified ||
                    buffType == BuffIndex.Slow50 ||
                    buffType == BuffIndex.Slow60 ||
                    buffType == BuffIndex.Slow80 ||
                    buffType == BuffIndex.ClayGoo ||
                    buffType == BuffIndex.Slow30 ||
                    buffType == BuffIndex.Cripple))
            {
                return;
            }
            orig(self, buffType);
        }

    }
}