using EntityStates;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TTGL_Survivor.SkillStates
{
    public class AimLagannImpact : BaseState
    {
        public const float c_MaxDuration = 3.0f;
        public const float c_MaxStepDistance = 100.0f;
        public const int c_MaxRebound = 3;
        private Quaternion m_OriginalRotation;
        private LineRenderer m_LineRenderer;
        private Tuple<Vector3, Vector3>[] m_TrajectoryNodes;
        private Animator animator;
        private bool cancelled;

        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
            cancelled = true;
            base.characterBody.SetAimTimer(c_MaxDuration);
            this.m_LineRenderer = base.gameObject.AddComponent<LineRenderer>();
            this.m_LineRenderer.startWidth = 0.5f;
            this.m_LineRenderer.endWidth = 0.5f;
            this.m_LineRenderer.material = Modules.Assets.mainAssetBundle.LoadAsset<Material>("SparksMat");
            this.m_LineRenderer.startColor = Color.green;
            this.m_LineRenderer.endColor = Color.white;
            this.m_LineRenderer.textureMode = LineTextureMode.Tile;
            
            if (base.cameraTargetParams)
            {
                base.cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
            }
            this.m_OriginalRotation = base.characterDirection.targetTransform.rotation;
            base.characterDirection.targetTransform.rotation = Util.QuaternionSafeLookRotation(base.inputBank.aimDirection);
            base.characterDirection.enabled = false;
        }

        public override void OnExit()
        {
            base.characterDirection.targetTransform.rotation = this.m_OriginalRotation;
            base.characterDirection.enabled = true;
            if (base.cameraTargetParams)
            {
                base.cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
            }
            if (this.cancelled)
            {
                this.animator.SetInteger("LagannImpact.stage", 0);
            }
            Destroy(this.m_LineRenderer);
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.inputBank)
            {                
                if (base.characterMotor && base.characterDirection)
                {
                    base.characterMotor.velocity = Vector3.zero;
                    base.characterDirection.targetTransform.rotation = Util.QuaternionSafeLookRotation(base.inputBank.aimDirection);
                }
                if (base.isAuthority)
                {
                    this.UpdateTrajectoryNodes(this.transform.position, base.inputBank.aimDirection, c_MaxRebound);
                    this.DrawTrajectoryLine();
                    if ((base.skillLocator && base.skillLocator.utility.IsReady() && base.inputBank.skill3.justPressed) || base.inputBank.interact.justPressed || base.inputBank.jump.justPressed)
                    {
                        this.outer.SetNextStateToMain();
                        return;
                    }
                    if (base.fixedAge >= c_MaxDuration || base.inputBank.skill1.justPressed || base.inputBank.skill2.justPressed || base.inputBank.skill4.justPressed)
                    {
                        this.cancelled = false;
                        this.outer.SetNextState(new LagannImpact() { TrajectoryNodes = this.m_TrajectoryNodes });
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
            if (Physics.Raycast(ray, out hit, c_MaxStepDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            {
                direction = Vector3.Reflect(direction, hit.normal);
                position = hit.point;
                nodes.Add(new Tuple<Vector3, Vector3>(position, hit.normal));
            }
            else
            {
                position += direction * c_MaxStepDistance;
                reflectionsRemaining = 0;
                nodes.Add(new Tuple<Vector3, Vector3>(position, Vector3.zero));
            }
            GetTrajectoryNodes(nodes, position, direction, reflectionsRemaining - 1);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

    }
}