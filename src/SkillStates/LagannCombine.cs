using EntityStates;
using EntityStates.Huntress;
using RoR2;
using RoR2.Projectile;
using RoR2.UI;
using System;
using System.Linq;
using TTGL_Survivor.Modules;
using TTGL_Survivor.Modules.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class LagannCombine : BaseSkillState
    {
        public static event Action onLagannCombineGlobal;

        public const float energyCost = 100f;
        public static string soundString = "TTGLCombine";
        public static float baseDuration = 18f;
        private Vector3 previousPosition;
        private Vector3 animationPosition;
        private float energy;
        private uint combineSoundRef;
        public static bool playedCutSceneOnce = true;
        private FrequencyConfig cinematicFrequence;
        private bool onLagannCombinedCalled = false;
        private Transform specialMoveCameraSource;
        private SkippableCamera forcedCamera;

        public override void OnEnter()
        {
            base.OnEnter();
            this.cinematicFrequence = Modules.Config.ttglShowCombiningAnimation;
            if (DisplayCinematic())
            {
                base.PlayAnimation("FullBody, Override", "TTGLSurvivorCombine", "Combine.playbackRate", LagannCombine.baseDuration);
                AllowOutOfBound(true);
                this.SetAntiGravity(base.characterBody, true);
                this.previousPosition = base.characterMotor.Motor.transform.position;
                this.animationPosition = this.previousPosition + new Vector3(50000f, 0, 0f);
                this.SetPosition(base.gameObject, this.animationPosition);
                if (CameraRigController.IsObjectSpectatedByAnyCamera(base.gameObject))
                {
                    combineSoundRef = Util.PlaySound(LagannCombine.soundString, base.gameObject);
                }
                var childSelector = base.GetModelChildLocator();
                if (childSelector)
                {
                    this.specialMoveCameraSource = childSelector.FindChild("SpecialMoveCameraSource");
                    this.forcedCamera = specialMoveCameraSource.GetComponent<SkippableCamera>();
                }
                UpdateCameraOverride();
                if (NetworkServer.active)
                {
                    base.characterBody.AddBuff(RoR2.RoR2Content.Buffs.HiddenInvincibility);
                }
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
                DisableCameraOverride();
                this.SetPosition(base.gameObject, this.previousPosition);
                this.SetAntiGravity(base.characterBody, false);
                AllowOutOfBound(false);
                base.PlayAnimation("FullBody, Override", "BufferEmpty");
                AkSoundEngine.StopPlayingID(this.combineSoundRef);
                playedCutSceneOnce = true;                
                if (NetworkServer.active) base.characterBody.RemoveBuff(RoR2.RoR2Content.Buffs.HiddenInvincibility);
            }
            TransformToGurrenLagann();            
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();   
            if (!DisplayCinematic())
            {
                if (base.isAuthority)
                {
                    this.outer.SetNextStateToMain();
                }
                return;
            }
            this.SetPosition(base.gameObject, this.animationPosition);
            UpdateCameraOverride();
            if (!onLagannCombinedCalled && base.fixedAge >= (LagannCombine.baseDuration - 2f))
            {
                onLagannCombinedCalled = true;
                OnLagannCombined();
            }
            if (base.isAuthority)
            {
                if ((base.fixedAge >= LagannCombine.baseDuration) || (base.inputBank && base.inputBank.interact.down))
                {
                    this.outer.SetNextStateToMain();
                }                
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
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
                self.queuedCollisions.Add(new MapZone.CollisionInfo(self, other));
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
            }
        }

        private void RemoveGurren()
        {
            var players = TeamComponent.GetTeamMembers(TeamIndex.Player);
            var gurrenMinionCache = GurrenMinionCache.GetOrSetGurrenStatusCache(base.characterBody.master);
            if (players != null)
            {
                foreach (var player in players.ToList())
                {                    
                    if (player.body &&
                        player.body.master && 
                        gurrenMinionCache.gurrenMinion == player.body.master)
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
            return (cinematicFrequence == FrequencyConfig.Always || (!playedCutSceneOnce && cinematicFrequence == FrequencyConfig.OncePerRun));
        }

        private void OnLagannCombined()
        {
            Action action = onLagannCombineGlobal;
            if (action == null)
            {
                return;
            }
            action();
        }

        private void UpdateCameraOverride()
        {
            if (this.specialMoveCameraSource && this.forcedCamera)
            {
                if (CameraRigController.IsObjectSpectatedByAnyCamera(base.gameObject))
                {
                    this.specialMoveCameraSource.gameObject.SetActive(true);
                    this.forcedCamera.allowUserControl = base.isAuthority;
                }
                else
                {
                    this.specialMoveCameraSource.gameObject.SetActive(false);
                }
            }
        }

        private void DisableCameraOverride()
        {
            if (this.specialMoveCameraSource)
            {
                this.specialMoveCameraSource.gameObject.SetActive(false);
            }
        }
    }
}