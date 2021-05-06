using EntityStates;
using EntityStates.Huntress;
using RewiredConsts;
using RoR2;
using RoR2.Projectile;
using RoR2.UI;
using System.Linq;
using TTGL_Survivor.Modules;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class LagannCombine : BaseSkillState
    {
        public const float energyCost = 100f;
        public static string soundString = "TTGLCombine";
        public static float baseDuration = 18f;
        private Vector3 previousPosition;
        private float energy;
        private uint combineSoundRef;
        public static bool playedCutSceneOnce = true;
        private FrequencyConfig cinematicFrequence;

        public override void OnEnter()
        {
            base.OnEnter();
            this.cinematicFrequence = Modules.Config.ttglShowCombiningAnimation.Value;
            if (DisplayCinematic())
            {
                base.PlayAnimation("FullBody, Override", "TTGLSurvivorCombine", "Combine.playbackRate", LagannCombine.baseDuration);
                FreezeTime(true);
                AllowOutOfBound(true);
                this.SetAntiGravity(base.characterBody, true);
                this.previousPosition = base.characterMotor.Motor.transform.position;
                this.SetPosition(base.gameObject, base.characterMotor.Motor.transform.position + new Vector3(50000f, 0, 0f));                
                combineSoundRef = Util.PlaySound(LagannCombine.soundString, base.gameObject);
            }
            else
            {
                Util.PlaySound(BaseBeginArrowBarrage.blinkSoundString, base.gameObject);
            }
            var spiralEnergyComponent = base.characterBody.GetComponent<SpiralEnergyComponent>();
            this.energy = spiralEnergyComponent.NetworkEnergy;
            var ttglMusicRemote = base.characterBody.GetComponent<TTGLMusicRemote>();
            ttglMusicRemote.PlayMusic(TTGLMusicController.MusicType.Combine);
        }

        public override void OnExit()
        {
            if (DisplayCinematic())
            {
                this.SetPosition(base.gameObject, this.previousPosition);
                this.SetAntiGravity(base.characterBody, false);
                FreezeTime(false);
                AllowOutOfBound(false);
                AkSoundEngine.StopPlayingID(this.combineSoundRef);
                playedCutSceneOnce = true;
            }
            TransformToGurrenLagann();            
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();   
            if (!DisplayCinematic())
            {
                this.outer.SetNextStateToMain();
                return;
            }
            if (base.isAuthority && (base.fixedAge >= LagannCombine.baseDuration))
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
                RemoveGurren();
                var master = base.characterBody.master;
                master.TransformBody("GurrenLagannBody");
                var newBody = master.GetBody();
                var spiralEnergyComponent = newBody.GetComponent<SpiralEnergyComponent>();
                spiralEnergyComponent.NetworkEnergy = this.energy;
                var extraSkillLocator = newBody.GetComponent<ExtraSkillSlots.ExtraSkillLocator>();
                extraSkillLocator.extraFirst.RemoveAllStocks();
            }
        }

        private void RemoveGurren()
        {
            var bodyIndex = BodyCatalog.FindBodyIndex("GurrenBody");
            var masterIndex = MasterCatalog.FindMasterIndex("GurrenAllyMaster");
            var players = TeamComponent.GetTeamMembers(TeamIndex.Player);
            if (players != null)
            {
                foreach (var player in players.ToList())
                {                    
                    if (player.body && player.body.bodyIndex == bodyIndex &&
                        player.body.master && player.body.master.masterIndex == masterIndex)
                    {
                        var master = player.body.master;
                        master.DestroyBody();
                        NetworkServer.Destroy(master.gameObject);
                        return;
                    }
                }
            }
        }

        private bool DisplayCinematic()
        {
            return (cinematicFrequence == FrequencyConfig.Always || (!playedCutSceneOnce && cinematicFrequence == FrequencyConfig.Once));
        }
    }
}