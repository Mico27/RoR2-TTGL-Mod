using EntityStates;
using EntityStates.Huntress;
using EntityStates.Loader;
using RoR2;
using RoR2.Projectile;
using TTGL_Survivor.Modules;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class BaseSpiralConversion : BaseSkillState
    {
        public static float conversionSpeed = 0.05f;
        public static float conversionRatio = 0.6f;
        protected SpiralEnergyComponent spiralEnergyComponent;
        protected GameObject chargeEffect;
        protected uint chargeLoopSFX;
        protected float initialEnergy;
        protected float previousEnergy;

        public override void OnEnter()
        {
            base.OnEnter();
            spiralEnergyComponent = base.GetComponent<SpiralEnergyComponent>();            
            Util.PlaySound(BaseChargeFist.enterSFXString, base.gameObject);
            this.chargeLoopSFX = Util.PlaySound(BaseChargeFist.startChargeLoopSFXString, base.gameObject);
            PlayEnterAnimation();
            CreateChargeEffect();
            var healthComponent = base.healthComponent;
            this.initialEnergy = healthComponent.barrier + healthComponent.shield;
            this.previousEnergy = initialEnergy;
            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2.RoR2Content.Buffs.HiddenInvincibility);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            var healthComponent = base.healthComponent;
            var currentEnergy = healthComponent.barrier + healthComponent.shield;
            var charge = (this.initialEnergy > 0f) ? Mathf.Clamp01((this.initialEnergy - currentEnergy) / this.initialEnergy): 0f;
            AkSoundEngine.SetRTPCValueByPlayingID("loaderShift_chargeAmount", charge * 100f, this.chargeLoopSFX);
            if (NetworkServer.active && spiralEnergyComponent && base.healthComponent.fullCombinedHealth > 0f)
            {
                if (currentEnergy < previousEnergy)
                {
                    CorrectSpiralEnergy(previousEnergy - currentEnergy);
                }
                if (currentEnergy > 0)
                {
                    ConvertSpiralEnergy();
                }
            }
            if (base.isAuthority && (currentEnergy <= 0f || !base.IsKeyDownAuthority()))
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2.RoR2Content.Buffs.HiddenInvincibility);
            }
            Util.PlaySound(BaseChargeFist.endChargeLoopSFXString, base.gameObject);
            DestroyChargeEffect();
            PlayExitAnimation();
            base.OnExit();
        }
               
        protected virtual void PlayEnterAnimation()
        {
            base.PlayAnimation("FullBody, Override", "GurrenLagannSpiralConversion");
        }

        protected virtual void PlayExitAnimation()
        {
            base.PlayAnimation("FullBody, Override", "BufferEmpty");
        }

        protected virtual void CreateChargeEffect()
        {
            this.chargeEffect = UnityEngine.Object.Instantiate<GameObject>(
               Assets.LoadAsset<GameObject>("SpiralSlowCharge"),
               base.characterBody.coreTransform
               );
        }

        protected virtual void DestroyChargeEffect()
        {
            EntityState.Destroy(this.chargeEffect);
        }

        private void CorrectSpiralEnergy(float missingEnergy)
        {
            var energyPercentil = missingEnergy / (base.healthComponent.fullCombinedHealth);
            spiralEnergyComponent.AddSpiralEnergy(energyPercentil * conversionRatio);
        }

        private void ConvertSpiralEnergy()
        {
            var healthComponent = base.healthComponent;
            var cost = healthComponent.fullCombinedHealth * conversionSpeed * Time.fixedDeltaTime;
            var initialCost = cost;
            if (cost > 0f && healthComponent.barrier > 0f)
            {
                if (cost <= healthComponent.barrier)
                {
                    healthComponent.Networkbarrier = healthComponent.barrier - cost;
                    cost = 0f;
                }
                else
                {
                    cost -= healthComponent.barrier;
                    healthComponent.Networkbarrier = 0f;
                }
            }
            if (cost > 0f && healthComponent.shield > 0f)
            {
                if (cost <= healthComponent.shield)
                {
                    healthComponent.Networkshield = healthComponent.shield - cost;
                    cost = 0f;
                }
                else
                {
                    cost -= healthComponent.shield;
                    healthComponent.Networkshield = 0f;
                    float scale = 1f;
                    if (base.characterBody)
                    {
                        scale = base.characterBody.radius;
                    }
                    EffectManager.SpawnEffect(HealthComponent.AssetReferences.shieldBreakEffectPrefab, new EffectData
                    {
                        origin = base.transform.position,
                        scale = scale
                    }, true);
                }
            }
            previousEnergy = healthComponent.barrier + healthComponent.shield;
            var energyPercentil = (initialCost - Mathf.Max(cost, 0)) / (base.healthComponent.fullCombinedHealth);
            spiralEnergyComponent.AddSpiralEnergy(energyPercentil * SpiralEnergyComponent.C_SPIRALENERGYCAP * conversionRatio);
            spiralEnergyComponent.energyUptimeStopwatch = 5f;
            base.characterBody.outOfDangerStopwatch = 0f;
        }
    }
}