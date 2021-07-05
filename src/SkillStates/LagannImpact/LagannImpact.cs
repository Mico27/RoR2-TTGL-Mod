using EntityStates;
using EntityStates.Merc;
using RoR2;
using RoR2.Projectile;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.RoR2Content;

namespace TTGL_Survivor.SkillStates
{
    
    public class LagannImpact : BaseState
    {
        
        public static float damageCoefficient = 15.0f;
        public const float c_SpeedCoefficient = 64.0f;
        public const float c_BouncingMaxTime = 0.5f;
        public const string c_HitboxGroupName = "LagannImpactHitbox";

        public Tuple<Vector3, Vector3>[] TrajectoryNodes { get; set; }
        public int CurrentNodeIndex;
        private int m_TrajectoryNodeCount; 
        private OverlapAttack overlapAttack;
        protected float pushForce = 300f;
        protected float procCoefficient = 2f;
        private bool cancelled;
        private Transform rootTransform;

        public override void OnEnter()
        {
            base.OnEnter();
            if (TTGL_SurvivorPlugin.rideMeExtendedInstalled)
            {
                TTGL_SurvivorPlugin.ExitSeat(base.gameObject);
            }
            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(Buffs.HiddenInvincibility);
            }
            if (base.isAuthority)
            {
                m_TrajectoryNodeCount = TrajectoryNodes != null ? TrajectoryNodes.Length : 0;
                if (m_TrajectoryNodeCount > CurrentNodeIndex)
                {
                    var nextNode = TrajectoryNodes[CurrentNodeIndex];                    
                    m_CurrentTarget = nextNode.Item1;
                }
            }
            if (m_CurrentTarget != null)
            {
                TTGL_SurvivorPlugin.instance.Logger.LogMessage("LagannImpact.m_CurrentTarget is not null");
            }
            this.cancelled = true;
            Util.PlaySound(EvisDash.beginSoundString, base.gameObject);

            base.characterMotor.useGravity = false;
            var childLocator = base.GetModelChildLocator();
            this.rootTransform = childLocator.FindChild("LagganArmature");
            base.PlayCrossfade("FullBody, Override", "LagannImpact3", 0.2f);           
            
            this.CreateBlinkEffect(Util.GetCorePosition(base.gameObject));

            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == c_HitboxGroupName);
            }

            this.overlapAttack = new OverlapAttack
            {
                attacker = base.gameObject,
                damage = damageCoefficient * base.characterBody.damage,
                pushAwayForce = this.pushForce,
                isCrit = base.RollCrit(),
                damageColorIndex = DamageColorIndex.Default,
                inflictor = base.gameObject,
                procChainMask = default(ProcChainMask),
                procCoefficient = this.procCoefficient,
                teamIndex = base.characterBody.teamComponent.teamIndex,
                hitBoxGroup = hitBoxGroup,
                hitEffectPrefab = Modules.Assets.punchImpactEffect,
                impactSound = Modules.Assets.drillRushHitSoundEvent.index,
            };
        }

        private void CreateBlinkEffect(Vector3 origin)
        {
            if (m_CurrentTarget != null)
            {
                EffectData effectData = new EffectData();
                effectData.rotation = Util.QuaternionSafeLookRotation((m_CurrentTarget - base.characterBody.corePosition).normalized);
                effectData.origin = origin;
                EffectManager.SpawnEffect(EvisDash.blinkPrefab, effectData, false);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority)
            {
                if (m_CurrentTarget == null || ((base.skillLocator && base.skillLocator.utility.IsReady() && base.inputBank.skill3.justPressed) || base.inputBank.interact.justPressed || base.inputBank.jump.justPressed))
                {
                    this.outer.SetNextStateToMain();
                    return;
                }
            }
            if (m_CurrentTarget != null)
            {
                if (base.characterMotor)
                {
                    var currentDirection = (m_CurrentTarget - base.characterBody.corePosition).normalized;
                    var newRotation = Util.QuaternionSafeLookRotation(currentDirection) * Quaternion.Euler(new Vector3(-90, 0, 0));
                    this.rootTransform.rotation = newRotation;
                    if (Vector3.Distance(base.characterBody.corePosition, m_CurrentTarget) <= base.characterBody.radius + 2f)
                    {
                        if (base.isAuthority)
                        {
                            if (m_TrajectoryNodeCount > CurrentNodeIndex + 1)
                            {
                                this.cancelled = false;
                                this.outer.SetNextState(new ReboundLagannImpact() { TrajectoryNodes = this.TrajectoryNodes, CurrentNodeIndex = this.CurrentNodeIndex });
                            }
                            else
                            {
                                this.outer.SetNextStateToMain();
                                return;
                            }
                        }
                    }
                    else
                    {
                        base.characterMotor.rootMotion += currentDirection * (c_SpeedCoefficient * Time.fixedDeltaTime);
                    }
                }
            }
            if (base.isAuthority)
            {
                this.overlapAttack.Fire();
            }
        }

        public override void OnExit()
        {
            if (cancelled)
            {
                base.characterMotor.useGravity = true;
                this.rootTransform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0));
                if (m_CurrentTarget != null)
                {
                    base.characterDirection.forward = (m_CurrentTarget - base.characterBody.corePosition).normalized;
                }
                base.PlayCrossfade("FullBody, Override", "LagannImpactExit", 0.2f);
                Util.PlaySound(EvisDash.endSoundString, base.gameObject);
            }
            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(Buffs.HiddenInvincibility);
            }
            base.OnExit();
        }
        
        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(m_CurrentTarget);
            writer.Write(cancelled);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            m_CurrentTarget = reader.ReadVector3();
            cancelled = reader.ReadBoolean();
        }
        
        private Vector3 m_CurrentTarget;
    }
}