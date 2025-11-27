using EntityStates;
using EntityStates.Huntress;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace TTGL_Survivor.SkillStates
{
    public class PrepareLagannImpact : BaseSkillState
    {        
        public override void OnEnter()
        {
            base.OnEnter();
            this.cancelled = true;
            Util.PlaySound(BaseBeginArrowBarrage.blinkSoundString, base.gameObject);
            this.modelTransform = base.GetModelTransform();
            this.animator = base.GetModelAnimator();
            if (this.modelTransform)
            {
                this.hurtboxGroup = this.modelTransform.GetComponent<HurtBoxGroup>();
            }
            this.prepDuration = this.basePrepDuration;
            base.PlayAnimation("FullBody, Override", "LagannImpact1");
            if (base.characterMotor)
            {
                base.characterMotor.velocity = Vector3.zero;
            }

            if (this.hurtboxGroup)
            {
                HurtBoxGroup hurtBoxGroup = this.hurtboxGroup;
                hurtBoxGroup.hurtBoxesDeactivatorCounter++;
            }
            if (this.isGrounded)
            {
                this.isJumping = true;
                base.SmallHop(base.characterMotor, this.jumpCoefficient);
                
                //this.CreateBlinkEffect(base.transform.position);
            }
        }

        public override void OnExit()
        {
            //this.CreateBlinkEffect(base.transform.position);
            if (this.hurtboxGroup)
            {
                HurtBoxGroup hurtBoxGroup = this.hurtboxGroup;
                hurtBoxGroup.hurtBoxesDeactivatorCounter--;
            }
            if (this.cancelled)
            {
                base.PlayCrossfade("FullBody, Override", "LagannImpactExit", 0.2f);
            }
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();        
            if (!this.isJumping)
            {
                base.characterMotor.velocity = Vector3.zero;
            }
            if (base.fixedAge >= this.prepDuration && base.isAuthority)
            {
                this.cancelled = false;
                this.outer.SetNextState(new AimLagannImpact());
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        private Transform modelTransform;
        [SerializeField]
        public float basePrepDuration = 1.0f;
        [SerializeField]
        public float jumpCoefficient = 30f;  
        private float prepDuration;
        private bool isJumping;        
        private HurtBoxGroup hurtboxGroup;
        private Animator animator;
        private bool cancelled;
    }
}