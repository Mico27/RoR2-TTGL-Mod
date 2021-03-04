using TTGL_Survivor.SkillStates.BaseStates;
using RoR2;
using UnityEngine;

namespace TTGL_Survivor.SkillStates
{
    public class DrillRush : BaseMeleeAttack
    {
        public const float c_DamageCoefficient = 3.0f;
        public override void OnEnter()
        {
            this.hitboxName = "DrillRushHitbox";
            this.playbackRateString = "DrillRush.playbackRate";
            this.damageType = DamageType.BypassArmor;
            this.damageCoefficient = c_DamageCoefficient;
            this.procCoefficient = 1f;
            this.pushForce = 500f;
            this.bonusForce = Vector3.zero;
            this.baseDuration = 0.8f;
            this.attackStartTime = 0.2f;
            this.attackEndTime = 0.4f;
            this.baseEarlyExitTime = 0.3f;
            this.hitStopDuration = 0.115f;
            this.attackRecoil = 0.75f;
            this.hitHopVelocity = 6f;

            this.swingSoundString = "HenryPunchSwing";
            this.muzzleString = swingIndex % 2 == 0 ? "LeftDrillMuzzle" : "RightDrillMuzzle";
            //this.swingEffectPrefab = Modules.Assets.swordSwingEffect;
            this.hitEffectPrefab = Modules.Assets.punchImpactEffect;

            this.impactSound = Modules.Assets.drillRushHitSoundEvent.index;

            base.OnEnter();
        }

        protected override void PlayAttackAnimation()
        {
            base.PlayCrossfade("Gesture, Override", "DrillRush" + (1 + swingIndex), this.playbackRateString, this.duration, 0.05f);
        }

        protected override void PlaySwingEffect()
        {
            //base.PlaySwingEffect();
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();
        }

        protected override void SetNextState()
        {
            int index = this.swingIndex;
            if (index == 0) index = 1;
            else index = 0;

            this.outer.SetNextState(new DrillRush
            {
                swingIndex = index
            });
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}