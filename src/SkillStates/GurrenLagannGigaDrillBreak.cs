using System;
using EntityStates;
using EntityStates.Huntress;
using EntityStates.VagrantMonster;
using RoR2;
using RoR2.Projectile;
using TTGL_Survivor.Modules;
using TTGL_Survivor.Modules.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{   
    public class GurrenLagannGigaDrillBreak : BaseSkillState
    {
        public static float damageCoefficient = 1000000f;
        public static float radius = 1000000f;
        public static int explosionCount = 2;
        public static float explosionInterval = 2f;
        public static string soundString = "TTGLGigaDrillBreak";
        protected Animator animator;
        protected CharacterBody gigaDrillBreakTarget;
        private float duration;
        private float durationBeforeExplosion;
        private int explosionIndex;
        private Vector3 previousPosition;
        private Vector3 previousTargetPosition;
        private Transform specialMoveTargetPosition;
        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
            this.explosionIndex = 0;
            duration = 22.5f;
            durationBeforeExplosion = 16f;
            var gurrenLagannController = base.characterBody.GetComponent<GurrenLagannController>();
            if (gurrenLagannController && gurrenLagannController.gigaDrillBreakTarget)
            {
                gigaDrillBreakTarget = gurrenLagannController.gigaDrillBreakTarget.GetComponent<CharacterBody>();
                ConstrictBoss(gigaDrillBreakTarget);
            }            
            base.PlayAnimation("FullBody, Override", "GurrenLagannGigaDrillBreak", "skill4.playbackRate", this.duration);
            Util.PlaySound(GurrenLagannGigaDrillBreak.soundString, base.gameObject);
            FreezeTime(true);
            AllowOutOfBound(true);
            this.SetAntiGravity(base.characterBody, true);
            this.previousPosition = base.characterMotor.Motor.transform.position;
            this.SetPosition(base.gameObject, base.characterMotor.Motor.transform.position + new Vector3(50000f, 0, 0f));
            if (gigaDrillBreakTarget)
            {
                var targetDirection = (gigaDrillBreakTarget.transform.position - base.characterDirection.targetTransform.position).normalized;
                base.characterDirection.forward = targetDirection;
                this.SetAntiGravity(gigaDrillBreakTarget, true);
                this.previousTargetPosition = gigaDrillBreakTarget.transform.position;
                this.SetPosition(gigaDrillBreakTarget.gameObject, gigaDrillBreakTarget.transform.position + new Vector3(50000f, 0f, 0f));
            }
            var childSelector = base.GetModelChildLocator();
            if (childSelector)
            {
                this.specialMoveTargetPosition = childSelector.FindChild("SpecialMoveTargetPosition");
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (this.explosionIndex < GurrenLagannGigaDrillBreak.explosionCount &&
                base.fixedAge >= this.durationBeforeExplosion)
            {
                this.Explode();
                this.durationBeforeExplosion += GurrenLagannGigaDrillBreak.explosionInterval;
                this.explosionIndex++;
            }
            if (this.specialMoveTargetPosition && this.gigaDrillBreakTarget)
            {
                this.SetPosition(gigaDrillBreakTarget.gameObject, this.specialMoveTargetPosition.position);
            }
            if (base.isAuthority)
            {
                if (base.fixedAge >= this.duration)
                {
                    this.outer.SetNextState(new GurrenLagannSplit());
                }
            }
        }

        public override void OnExit()
        {

            this.SetPosition(base.gameObject, this.previousPosition);
            this.SetAntiGravity(base.characterBody, false);
            if (gigaDrillBreakTarget)
            {
                this.SetPosition(gigaDrillBreakTarget.gameObject, this.previousTargetPosition);
                this.SetAntiGravity(gigaDrillBreakTarget, false);
            }
            UnConstrictBoss(gigaDrillBreakTarget);
            FreezeTime(false);
            AllowOutOfBound(false);
            base.OnExit();
        }

        private void Explode()
        {
            if (this.gigaDrillBreakTarget)
            {
                if (explosionIndex == 0)
                {
                    EffectManager.SpawnEffect(Assets.specialExplosion, new EffectData
                    {
                        origin = this.gigaDrillBreakTarget.transform.position,
                        scale = 1f
                    }, false);
                }
                else
                {
                    if (NetworkServer.active)
                    {
                        new BlastAttack
                        {
                            damageType = (DamageType.BypassArmor | DamageType.BypassOneShotProtection | DamageType.WeakPointHit),
                            attacker = base.gameObject,
                            inflictor = base.gameObject,
                            teamIndex = TeamComponent.GetObjectTeam(base.gameObject),
                            baseDamage = this.damageStat * GurrenLagannGigaDrillBreak.damageCoefficient,
                            baseForce = ExplosionAttack.force,
                            position = this.gigaDrillBreakTarget.transform.position,
                            radius = GurrenLagannGigaDrillBreak.radius,
                            falloffModel = BlastAttack.FalloffModel.None,
                            attackerFiltering = AttackerFiltering.NeverHit
                        }.Fire();
                    }
                }
            }
        }

        private void ConstrictBoss(CharacterBody bossBody)
        {
            if (bossBody)
            {
                var existingConstrict = bossBody.GetComponentInChildren<GurrenLagannShadesConstrictComponent>();
                if (!existingConstrict || existingConstrict.visualState == 1)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(TemporaryVisualEffects.gurrenLagannShadesBindingEffect, bossBody.corePosition, Quaternion.identity);
                    var component = gameObject.GetComponent<GurrenLagannShadesConstrictComponent>();
                    component.duration = this.duration;
                    component.parentTransform = bossBody.transform;
                    gameObject.transform.SetParent(bossBody.transform);
                }
                else
                {
                    existingConstrict.durationCounter = 0f;
                    existingConstrict.duration = this.duration;
                }
            }
        }

        private void UnConstrictBoss(CharacterBody bossBody)
        {
            if (bossBody)
            {
                var existingConstrict = bossBody.GetComponentInChildren<GurrenLagannShadesConstrictComponent>();
                if (existingConstrict && existingConstrict.visualState == 0)
                {
                    existingConstrict.durationCounter = 0f;
                    existingConstrict.duration = 0f;
                }
            }
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
            if (teamComponent.body == this.gigaDrillBreakTarget)
            {
                return;
            }
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
            if (component && (component == gigaDrillBreakTarget || component == base.characterBody))
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

    }
}