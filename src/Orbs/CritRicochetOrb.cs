using RoR2;
using RoR2.Orbs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TTGL_Survivor.Orbs
{
    public class CritRicochetOrb : Orb
    {
        public override void OnArrival()
        {
            base.OnArrival();
            if (this.target)
            {
                if (!string.IsNullOrEmpty(this.hitSoundString))
                {
                    Util.PlaySound(this.hitSoundString, this.target.gameObject);
                }
                if (this.tracerEffectPrefab)
                {
                    EffectData effectData = new EffectData
                    {
                        origin = this.target.transform.position,
                        start = this.origin,
                    };
                    EffectManager.SpawnEffect(this.tracerEffectPrefab, effectData, true);
                }
                if (this.hitEffectPrefab)
                {
                    EffectManager.SimpleImpactEffect(this.hitEffectPrefab, this.target.transform.position, Vector3.zero, true);
                }
                if (this.damageValue > 0f)
                {
                    HealthComponent healthComponent = this.target.healthComponent;
                    if (healthComponent)
                    {
                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.damage = this.damageValue;
                        damageInfo.attacker = this.attacker;
                        damageInfo.inflictor = this.inflictor;
                        damageInfo.force = Vector3.zero;
                        damageInfo.crit = this.isCrit;
                        damageInfo.procChainMask = this.procChainMask;
                        damageInfo.procCoefficient = this.procCoefficient;
                        damageInfo.position = this.target.transform.position;
                        damageInfo.damageColorIndex = this.damageColorIndex;
                        damageInfo.damageType = this.damageType;
                        healthComponent.TakeDamage(damageInfo);
                        GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                        GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
                    }
                }
                this.hitCallback?.Invoke(this);
                if (this.bouncesRemaining > 0 && this.isCrit)
                {
                    if (resetBouncedObjects)
                    {
                        this.bouncedObjects.Clear();
                        this.bouncedObjects.Add(this.target.healthComponent);
                    }
                    HurtBox hurtBox = this.PickNextTarget(this.target.transform.position);
                    if (hurtBox)
                    {                        
                        CritRicochetOrb CritRicochetOrb = new CritRicochetOrb();
                        CritRicochetOrb.search = this.search;
                        CritRicochetOrb.origin = this.target.transform.position;
                        CritRicochetOrb.target = hurtBox;
                        CritRicochetOrb.attacker = this.attacker;
                        CritRicochetOrb.attackerBody = this.attackerBody;
                        CritRicochetOrb.inflictor = this.inflictor;
                        CritRicochetOrb.teamIndex = this.teamIndex;
                        CritRicochetOrb.damageValue = this.damageValue;
                        CritRicochetOrb.isCrit = this.attackerBody.RollCrit();
                        CritRicochetOrb.bouncesRemaining = this.bouncesRemaining - 1;
                        CritRicochetOrb.bouncedObjects = this.bouncedObjects;
                        CritRicochetOrb.resetBouncedObjects = this.resetBouncedObjects;
                        CritRicochetOrb.procChainMask = this.procChainMask;
                        CritRicochetOrb.procCoefficient = this.procCoefficient;
                        CritRicochetOrb.damageColorIndex = this.damageColorIndex;
                        CritRicochetOrb.duration = this.duration;                        
                        CritRicochetOrb.range = this.range;
                        CritRicochetOrb.damageType = this.damageType;
                        CritRicochetOrb.tracerEffectPrefab = this.tracerEffectPrefab;
                        CritRicochetOrb.hitEffectPrefab = this.hitEffectPrefab;
                        CritRicochetOrb.hitSoundString = this.hitSoundString;
                        CritRicochetOrb.hitCallback = this.hitCallback;
                        OrbManager.instance.AddOrb(CritRicochetOrb);
                        return;
                    }
                }
            }
            this.bouncedObjects.Clear();
        }

        public HurtBox PickNextTarget(Vector3 position)
        {
            if (this.search == null)
            {
                this.search = new BullseyeSearch();
            }
            this.search.searchOrigin = position;
            this.search.searchDirection = Vector3.zero;
            this.search.teamMaskFilter = TeamMask.allButNeutral;
            this.search.teamMaskFilter.RemoveTeam(this.teamIndex);
            this.search.filterByLoS = false;
            this.search.sortMode = BullseyeSearch.SortMode.Distance;
            this.search.maxDistanceFilter = this.range;
            this.search.RefreshCandidates();
            HurtBox hurtBox = (from v in this.search.GetResults()
                               where !this.bouncedObjects.Contains(v.healthComponent)
                               select v).FirstOrDefault<HurtBox>();
            if (hurtBox)
            {
                this.bouncedObjects.Add(hurtBox.healthComponent);
            }
            return hurtBox;
        }
        
        public int bouncesRemaining;

        public bool resetBouncedObjects;

        public float damageValue;

        public GameObject attacker;

        public CharacterBody attackerBody;

        public GameObject inflictor;
        
        public List<HealthComponent> bouncedObjects;

        public TeamIndex teamIndex;

        public bool isCrit;

        public ProcChainMask procChainMask;

        public float procCoefficient = 1f;

        public DamageColorIndex damageColorIndex;

        public float range = 20f;
                
        public DamageType damageType;
                
        private BullseyeSearch search;

        public GameObject hitEffectPrefab;

        public GameObject tracerEffectPrefab;

        public string hitSoundString;

        public delegate void HitCallback(CritRicochetOrb orb);

        public CritRicochetOrb.HitCallback hitCallback;

    }
}
