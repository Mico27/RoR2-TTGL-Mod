using EntityStates;
using EntityStates.Loader;
using EntityStates.Mage.Weapon;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using System.Collections.Generic;
using TTGL_Survivor.Modules;
using TTGL_Survivor.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class YokoPiercingRifleCharging : BaseSkillState
    {
        public static float baseChargeDuration = 0.5f;

        public float chargeDuration;
        public float charge = 1f;
        private float durationStopWatch = 0f;
        private GameObject chargeEffect;
        private uint chargeLoopSFX;

        public override void OnEnter()
        {
            base.OnEnter();
            this.chargeDuration = YokoPiercingRifleCharging.baseChargeDuration / this.attackSpeedStat;
            base.characterBody.SetAimTimer(this.chargeDuration);
            var muzzleTransform = base.GetModelChildLocator().FindChild("Muzzle");
            Util.PlaySound(BaseChargeFist.enterSFXString, base.gameObject);
            this.chargeLoopSFX = Util.PlaySound(BaseChargeFist.startChargeLoopSFXString, base.gameObject);
            this.chargeEffect = UnityEngine.Object.Instantiate<GameObject>(
              Assets.mainAssetBundle.LoadAsset<GameObject>("YokoRifleCharge"),
              muzzleTransform
              );
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            var stock = base.activatorSkillSlot.stock;
            var maxStock = base.activatorSkillSlot.maxStock;
            var percentUsedStock = ((maxStock > 0f) ? Mathf.Clamp01((maxStock - stock) / maxStock) : 0f) * 100f;
            AkSoundEngine.SetRTPCValueByPlayingID("loaderShift_chargeAmount", percentUsedStock, this.chargeLoopSFX);
            this.chargeEffect.transform.localScale = Vector3.one * (this.charge / 5f);
            this.durationStopWatch += Time.fixedDeltaTime;
            if (this.durationStopWatch > chargeDuration)
            {
                base.characterBody.SetAimTimer(this.chargeDuration);
                this.charge += 1;
                this.durationStopWatch = 0f;
                if (stock > 0)
                {
                    base.activatorSkillSlot.DeductStock(1);                    
                }
                else if (base.isAuthority)
                {
                    this.outer.SetNextState(new YokoPiercingRifle() { charge = this.charge });
                    return;
                }
            }
            if (base.isAuthority && !base.IsKeyDownAuthority())
            {
                this.outer.SetNextState(new YokoPiercingRifle() { charge = this.charge });
                return;
            }
        }

        public override void OnExit()
        {
            Util.PlaySound(BaseChargeFist.endChargeLoopSFXString, base.gameObject);
            EntityState.Destroy(this.chargeEffect);
            base.OnExit();
        }
        
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }

    }
}