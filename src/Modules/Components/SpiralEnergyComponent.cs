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
        public float energyModifier = 1.0f;
        private CharacterBody body = null;
        private EntityStateMachine outer = null;
        private TTGLMusicRemote musicRemote = null;
        private float monsterCountCoefficient = 0.0f;
        private float healthCoefficient = 0.0f;
        private bool hadFullBuff;
        private float trottleUpdateTime = 0.0f;
        private float energyUptimeStopwatch = 0.0f;
        private bool musicPlayed = false;
        private float checkGurrenPassiveInterval = 0.5f;
        private float checkGurrenPassiveStopWatch = 0f;

        static SpiralEnergyComponent()
        {
            NetworkBehaviour.RegisterCommandDelegate(typeof(SpiralEnergyComponent), SpiralEnergyComponent.kCmdCmdAddSpiralEnergy, new NetworkBehaviour.CmdDelegate(SpiralEnergyComponent.InvokeCmdCmdAddSpiralEnergy));
            NetworkCRC.RegisterBehaviour("SpiralEnergyComponent", 0);
        }

        public void Awake()
        {
            this.body = base.GetComponent<CharacterBody>();
            this.outer = base.GetComponent<EntityStateMachine>();
            this.musicRemote = base.GetComponent<TTGLMusicRemote>();
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;  
        }

        public void OnDestroy()
        {
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
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


        // Token: 0x060010C9 RID: 4297 RVA: 0x000453D2 File Offset: 0x000435D2
        [Command]
        private void CmdAddSpiralEnergy(float value)
        {
            this.AddSpiralEnergy(value);
        }

        // Token: 0x060010CA RID: 4298 RVA: 0x000453DB File Offset: 0x000435DB
        public void AddSpiralEnergyAuthority(float value)
        {
            if (NetworkServer.active)
            {
                this.AddSpiralEnergy(value);
                return;
            }
            this.CallCmdAddSpiralEnergy(value);
        }

        [Server]
        public void AddSpiralEnergy(float value)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'System.Void SpiralEnergyComponent::AddSpiralEnergy(System.Single)' called on client");
                return;
            }
            if (this.energy < C_SPIRALENERGYCAP)
            {
                this.NetworkEnergy = Mathf.Min(this.energy + value, C_SPIRALENERGYCAP);
            }
        }

        protected static void InvokeCmdCmdAddSpiralEnergy(NetworkBehaviour obj, NetworkReader reader)
        {
            if (!NetworkServer.active)
            {
                Debug.LogError("Command CmdAddSpiralEnergy called on client.");
                return;
            }
            ((SpiralEnergyComponent)obj).CmdAddSpiralEnergy(reader.ReadSingle());
        }

        public void CallCmdAddSpiralEnergy(float value)
        {
            if (!NetworkClient.active)
            {
                Debug.LogError("Command function CmdAddSpiralEnergy called on server.");
                return;
            }
            if (base.isServer)
            {
                this.CmdAddSpiralEnergy(value);
                return;
            }
            NetworkWriter networkWriter = new NetworkWriter();
            networkWriter.Write(0);
            networkWriter.Write((short)((ushort)5));
            networkWriter.WritePackedUInt32((uint)SpiralEnergyComponent.kCmdCmdAddSpiralEnergy);
            networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
            networkWriter.Write(value);
            base.SendCommandInternal(networkWriter, 0, "CmdAddSpiralEnergy");
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
            UpdateGurrenPassive();
            var hasFullEnergyBuff = this.body.HasBuff(Modules.Buffs.maxSpiralPowerBuff);
            var hasFullEnergyDeBuff = this.body.HasBuff(Modules.Buffs.maxSpiralPowerDeBuff);
            var hasKaminaBuff = this.body.HasBuff(Modules.Buffs.kaminaBuff);
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
                    newChargeRate = 0.2f;
                }
                else if (healthCoefficient >= 1.0f)
                {
                    newChargeRate = -0.2f;
                }
                else if (energyUptimeStopwatch < C_MaxEnergyUptime)
                {
                    newChargeRate = ((this.healthCoefficient + this.monsterCountCoefficient) / ((hasFullEnergyDeBuff) ? 120f : 60f)) * this.energyModifier;
                    if (hasKaminaBuff)
                    {
                        newChargeRate *= 2;
                    }
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
            if (this.musicRemote)
            {
                if (this.body.HasBuff(Modules.Buffs.maxSpiralPowerBuff))
                {
                    if (!musicPlayed)
                    {
                        this.musicRemote.PlayMusic(TTGLMusicController.MusicType.FullBuff);
                        musicPlayed = true;
                    }
                }
                else
                {
                    musicPlayed = false;
                }
            }
        }
        private void UpdateGurrenPassive()
        {
            checkGurrenPassiveStopWatch += Time.fixedDeltaTime;
            if (checkGurrenPassiveStopWatch > checkGurrenPassiveInterval)
            {
                checkGurrenPassiveStopWatch = 0f;
                if (this.body)
                {
                    int buffCounts = 0;
                    var gurrenBodyIndex = BodyCatalog.FindBodyIndex("GurrenBody");
                    var allies = TeamComponent.GetTeamMembers(TeamIndex.Player);
                    foreach (var ally in allies)
                    {
                        var allyBody = ally.body;
                        if (allyBody && allyBody != this.body && 
                            allyBody.bodyIndex == gurrenBodyIndex &&
                            (Vector3.Distance(allyBody.transform.position, this.body.transform.position) <= Components.GurrenController.passiveDistance))
                        {
                            buffCounts++;
                        }
                    }
                    this.body.SetBuffCount(Buffs.kaminaBuff.buffIndex, buffCounts);
                }
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

        private static int kCmdCmdAddSpiralEnergy = -1976809258;
    }
}