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
    
    public class ReboundLagannImpact : BaseState
    {
        public const float c_BouncingMaxTime = 0.5f;

        public Tuple<Vector3, Vector3>[] TrajectoryNodes { get; set; }
        public int CurrentNodeIndex; 
        private float m_BouncingTime;
        private bool cancelled;
        private Transform rootTransform;
        

        public override void OnEnter()
        {
            base.OnEnter();
            this.cancelled = true;
            m_BouncingTime = 0.0f;
            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(Buffs.HiddenInvincibility);
            }
            if (base.isAuthority)
            {
                var nextNode = TrajectoryNodes[CurrentNodeIndex];
                currentDirection = -nextNode.Item2;
            }
            base.characterMotor.useGravity = false;
            var childLocator = base.GetModelChildLocator();
            this.rootTransform = childLocator.FindChild("LagganArmature");

            var newRotation = Util.QuaternionSafeLookRotation(currentDirection) * Quaternion.Euler(new Vector3(-90, 0, 0));
            this.rootTransform.rotation = newRotation;
            base.characterMotor.velocity = Vector3.zero;
            base.characterMotor.Motor.ForceUnground();
            Util.PlaySound(EvisDash.endSoundString, base.gameObject);
            this.CreateBlinkEffect(base.characterBody.corePosition);

            
            base.PlayCrossfade("FullBody, Override", "LagannImpact4", 0.2f);
            m_BouncingTime = 0.0f;
        }

        private void CreateBlinkEffect(Vector3 origin)
        {
            EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(currentDirection);
            effectData.origin = origin;
            EffectManager.SpawnEffect(EvisDash.blinkPrefab, effectData, false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority)
            {
                if (((base.skillLocator && base.skillLocator.utility.IsReady() && base.inputBank.skill3.justPressed) || base.inputBank.interact.justPressed || base.inputBank.jump.justPressed))
                {
                    this.outer.SetNextStateToMain();
                    return;
                }
            }
            if (m_BouncingTime >= c_BouncingMaxTime)
            {                
                if (base.isAuthority)
                {
                    this.cancelled = false;
                    base.characterMotor.Motor.ForceUnground();
                    this.outer.SetNextState(new LagannImpact() { TrajectoryNodes = this.TrajectoryNodes, CurrentNodeIndex = this.CurrentNodeIndex + 1 });
                }
                return;
            }
            m_BouncingTime += Time.fixedDeltaTime;
        }

        public override void OnExit()
        {          
            if (this.cancelled)
            {
                base.characterMotor.useGravity = true;
                this.rootTransform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0));
                base.characterDirection.forward = currentDirection;
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
            writer.Write(currentDirection);
            writer.Write(cancelled);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            currentDirection = reader.ReadVector3();
            cancelled = reader.ReadBoolean();
        }

        private Vector3 currentDirection;
    }
}