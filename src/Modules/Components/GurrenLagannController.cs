using System;
using RoR2;
using TTGL_Survivor.Modules.Survivors;
using TTGL_Survivor.SkillStates;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.Modules.Components
{
    // just a class to run some custom code for things like weapon models
    public class GurrenLagannController : MonoBehaviour
    {
        private CharacterBody body;
        private Animator animator;
        private bool hadFullSpiralPowerBuff;
        private CharacterBody currentGigaDrillBreakerTarget;
        private GenericSkill secondarySkill;
        private SkillLocator skillLocator;
        private bool hadGigaDrillBreakPrerequisite;
        internal EntityStateMachine gigaDrillBreakTarget;

        public void Awake()
        {
            this.body = base.GetComponent<CharacterBody>();
            var model = base.GetComponent<ModelLocator>();
            this.animator = model.modelTransform.GetComponent<Animator>();
            this.skillLocator = base.GetComponent<SkillLocator>();
            if (this.skillLocator)
            {
                this.secondarySkill = this.skillLocator.secondary;
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
            }
            DetectGigaDrillBreakPrerequisites();
        }

        private void DetectGigaDrillBreakPrerequisites()
        {
            if (gigaDrillBreakTarget)
            {
                if (gigaDrillBreakTarget.state is GurrenLagannShadesConstrictState)
                {
                    return;
                }
                else
                {
                    gigaDrillBreakTarget = null;
                }
            }
            var monsters = TeamComponent.GetTeamMembers(TeamIndex.Monster);           
            if (monsters != null)
            {
                foreach(var monster in monsters)
                {
                    if (monster.body && monster.body.isBoss)
                    {
                        var entityState = monster.body.GetComponent<EntityStateMachine>();
                        if (entityState && entityState.state is GurrenLagannShadesConstrictState)
                        {
                            gigaDrillBreakTarget = entityState;
                            break;
                        }
                    }
                }
            }
            if (gigaDrillBreakTarget && !hadGigaDrillBreakPrerequisite)
            {
                hadGigaDrillBreakPrerequisite = true;
                if (this.secondarySkill)
                {
                    this.secondarySkill.SetSkillOverride("GigaDrillBreak", GurrenLagann.gigaDrillBreakerSkillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
            }
            else if (!gigaDrillBreakTarget && hadGigaDrillBreakPrerequisite)
            {
                hadGigaDrillBreakPrerequisite = false;
                if (this.secondarySkill)
                {
                    this.secondarySkill.UnsetSkillOverride("GigaDrillBreak", GurrenLagann.gigaDrillBreakerSkillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
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

        public void SetGigaDrillBreakerTarget(CharacterBody bossBody)
        {
            if (currentGigaDrillBreakerTarget != bossBody)
            {
                if (currentGigaDrillBreakerTarget)
                {
                    DetachShadesBinding();
                }
                currentGigaDrillBreakerTarget = bossBody;
                if (currentGigaDrillBreakerTarget)
                {
                    AttachShadesBinding(0);
                }             
            }
            else if (currentGigaDrillBreakerTarget)
            {
                AttachShadesBinding(1);
            }
            if (bossBody)
            {
                SetStateOnHurt.SetStunOnObject(bossBody.gameObject, 10f);
            }
        }

        private void AttachShadesBinding(int count)
        {
            if (currentGigaDrillBreakerTarget)
            {
                TTGL_Survivor.TTGL_SurvivorPlugin.instance.Logger.LogMessage("Boss " + currentGigaDrillBreakerTarget.GetDisplayName() + " was struck with Gurren Lagann's shades " + (count +1) + " time(s)!");
            }            
        }

        private void DetachShadesBinding()
        {
            if (currentGigaDrillBreakerTarget)
            {
                TTGL_Survivor.TTGL_SurvivorPlugin.instance.Logger.LogMessage("Boss " + currentGigaDrillBreakerTarget.GetDisplayName() + " was freed from Gurren Lagann's shades!");
            }
        }
    }
}