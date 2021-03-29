using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2.Projectile
{
    [RequireComponent(typeof(ProjectileController))]
    public class GurrenLagannShadesProjectile : NetworkBehaviour, IProjectileImpactBehavior
    {
        private void Start()
        {
            this.rigidbody = base.GetComponent<Rigidbody>();
            this.projectileController = base.GetComponent<ProjectileController>();
            if (this.projectileController && this.projectileController.owner)
            {
                this.ownerTransform = this.projectileController.owner.transform;
                var stateMachine = this.ownerTransform.GetComponent<EntityStateMachine>();
                if (stateMachine)
                {
                    var childLocator = stateMachine.commonComponents.modelLocator.modelTransform.GetComponentInChildren<ChildLocator>();
                    if (childLocator)
                    {
                        this.returnTarget = childLocator.FindChild("HurtboxChest");
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (this.ownerTransform)
            {
                var stateMachine = this.ownerTransform.GetComponent<EntityStateMachine>();
                if (stateMachine)
                {                    
                    var skillLocator = stateMachine.commonComponents.skillLocator;
                    if (skillLocator)
                    {
                        skillLocator.secondary.AddOneStock();
                    }
                }
            }
        }

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            if (!this.canHitWorld)
            {
                return;
            }
            if (this.NetworkboomerangState != 2)
            {
                this.stopwatch = 0f;
                this.NetworkboomerangState = 2;
            }
            EffectManager.SimpleImpactEffect(this.impactSpark, impactInfo.estimatedPointOfImpact, -base.transform.forward, true);
        }

        private bool Reel()
        {
            if (this.returnTarget)
            {
                Vector3 vector = this.returnTarget.position - base.transform.position;
                return vector.magnitude <= 2f;
            }
            else if (this.projectileController.owner)
            {
                Vector3 vector = this.projectileController.owner.transform.position - base.transform.position;
                return vector.magnitude <= 2f;
            }
            return true;            
        }

        public void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                if (!this.projectileController.owner)
                {
                    UnityEngine.Object.Destroy(base.gameObject);
                    return;
                }
                switch (this.boomerangState)
                {
                    case 0:
                        if (NetworkServer.active)
                        {
                            this.rigidbody.velocity = this.travelSpeed * base.transform.forward;
                            this.stopwatch += Time.fixedDeltaTime;
                            if (this.stopwatch >= this.maxFlyStopwatch)
                            {
                                this.stopwatch = 0f;
                                this.NetworkboomerangState = 1;
                                return;
                            }
                        }
                        break;
                    case 1:
                        {
                            this.stopwatch += Time.fixedDeltaTime;
                            float num = this.stopwatch / this.transitionDuration;
                            Vector3 a = this.CalculatePullDirection();
                            this.rigidbody.velocity = Vector3.Lerp(this.travelSpeed * base.transform.forward, this.travelSpeed * a, num);
                            if (num >= 1f)
                            {
                                this.stopwatch = 0f;
                                this.NetworkboomerangState = 2;
                                return;
                            }
                            break;
                        }
                    case 2:
                        {
                            this.stopwatch += Time.fixedDeltaTime;
                            bool flag = this.Reel();
                            if (NetworkServer.active)
                            {
                                this.canHitWorld = false;
                                Vector3 a2 = this.CalculatePullDirection();
                                this.rigidbody.velocity = this.travelSpeed * a2;
                                if (flag || (this.stopwatch >= this.maxFlyBackStopwatch))
                                {
                                    UnityEngine.Object.Destroy(base.gameObject);
                                }
                            }
                            break;
                        }
                    default:
                        return;
                }
            }
        }

        private Vector3 CalculatePullDirection()
        {
            if (this.returnTarget)
            {
                return (this.returnTarget.position - base.transform.position).normalized;
            }
            else if (this.projectileController.owner)
            {
                return (this.projectileController.owner.transform.position - base.transform.position).normalized;
            }
            return base.transform.forward;
        }

        public int NetworkboomerangState
        {
            get
            {
                return this.boomerangState;
            }
            [param: In]
            set
            {
                base.SetSyncVar<int>(value, ref this.boomerangState, 1U);
            }
        }

        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
                writer.Write(this.boomerangState);
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
                writer.Write(this.boomerangState);
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
                this.boomerangState = reader.ReadInt32();
                return;
            }
            int num = (int)reader.ReadPackedUInt32();
            if ((num & 1) != 0)
            {
                this.boomerangState = reader.ReadInt32();
            }
        }
        
        public float travelSpeed = 40f;
        
        public float transitionDuration = 1f;

        private float maxFlyStopwatch = 2f;

        public float maxFlyBackStopwatch = 2f;

        public GameObject impactSpark;

        public GameObject crosshairPrefab;
        
        public bool canHitWorld;

        private ProjectileController projectileController;

        [SyncVar]
        public int boomerangState;

        private Transform ownerTransform;

        private Transform returnTarget;
        
        private Rigidbody rigidbody;

        private float stopwatch;
                
    }
}
