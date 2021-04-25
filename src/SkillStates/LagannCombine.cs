using EntityStates;
using EntityStates.Huntress;
using RoR2;
using RoR2.Projectile;
using TTGL_Survivor.Modules;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class LagannCombine : BaseSkillState
    {
        public static string soundString = "TTGLCombine";
        public static float baseDuration = 18f;
        private Vector3 previousPosition;

        public override void OnEnter()
        {
            base.OnEnter();
            base.PlayAnimation("FullBody, Override", "TTGLSurvivorCombine", "Combine.playbackRate", LagannCombine.baseDuration);
            FreezeTime(true);
            AllowOutOfBound(true);
            this.SetAntiGravity(base.characterBody, true);
            this.previousPosition = base.characterMotor.Motor.transform.position;
            this.SetPosition(base.gameObject, base.characterMotor.Motor.transform.position + new Vector3(50000f, 0, 0f));
            var ttglMusicRemote = base.characterBody.GetComponent<TTGLMusicRemote>();
            ttglMusicRemote.PlayMusic(TTGLMusicController.MusicType.Combine);
            Util.PlaySound(LagannCombine.soundString, base.gameObject);
        }

        public override void OnExit()
        {
            this.SetPosition(base.gameObject, this.previousPosition);
            this.SetAntiGravity(base.characterBody, false);
            FreezeTime(false);
            AllowOutOfBound(false);
            TransformToGurrenLagann();
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();        
            if (base.fixedAge >= LagannCombine.baseDuration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }


        private void FreezeTime(bool enableFunc)
        {
            this.FreezeTeamComponents(TeamIndex.Monster, enableFunc);
            this.FreezeTeamComponents(TeamIndex.Player, enableFunc);
        }

        private void FreezeTeamComponents(TeamIndex teamIndex, bool isEnabled)
        {
            var teamMembers = TeamComponent.GetTeamMembers(teamIndex);
            if (teamMembers != null && teamMembers.Count > 0)
            {
                foreach (TeamComponent teamComponent in teamMembers)
                {
                    FreezeTeamComponent(teamComponent, isEnabled);
                }
            }
        }
        private void FreezeTeamComponent(TeamComponent teamComponent, bool isEnabled)
        {
            if (teamComponent.body != base.characterBody)
            {
                var characterDirection = teamComponent.gameObject.GetComponent<CharacterDirection>();
                if (characterDirection)
                {
                    characterDirection.enabled = !isEnabled;
                }
                var characterMotor = teamComponent.gameObject.GetComponent<CharacterMotor>();
                if (characterMotor)
                {
                    characterMotor.enabled = !isEnabled;
                }
                var characterBody = teamComponent.gameObject.GetComponent<CharacterBody>();
                if (characterBody)
                {
                    characterBody.enabled = !isEnabled;
                }
                var entityStateMachines = teamComponent.gameObject.GetComponents<EntityStateMachine>();
                if (entityStateMachines != null && entityStateMachines.Length > 0)
                {
                    foreach (var entityStateMachine in entityStateMachines)
                    {
                        entityStateMachine.enabled = !isEnabled;
                    }
                }
                var modelLocator = teamComponent.gameObject.GetComponent<ModelLocator>();
                if (modelLocator && modelLocator.modelTransform)
                {
                    var animator = modelLocator.modelTransform.GetComponent<Animator>();
                    if (animator)
                    {
                        animator.enabled = !isEnabled;
                    }
                }
                var rigidBody = teamComponent.gameObject.GetComponent<Rigidbody>();
                if (rigidBody && !rigidBody.isKinematic)
                {
                    rigidBody.velocity = Vector3.zero;
                }
                var rigidBodyMotor = teamComponent.gameObject.GetComponent<RigidbodyMotor>();
                if (rigidBodyMotor)
                {
                    rigidBodyMotor.moveVector = Vector3.zero;
                    rigidBodyMotor.enabled = !isEnabled;
                }
            }
            if (teamComponent.teamIndex == TeamIndex.Player &&
                teamComponent.body && teamComponent.body.healthComponent &&
                teamComponent.body.healthComponent.alive)
            {
                if (isEnabled)
                {
                    teamComponent.body.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
                }
                else
                {
                    teamComponent.body.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
                }
            }
        }

        private void SetAntiGravity(CharacterBody character, bool enabled)
        {
            if (character && character.characterMotor)
            {
                CharacterGravityParameters gravityParameters = character.characterMotor.gravityParameters;
                gravityParameters.channeledAntiGravityGranterCount += (enabled ? 1 : -1);
                character.characterMotor.gravityParameters = gravityParameters;
            }
        }

        private void AllowOutOfBound(bool enabled)
        {
            if (enabled)
            {
                On.RoR2.MapZone.TryZoneStart += MapZone_TryZoneStart;
            }
            else
            {
                On.RoR2.MapZone.TryZoneStart -= MapZone_TryZoneStart;
            }
        }

        private void MapZone_TryZoneStart(On.RoR2.MapZone.orig_TryZoneStart orig, MapZone self, Collider other)
        {
            CharacterBody component = other.GetComponent<CharacterBody>();
            if (component && (component == base.characterBody))
            {
                return;
            }
            orig(self, other);
        }

        private void SetPosition(GameObject character, Vector3 newPosition)
        {
            CharacterMotor characterMotor = character.GetComponent<CharacterMotor>();
            if (characterMotor && characterMotor.Motor)
            {
                characterMotor.Motor.SetPosition(newPosition, true);
                characterMotor.Motor.BaseVelocity = Vector3.zero;
                characterMotor.velocity = Vector3.zero;
                characterMotor.rootMotion = Vector3.zero;
                return;
            }
            RigidbodyMotor rigidbodyMotor = character.GetComponent<RigidbodyMotor>();
            if (rigidbodyMotor && rigidbodyMotor.rigid)
            {
                rigidbodyMotor.rigid.MovePosition(newPosition);
                rigidbodyMotor.rootMotion = Vector3.zero;
                return;
            }
            gameObject.transform.position = newPosition;
        }

        private void TransformToGurrenLagann()
        {
            if (NetworkServer.active && base.characterBody && base.characterBody.master)
            {
                var spiralEnergyComponent = base.characterBody.GetComponent<SpiralEnergyComponent>();
                var energy = spiralEnergyComponent.energy;
                var charge_rate = spiralEnergyComponent.charge_rate;
                var master = base.characterBody.master;
                master.TransformBody("GurrenLagannBody");
                var newBody = master.GetBody();
                spiralEnergyComponent = newBody.GetComponent<SpiralEnergyComponent>();
                spiralEnergyComponent.energy = energy;
                spiralEnergyComponent.charge_rate = charge_rate;
            }
        }
    }
}