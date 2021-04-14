using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TTGL_Survivor.Modules.Components;
using TTGL_Survivor.Modules;
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
            this.hitBoxGroup = base.GetComponent<HitBoxGroup>();
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
            if (!this.projectileController.owner)
            {
                if (NetworkServer.active)
                {
                    UnityEngine.Object.Destroy(base.gameObject);
                }
                return;
            }
            switch (this.boomerangState)
            {
                case 0:
                    this.rigidbody.velocity = this.travelSpeed * base.transform.forward;
                    this.stopwatch += Time.fixedDeltaTime;
                    if (this.stopwatch >= this.maxFlyStopwatch)
                    {
                        this.stopwatch = 0f;
                        this.NetworkboomerangState = 1;
                        return;
                    }
                    break;
                case 1:
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
                case 2:
                    this.stopwatch += Time.fixedDeltaTime;
                    bool flag = this.Reel();
                    this.canHitWorld = false;
                    Vector3 a2 = this.CalculatePullDirection();
                    this.rigidbody.velocity = this.travelSpeed * a2;
                    if (flag || (this.stopwatch >= this.maxFlyBackStopwatch))
                    {
                        if (NetworkServer.active)
                        {
                            UnityEngine.Object.Destroy(base.gameObject);
                        }
                    }
                    break;
                default:
                    return;
            }
            if (NetworkServer.active)
            {
                StunLockBoss();
            }
        }

        private void StunLockBoss()
        {
            if (!hasHitBoss)
            {
                HitBox[] hitBoxes = this.hitBoxGroup.hitBoxes;
                foreach (HitBox hitBox in hitBoxes)
                {
                    Transform transform = hitBox.transform;
                    Vector3 position = transform.position;
                    Vector3 vector = transform.lossyScale * 0.5f;
                    Quaternion rotation = transform.rotation;
                    Collider[] overlapColliders = Physics.OverlapBox(position, vector, rotation, LayerIndex.entityPrecise.mask);
                    foreach (Collider collider in overlapColliders)
                    {
                        HurtBox component = collider.GetComponent<HurtBox>();
                        if (component && component.healthComponent &&
                            component.healthComponent.body &&
                            component.healthComponent.body.isBoss)
                        {
                            hasHitBoss = true;                           
                            this.ConstrictBoss(component.healthComponent.body);
                            UnityEngine.Object.Destroy(base.gameObject);
                            break;
                        }
                    }
                    if (hasHitBoss)
                    {
                        break;
                    }
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

        private void ConstrictBoss(CharacterBody bossBody)
        {
            var existingConstrict = bossBody.GetComponentInChildren<GurrenLagannShadesConstrictComponent>();
            if (!existingConstrict || existingConstrict.visualState == 1)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(TemporaryVisualEffects.gurrenLagannShadesBindingEffect, bossBody.corePosition, Quaternion.identity);
                var component = gameObject.GetComponent<GurrenLagannShadesConstrictComponent>();
                component.duration = 8f;
                component.parentTransform = bossBody.transform;
                gameObject.transform.SetParent(bossBody.transform);
            }
            else
            {
                existingConstrict.durationCounter = 0f;
            }            
        }

        private void UNetVersion()
        {
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

        private HitBoxGroup hitBoxGroup;

        private float stopwatch;

        private bool hasHitBoss = false;
                
    }
}
