using UnityEngine;
using EntityStates;
using RoR2;
using TTGL_Survivor.UI;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Runtime.InteropServices;

namespace TTGL_Survivor.Modules
{
    public class SpiralEnergyComponent : NetworkBehaviour, IOnDamageDealtServerReceiver, IOnTakeDamageServerReceiver
    {
        public const float C_SPIRALENERGYCAP = 300.0f;
        public const float C_MaxTrottleUpdateTime = 1.0f;
        public const float C_MaxEnergyUptime = 6.0f;
        private CharacterBody body = null;
        private EntityStateMachine outer = null;
        private float monsterCountCoefficient = 0.0f;
        private float healthCoefficient = 0.0f;
        private bool wasPaused;
        private bool hadFullBuff;
        private float trottleUpdateTime = 0.0f;
        private float energyUptimeStopwatch = 0.0f;

        public void Awake()
        {
            this.body = base.GetComponent<CharacterBody>();
            this.outer = base.GetComponent<EntityStateMachine>();            
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;            
            MusicController.pickTrackHook += MusicController_pickTrackHook;
        }

        public void OnDestroy()
        {
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
            MusicController.pickTrackHook -= MusicController_pickTrackHook;
            if (this.playedMusic)
            {
                AkSoundEngine.PostEvent("TTGLFullBuffStop", this.body.gameObject);
            }
        }
        private void LateUpdate()
        {
            if (this.playedMusic)
            {
                bool flag = Time.timeScale == 0f;
                if (this.wasPaused != flag)
                {
                    AkSoundEngine.PostEvent(flag ? "TTGLFullBuffPause" : "TTGLFullBuffResume", base.gameObject);
                    this.wasPaused = flag;
                }
            }            
        }

        private void FixedUpdate()
        {            
            if (NetworkServer.active)
            {
                this.ServerFixedUpdate();
            }
            if (isAuthority)
            {
                this.AuthorityFixedUpdate();
            }            
        }

        private void MusicController_pickTrackHook(MusicController musicController, ref MusicTrackDef newTrack)
        {
            if (this.playedMusic)
            {
                newTrack = null;
            }
        }

        public void OnDamageDealtServer(DamageReport damageReport)
        {
            this.energyUptimeStopwatch = 0.0f;
        }

        public void OnTakeDamageServer(DamageReport damageReport)
        {
            this.energyUptimeStopwatch = 0.0f;
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            // a simple stat hook, adds armor after stats are recalculated
            if (self && self == this.body)
            {
                this.body.damage += (this.body.damage * (this.energy / 100));
                this.body.regen += (this.body.regen * (this.energy / 300));
                this.body.moveSpeed += (this.body.moveSpeed * (this.energy / 500));
                if (this.body.HasBuff(Modules.Buffs.maxSpiralPowerBuff))
                {
                    this.body.armor += 300f;
                }
            }
        }
        
        private void ServerFixedUpdate()
        {
            var hasFullEnergyBuff = this.body.HasBuff(Modules.Buffs.maxSpiralPowerBuff);
            var hasFullEnergyDeBuff = this.body.HasBuff(Modules.Buffs.maxSpiralPowerDeBuff);
            if (energyUptimeStopwatch < C_MaxEnergyUptime)
            {
                energyUptimeStopwatch += Time.fixedDeltaTime;
            }
            if (trottleUpdateTime < C_MaxTrottleUpdateTime)
            {
                trottleUpdateTime += Time.fixedDeltaTime;
            }
            if (trottleUpdateTime >= C_MaxTrottleUpdateTime)
            {
                trottleUpdateTime = 0f;
                var monsterCount = TeamComponent.GetTeamMembers(TeamIndex.Monster).Count;
                this.monsterCountCoefficient = ((float)monsterCount / 40.0f);
                if (this.body.healthComponent.fullCombinedHealth != 0)
                {
                    this.healthCoefficient = this.body.healthComponent.missingCombinedHealth / this.body.healthComponent.fullCombinedHealth;
                }
                var newChargeRate = 0.0f;
                if (this.body.HasBuff(Modules.Buffs.maxSpiralPowerBuff))
                {
                    newChargeRate = 0.5f;
                }
                else if (healthCoefficient >= 1.0f)
                {
                    newChargeRate = -0.5f;
                }
                else if (energyUptimeStopwatch < C_MaxEnergyUptime)
                {
                    newChargeRate = (Mathf.Pow((this.healthCoefficient + this.monsterCountCoefficient), 2f) / ((hasFullEnergyDeBuff) ? 60f : 30f));
                }
                else
                {
                    newChargeRate = (this.energy > (5.0f + this.body.level))? (hasFullEnergyDeBuff) ? -0.03f : -0.01f : 0.02f;
                }
                if (this.charge_rate != newChargeRate)
                {
                    this.NetworkChargeRate = newChargeRate;
                    this.body.statsDirty = true;
                }
            }
            this.NetworkEnergy = Mathf.Clamp(this.energy + this.charge_rate * Time.fixedDeltaTime * C_SPIRALENERGYCAP, 0.0f, C_SPIRALENERGYCAP);
                        
            if (!this.hadFullBuff && !hasFullEnergyDeBuff && this.energy >= C_SPIRALENERGYCAP)
            {
                this.body.AddTimedBuff(Modules.Buffs.maxSpiralPowerBuff, 100f);
                this.hadFullBuff = true;
            }
            else if (this.hadFullBuff && !hasFullEnergyBuff)
            {
                this.body.AddTimedBuff(Modules.Buffs.maxSpiralPowerDeBuff, 120f);
                this.hadFullBuff = false;
            }
        }

        private void AuthorityFixedUpdate()
        {
            if (this.body.HasBuff(Modules.Buffs.maxSpiralPowerBuff))
            {
                if (!playedMusic)
                {
                    AkSoundEngine.PostEvent("TTGLFullBuffPlay", this.body.gameObject);
                    playedMusic = true;
                }
            }
            else if (playedMusic)
            {
                playedMusic = false;
            }
        }

        protected bool isAuthority
        {
            get
            {
                return Util.HasEffectiveAuthority(this.outer.networkIdentity);
            }
        }

        public float NetworkEnergy
        {
            get
            {
                return this.energy;
            }
            [param: In]
            set
            {
                base.SetSyncVar<float>(value, ref this.energy, 1U);
            }
        }
        public float NetworkChargeRate
        {
            get
            {
                return this.charge_rate;
            }
            [param: In]
            set
            {
                base.SetSyncVar<float>(value, ref this.charge_rate, 2U);
            }
        }

        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
                writer.Write(this.energy);
                writer.Write(this.charge_rate);
                return true;
            }
            bool flag = false;
            if ((base.syncVarDirtyBits & 1U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(base.syncVarDirtyBits);
                    flag = true;
                }
                writer.Write(this.energy);
            }
            if ((base.syncVarDirtyBits & 2U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(base.syncVarDirtyBits);
                    flag = true;
                }
                writer.Write(this.charge_rate);
            }
            if (!flag)
            {
                writer.WritePackedUInt32(base.syncVarDirtyBits);
            }
            return flag;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                this.energy = reader.ReadSingle();
                this.charge_rate = reader.ReadSingle();
                return;
            }
            int num = (int)reader.ReadPackedUInt32();
            if ((num & 1) != 0)
            {
                this.energy = reader.ReadSingle();
            }
            if ((num & 2) != 0)
            {
                this.charge_rate = reader.ReadSingle();
            }
        }

        [HideInInspector]
        [SyncVar]
        [Tooltip("How much energy this object has.")]
        public float energy = 0f;

        [Tooltip("How much charge rate this object has.")]
        [HideInInspector]
        [SyncVar]
        public float charge_rate = 0f;
        
        private bool playedMusic;
    }
}