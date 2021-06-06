using EntityStates;
using EntityStates.Merc;
using RoR2;
using RoR2.Projectile;
using System;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.RoR2Content;

namespace TTGL_Survivor.SkillStates
{
    
    public class LagannImpact : BaseState
    {
        
        public static float damageCoefficient = 15.0f;
        public const float c_SpeedCoefficient = 8.0f;
        public const float c_BouncingMaxTime = 0.5f;
        public const string c_HitboxGroupName = "LagannImpactHitbox";

        public Tuple<Vector3, Vector3>[] TrajectoryNodes { get; set; }
        private int m_TrajectoryNodeCount;
        private int m_CurrentNodeIndex;
        private Vector3 m_CurrentDirection;
        private bool m_IsBouncing;
        private float m_BouncingTime;
        private OverlapAttack overlapAttack;
        protected float pushForce = 300f;
        protected float procCoefficient = 2f;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            m_BouncingTime = 0.0f;
            m_IsBouncing = false;
            m_CurrentNodeIndex = 1;
            m_TrajectoryNodeCount = TrajectoryNodes != null ? TrajectoryNodes.Length : 0;
            this.animator = base.GetModelAnimator();
            this.animator.SetInteger("LagannImpact.stage", 2);
            Util.PlaySound(EvisDash.beginSoundString, base.gameObject);
            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(Buffs.HiddenInvincibility);
            }
            base.PlayAnimation("FullBody, Override", "LagannImpact2");            
            base.characterDirection.enabled = false;
            base.characterMotor.useGravity = false;
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
            EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(m_CurrentDirection);
            effectData.origin = origin;
            EffectManager.SpawnEffect(EvisDash.blinkPrefab, effectData, false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority)
            {
                if (!(m_TrajectoryNodeCount > m_CurrentNodeIndex) || ((base.skillLocator && base.skillLocator.utility.IsReady() && base.inputBank.skill3.justPressed) || base.inputBank.interact.justPressed || base.inputBank.jump.justPressed))
                {
                    this.outer.SetNextStateToMain();
                    return;
                }
            }
            if (m_IsBouncing)
            {
                if (m_BouncingTime >= c_BouncingMaxTime)
                {
                    this.animator.SetInteger("LagannImpact.stage", 2);
                    Util.PlaySound(EvisDash.beginSoundString, base.gameObject);
                   // Util.PlaySound("TTGLLagannImpactFire", base.gameObject);
                    this.CreateBlinkEffect(base.characterBody.corePosition);
                    m_IsBouncing = false;
                    this.overlapAttack.ResetIgnoredHealthComponents();
                    return;
                }
                m_BouncingTime += Time.fixedDeltaTime;
            }
            else
            {
                if (m_TrajectoryNodeCount > m_CurrentNodeIndex)
                {
                    var nextNode = TrajectoryNodes[m_CurrentNodeIndex];
                    m_CurrentDirection = (nextNode.Item1 - base.characterBody.corePosition).normalized;
                    if (base.characterMotor && base.characterDirection)
                    {
                        base.characterDirection.targetTransform.rotation = Util.QuaternionSafeLookRotation(m_CurrentDirection);
                        if (Vector3.Distance(base.characterBody.corePosition, nextNode.Item1) <= base.characterBody.radius + 2f)
                        {
                            if (m_TrajectoryNodeCount > m_CurrentNodeIndex + 1)
                            {
                                base.characterDirection.targetTransform.rotation = Util.QuaternionSafeLookRotation(-nextNode.Item2);
                                base.characterMotor.velocity = Vector3.zero;
                                Util.PlaySound(EvisDash.endSoundString, base.gameObject);
                                this.CreateBlinkEffect(base.characterBody.corePosition);
                                this.animator.SetInteger("LagannImpact.stage", 3);
                                //base.PlayAnimation("FullBody, Override", "LagannImpact3", "LagannImpact.playbackRate", c_BouncingMaxTime);
                                m_BouncingTime = 0.0f;
                                m_IsBouncing = true;
                            }
                            else
                            {
                                base.characterMotor.velocity *= 0.5f;
                            }
                            m_CurrentNodeIndex++;
                        }
                        else
                        {
                            base.characterMotor.rootMotion += m_CurrentDirection * (base.moveSpeedStat * c_SpeedCoefficient * Time.fixedDeltaTime);
                        }
                    }                    
                }
                if (base.isAuthority)
                {
                    this.overlapAttack.Fire();
                }
            }
        }

        public override void OnExit()
        {
            base.characterMotor.useGravity = true;
            base.characterDirection.enabled = true;
            base.characterDirection.forward = m_CurrentDirection;
            this.animator.SetInteger("LagannImpact.stage", 0);
            Util.PlaySound(EvisDash.endSoundString, base.gameObject);
            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(Buffs.HiddenInvincibility);
            }
            base.OnExit();
        }
                
    }
}