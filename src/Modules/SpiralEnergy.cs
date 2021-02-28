using UnityEngine;
using EntityStates;
using RoR2;
using TTGL_Survivor.UI;

namespace TTGL_Survivor.Modules
{
    public class SpiralEnergy : GenericEnergyComponent
    {
        public const float C_SPIRALENERGYCAP = 300.0f;
        private CharacterBody body = null;
        private float monsterCountCoefficient = 0.0f;
        private float healthCoefficient = 0.0f;

        public new void Awake()
        {
            base.Awake();
            this.capacity = C_SPIRALENERGYCAP;
            this.body = base.GetComponent<CharacterBody>();
            On.RoR2.TeamComponent.OnChangeTeam += TeamComponent_OnChangeTeam;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.HealthComponent.Heal += HealthComponent_Heal;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.UI.HUD.Awake += HUD_Awake;
        }

        public void OnDestroy()
        {
            On.RoR2.TeamComponent.OnChangeTeam -= TeamComponent_OnChangeTeam;
            On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
            On.RoR2.HealthComponent.Heal -= HealthComponent_Heal;
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
            On.RoR2.UI.HUD.Awake -= HUD_Awake;
        }

        private void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);
            if (self != null && self.mainContainer != null)
            {
                var existingSpiralEnergyGauge = self.mainContainer.GetComponentInChildren<SpiralPowerGauge>();
                if (!existingSpiralEnergyGauge)
                {
                    TTGL_SurvivorPlugin.instance.Logger.LogMessage("SpiralPowerNotFound, creating one");
                    var spiralPowerPanel = Instantiate(Modules.Assets.mainAssetBundle.LoadAsset<GameObject>("SpiralPowerPanel"));
                    TTGL_SurvivorPlugin.instance.Logger.LogMessage("SpiralPowerPanel asset loaded");
                    var spiritPowerGauge = spiralPowerPanel.AddComponent<SpiralPowerGauge>();
                    TTGL_SurvivorPlugin.instance.Logger.LogMessage("SpiralPowerGauge added to panel");
                    spiritPowerGauge.source = this;
                    TTGL_SurvivorPlugin.instance.Logger.LogMessage("Setting parent to maimainContainernUIPanel");
                    spiralPowerPanel.transform.SetParent(self.mainContainer.transform);
                    
                    TTGL_SurvivorPlugin.instance.Logger.LogMessage("Setting positioning");
                    var rectTransform = spiralPowerPanel.GetComponent<RectTransform>();
                    rectTransform.anchorMin = new Vector2(1, 0);
                    rectTransform.anchorMax = new Vector2(1, 0);
                    rectTransform.pivot = new Vector2(1, 0);
                    rectTransform.sizeDelta = new Vector2(120, 120);
                    rectTransform.anchoredPosition = new Vector2(-20, 200);
                    rectTransform.localScale = new Vector3(2, 2, 2);
                    
                    
                    TTGL_SurvivorPlugin.instance.Logger.LogMessage("hud completed");
                }
                else
                {
                    TTGL_SurvivorPlugin.instance.Logger.LogMessage("SpiralPower Found");
                    existingSpiralEnergyGauge.source = this;
                }
            }            
        }

        private void TeamComponent_OnChangeTeam(On.RoR2.TeamComponent.orig_OnChangeTeam orig, TeamComponent self, TeamIndex newTeamIndex)
        {
            orig(self, newTeamIndex);
            this.UpdateSpiralEnergyRegen(true);
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            // a simple stat hook, adds armor after stats are recalculated
            if (self && self == this.body)
            {
                this.UpdateSpiralEnergyRegen(false);                
                this.UpdateSpiralEnergyStatEffects();
            }
        }

        private float HealthComponent_Heal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            var result = orig(self, amount, procChainMask, nonRegen);
            if (self && self.body == this.body)
            {
                this.UpdateSpiralEnergyRegen(true);
            }
            return result;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);
            if (self && self.body == this.body)
            {
                this.UpdateSpiralEnergyRegen(true);
            }
        }

        private void UpdateSpiralEnergyRegen(bool setDirty)
        {
            var monsterCount = TeamComponent.GetTeamMembers(TeamIndex.Monster).Count;
            this.monsterCountCoefficient = ((float)monsterCount / 40.0f);
            if (this.body.healthComponent != null && this.body.healthComponent.fullCombinedHealth != 0)
            {
                this.healthCoefficient = this.body.healthComponent.missingCombinedHealth / this.body.healthComponent.fullCombinedHealth;
            }
            var newNormalizedChargeRate = 0.0f;
            if (healthCoefficient >= 1.0f)
            {
                newNormalizedChargeRate = -0.5f;
            }
            else if (!this.body.outOfCombat || !this.body.outOfDanger)
            {
                newNormalizedChargeRate = (Mathf.Pow((this.healthCoefficient + this.monsterCountCoefficient), 2) / ((!this.body.outOfDanger) ? 30 : 60));
            }
            else
            {
                newNormalizedChargeRate = (this.energy > 10) ?  (-0.02f * this.energy / 100): 0.02f;
            }
            if (this.normalizedChargeRate != newNormalizedChargeRate)
            {
                this.normalizedChargeRate = newNormalizedChargeRate;
                if (setDirty)
                {
                    this.body.statsDirty = true;
                }
            }            
        }

        private void UpdateSpiralEnergyStatEffects()
        {
            this.body.damage += (this.body.damage * (this.energy / 100));
            this.body.regen += (this.body.regen * (this.energy / 300));
            this.body.moveSpeed += (this.body.moveSpeed * (this.energy / 500));            
        }
    }
}