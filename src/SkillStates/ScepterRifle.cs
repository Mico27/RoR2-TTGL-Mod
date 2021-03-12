﻿using EntityStates;
using EntityStates.Mage.Weapon;
using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using TTGL_Survivor.Orbs;
using UnityEngine;

namespace TTGL_Survivor.SkillStates
{
    public class ScepterRifle : BaseSkillState
    {
        public static float damageCoefficient = 2.5f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.6f;
        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;
       // public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");

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
                EffectManager.SimpleMuzzleFlash(Modules.Assets.yokoRifleMuzzleSmallEffect, base.gameObject, this.muzzleString, false);
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
                        damage = 0f,
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
                        tracerEffectPrefab = FireLaserbolt.tracerEffectPrefab,
                        spreadPitchScale = 0f,
                        spreadYawScale = 0f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        hitEffectPrefab = Modules.Assets.yokoRifleHitSmallEffect,                        
                    };
                    bulletAttack.hitCallback = (ref BulletAttack.BulletHit hitInfo) =>
                    {
                        var result = bulletAttack.DefaultHitCallback(ref hitInfo);
                        this.Explode(hitInfo.point, bulletAttack.isCrit, (hitInfo.hitHurtBox)? hitInfo.hitHurtBox.gameObject: hitInfo.entityObject);
                        if (hitInfo.hitHurtBox)
                        {
                            if (bulletAttack.isCrit)
                            {
                                CritRicochetOrb critRicochetOrb = new CritRicochetOrb();
                                critRicochetOrb.damageValue = bulletAttack.damage;
                                critRicochetOrb.isCrit = base.RollCrit();
                                critRicochetOrb.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
                                critRicochetOrb.attacker = base.gameObject;
                                critRicochetOrb.attackerBody = base.characterBody;
                                critRicochetOrb.procCoefficient = bulletAttack.procCoefficient;
                                critRicochetOrb.duration = 0.5f;
                                critRicochetOrb.bouncedObjects = new List<HealthComponent>();
                                critRicochetOrb.range = Mathf.Max(30f, hitInfo.distance);
                                critRicochetOrb.tracerEffectPrefab = FireLaserbolt.tracerEffectPrefab;
                                critRicochetOrb.hitEffectPrefab = Modules.Assets.yokoRifleHitSmallEffect;
                                critRicochetOrb.hitSoundString = "TTGLTokoRifleCrit";
                                critRicochetOrb.origin = hitInfo.point;
                                critRicochetOrb.bouncedObjects.Add(hitInfo.hitHurtBox.healthComponent);
                                critRicochetOrb.hitCallback = (CritRicochetOrb orb) =>
                                {
                                    this.Explode(orb.target.transform.position, orb.isCrit, orb.target.gameObject);
                                };
                                var nextTarget = critRicochetOrb.PickNextTarget(hitInfo.point);
                                if (nextTarget)
                                {
                                    critRicochetOrb.target = nextTarget;
                                    critRicochetOrb.FireDelayed();
                                }
                            }
                        }                        
                        return result;
                    };
                    bulletAttack.Fire();
                }
            }
        }

        private void Explode(Vector3 spawnPosition, bool isCrit, GameObject SoundGameObject)
        {
            Util.PlaySound("HenryBombExplosion", SoundGameObject ?? base.gameObject);
            EffectManager.SpawnEffect(Modules.Assets.yokoRifleExplosiveRoundExplosion, new EffectData
            {
                origin = spawnPosition,
                scale = 20f
            }, true);
            new BlastAttack
            {
                position = spawnPosition,
                baseDamage = ScepterRifle.damageCoefficient * this.damageStat,
                baseForce = 100f,
                radius = 20f,
                attacker = base.gameObject,
                inflictor = base.gameObject,
                teamIndex = TeamComponent.GetObjectTeam(base.gameObject),
                crit = isCrit,
                procChainMask = default(ProcChainMask),
                procCoefficient = procCoefficient,
                bonusForce = Vector3.zero,
                falloffModel = BlastAttack.FalloffModel.Linear,
                damageType = DamageType.Generic,
            }.Fire();
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