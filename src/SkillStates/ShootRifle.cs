﻿using EntityStates;
using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using TTGL_Survivor.Orbs;
using UnityEngine;

namespace TTGL_Survivor.SkillStates
{
    public class ShootRifle : BaseSkillState
    {
        public static float damageCoefficient = 2.0f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.6f;
        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;
        public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");

        private float duration;
        private float fireTime;
        private bool hasFired;
        private Animator animator;
        private string muzzleString;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = ShootRifle.baseDuration / this.attackSpeedStat;
            this.fireTime = 0.2f * this.duration;
            base.characterBody.SetAimTimer(2f);
            this.animator = base.GetModelAnimator();
            this.muzzleString = "Muzzle";
            base.PlayAnimation("Gesture, Override", "ShootRifle", "ShootRifle.playbackRate", 2f * this.duration);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void Fire()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                base.characterBody.AddSpreadBloom(1.5f);
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol.effectPrefab, base.gameObject, this.muzzleString, false);
                Util.PlaySound("TTGLTokoRifleFire", base.gameObject);

                if (base.isAuthority)
                {
                    Ray aimRay = base.GetAimRay();
                    base.AddRecoil(-1f * ShootRifle.recoil, -2f * ShootRifle.recoil, -0.5f * ShootRifle.recoil, 0.5f * ShootRifle.recoil);
                    var bulletAttack = new BulletAttack
                    {
                        bulletCount = 1,
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        damage = ShootRifle.damageCoefficient * this.damageStat,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.Generic,
                        falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                        maxDistance = ShootRifle.range,
                        force = ShootRifle.force,
                        hitMask = LayerIndex.CommonMasks.bullet,
                        minSpread = 0f,
                        maxSpread = 0f,
                        isCrit = base.RollCrit(),
                        owner = base.gameObject,
                        muzzleName = muzzleString,
                        smartCollision = false,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = procCoefficient,
                        radius = 0.75f,
                        sniper = false,
                        stopperMask = LayerIndex.CommonMasks.bullet,
                        weapon = null,
                        tracerEffectPrefab = ShootRifle.tracerEffectPrefab,
                        spreadPitchScale = 0f,
                        spreadYawScale = 0f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol.hitEffectPrefab,                        
                    };
                    bulletAttack.hitCallback = (ref BulletAttack.BulletHit hitInfo) =>
                    {
                        var result = bulletAttack.DefaultHitCallback(ref hitInfo);
                        if (hitInfo.hitHurtBox)
                        {
                            base.characterBody.outOfCombatStopwatch = 0f;
                            if (bulletAttack.isCrit)
                            {
                                CritRicochetOrb critRicochetOrb = new CritRicochetOrb();
                                critRicochetOrb.damageValue = bulletAttack.damage;
                                critRicochetOrb.isCrit = base.RollCrit();
                                critRicochetOrb.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
                                critRicochetOrb.attacker = base.gameObject;
                                critRicochetOrb.attackerBody = base.characterBody;
                                critRicochetOrb.procCoefficient = bulletAttack.procCoefficient;
                                critRicochetOrb.speed = 100.0f;
                                critRicochetOrb.bouncedObjects = new List<HealthComponent>();
                                critRicochetOrb.range = hitInfo.distance;
                                critRicochetOrb.tracerEffectPrefab = bulletAttack.tracerEffectPrefab;
                                critRicochetOrb.hitEffectPrefab = bulletAttack.hitEffectPrefab;
                                critRicochetOrb.hitSoundString = "TTGLTokoRifleCrit";

                                var nextTarget = critRicochetOrb.PickNextTarget(hitInfo.point);
                                if (nextTarget)
                                {
                                    Util.PlaySound("TTGLTokoRifleCrit", nextTarget.gameObject);
                                    critRicochetOrb.origin = hitInfo.point;
                                    critRicochetOrb.target = nextTarget;
                                    OrbManager.instance.AddOrb(critRicochetOrb);
                                }
                            }
                        }                        
                        return result;
                    };
                    bulletAttack.Fire();
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.fireTime)
            {
                this.Fire();
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}