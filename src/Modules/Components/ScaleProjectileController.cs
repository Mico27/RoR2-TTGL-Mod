using System;
using System.Runtime.InteropServices;
using RoR2;
using RoR2.Projectile;
using TTGL_Survivor.Modules.Survivors;
using TTGL_Survivor.SkillStates;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.Modules.Components
{
    public class ScaleProjectileController : NetworkBehaviour
    {
        public ProjectileController projectileController { get; set; }

        private void Start()
        {
            this.projectileController = base.GetComponent<ProjectileController>();
        }

        private void Update()
        {
            UpdateTransform();
        }
        
        private void UpdateTransform()
        {
            var newScale = Vector3.one * NetworkChargeRate;
            if (base.transform.localScale != newScale && (this.projectileController && this.projectileController.ghost))
            {
                base.transform.localScale = newScale;
                this.projectileController.ghost.transform.localScale = newScale;
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
                base.SetSyncVar<float>(value, ref this.charge_rate, 1U);
            }
        }

        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
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
                this.charge_rate = reader.ReadSingle();
                return;
            }
            int num = (int)reader.ReadPackedUInt32();
            if ((num & 1) != 0)
            {
                this.charge_rate = reader.ReadSingle();
            }
        }

        [Tooltip("How much charge rate this object has.")]
        [HideInInspector]
        [SyncVar]
        public float charge_rate = 1f;

    }
}