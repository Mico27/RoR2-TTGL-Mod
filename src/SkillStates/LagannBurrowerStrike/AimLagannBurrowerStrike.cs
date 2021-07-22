using RoR2;
using UnityEngine;
using EntityStates;
using RoR2.Audio;
using System;
using UnityEngine.Networking;
using EntityStates.Merc;
using EntityStates.Huntress;
using TTGL_Survivor.Modules;
using System.Collections.Generic;
using System.Linq;
using RoR2.Projectile;
using EntityStates.Loader;

namespace TTGL_Survivor.SkillStates
{
    public class AimLagannBurrowerStrike : BaseSkillState
    {
        public static float maxDuration = 5.0f;

        private HurtBoxGroup hurtboxGroup;
        private GameObject areaIndicatorInstance;
        private CameraTargetParams.AimRequest aimRequest;

        public override void OnEnter()
        {
            base.OnEnter();
            var modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                this.hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
                this.hurtboxGroup.hurtBoxesDeactivatorCounter++;
            }
            base.PlayAnimation("FullBody, Override", "LagannBurrowerStrikeHidden");
            if (ArrowRain.areaIndicatorPrefab)
            {
                this.areaIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(ArrowRain.areaIndicatorPrefab);
                this.areaIndicatorInstance.transform.localScale = new Vector3(1.5f, ArrowRain.arrowRainRadius, 1.5f);
                UpdateAreaIndicator();
            }
            if (base.cameraTargetParams)
            {
                this.aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.inputBank &&
                (base.fixedAge >= AimLagannBurrowerStrike.maxDuration || base.inputBank.skill1.justPressed || base.inputBank.skill4.justPressed))
            {
                this.SetNextState();
            }
                    
        }
        public override void Update()
        {
            base.Update();
            this.UpdateAreaIndicator();
        }
        public override void OnExit()
        {
            if (this.areaIndicatorInstance)
            {
                EntityState.Destroy(this.areaIndicatorInstance);
            }
            if (this.aimRequest != null)
            {
                this.aimRequest.Dispose();
            }
            if (this.hurtboxGroup)
            {
                this.hurtboxGroup.hurtBoxesDeactivatorCounter--;
            }
            base.OnExit();
        }

        private void UpdateAreaIndicator()
        {
            if (this.areaIndicatorInstance)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(base.GetAimRay(), out raycastHit, 1000f, LayerIndex.world.mask))
                {
                    this.areaIndicatorInstance.transform.position = raycastHit.point;
                    this.areaIndicatorInstance.transform.up = raycastHit.normal;
                }
            }
        }

        private void SetNextState()
        {
            if (this.areaIndicatorInstance)
            {
                this.outer.SetNextState(new LagannBurrowerStrike
                {
                    spawnLocation = this.areaIndicatorInstance.transform.position,
                    spawnRotation = this.areaIndicatorInstance.transform.up,
                });
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

    }
}