using EntityStates;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class AimLagannImpact : BaseState
    {
        public const float c_MaxDuration = 3.0f;
        public static float maxStepDistanceMultiplier = 10.0f;
        public static int maxRebound = 4;
        private LineRenderer m_LineRenderer;
        private Tuple<Vector3, Vector3>[] m_TrajectoryNodes;
        private Animator animator;
        private bool cancelled;
        private float currentMaxStepDistance;
        private Transform rootTransform;

        public override void OnEnter()
        {
            base.OnEnter();
            currentMaxStepDistance = maxStepDistanceMultiplier * base.moveSpeedStat;
            this.animator = base.GetModelAnimator();
            cancelled = true;
            base.characterBody.SetAimTimer(c_MaxDuration);
            this.m_LineRenderer = base.gameObject.AddComponent<LineRenderer>();
            this.m_LineRenderer.startWidth = 0.5f;
            this.m_LineRenderer.endWidth = 0.5f;
            this.m_LineRenderer.material = Modules.Assets.LoadAsset<Material>("SparksMat");
            this.m_LineRenderer.startColor = Color.green;
            this.m_LineRenderer.endColor = Color.white;
            this.m_LineRenderer.textureMode = LineTextureMode.Tile;
            
            if (base.cameraTargetParams)
            {
                base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Standard);
            }
            base.characterMotor.useGravity = false;
            var childLocator = base.GetModelChildLocator();
            this.rootTransform = childLocator.FindChild("LagganArmature");
            
            base.PlayCrossfade("FullBody, Override", "LagannImpact2", 0.2f);
        }

        public override void OnExit()
        {
            if (base.cameraTargetParams)
            {
                base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Standard);
            }
            if (this.cancelled)
            {
                base.characterMotor.useGravity = true;
                this.rootTransform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0));
                base.PlayCrossfade("FullBody, Override", "LagannImpactExit", 0.5f);
            }
            Destroy(this.m_LineRenderer);
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.inputBank)
            {                
                if (base.characterMotor)
                {
                    base.characterMotor.velocity = Vector3.zero;                    
                }
                var newRotation = Util.QuaternionSafeLookRotation(base.inputBank.aimDirection) * Quaternion.Euler(new Vector3(-90, 0, 0));
                this.rootTransform.rotation = newRotation;
                this.UpdateTrajectoryNodes(this.transform.position, base.inputBank.aimDirection, maxRebound);
                this.DrawTrajectoryLine();
                if (base.isAuthority)
                {                    
                    if ((base.skillLocator && base.skillLocator.utility.IsReady() && base.inputBank.skill3.justPressed) || base.inputBank.interact.justPressed || base.inputBank.jump.justPressed)
                    {
                        this.outer.SetNextStateToMain();
                        return;
                    }
                    if (base.fixedAge >= c_MaxDuration || base.inputBank.skill1.justPressed || base.inputBank.skill2.justPressed || base.inputBank.skill4.justPressed)
                    {
                        this.cancelled = false;
                        this.outer.SetNextState(new LagannImpact() { TrajectoryNodes = this.m_TrajectoryNodes, CurrentNodeIndex = 1 });
                    }                    
                }
            }
        }

        private void DrawTrajectoryLine()
        {
            this.m_LineRenderer.positionCount = m_TrajectoryNodes.Length;
            this.m_LineRenderer.SetPositions(m_TrajectoryNodes.Select(x=>x.Item1).ToArray());
        }

        private void UpdateTrajectoryNodes(Vector3 position, Vector3 direction, int reflectionsRemaining)
        {
            List<Tuple<Vector3, Vector3>> trajectoryNodes = new List<Tuple<Vector3, Vector3>>();
            trajectoryNodes.Add(new Tuple<Vector3, Vector3>(position, Vector3.zero));
            this.GetTrajectoryNodes(trajectoryNodes, position, direction, reflectionsRemaining);
            m_TrajectoryNodes = trajectoryNodes.ToArray();
        }

        private void GetTrajectoryNodes(List<Tuple<Vector3, Vector3>> nodes, Vector3 position, Vector3 direction, int reflectionsRemaining)
        {            
            if (reflectionsRemaining <= 0)
            {
                return;
            }
            Ray ray = new Ray(position, direction);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, currentMaxStepDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            {
                direction = Vector3.Reflect(direction, hit.normal);
                position = hit.point;
                nodes.Add(new Tuple<Vector3, Vector3>(position, hit.normal));
            }
            else
            {
                position += direction * currentMaxStepDistance;
                reflectionsRemaining = 0;
                nodes.Add(new Tuple<Vector3, Vector3>(position, Vector3.zero));
            }
            GetTrajectoryNodes(nodes, position, direction, reflectionsRemaining - 1);
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(cancelled);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            cancelled = reader.ReadBoolean();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

    }
}