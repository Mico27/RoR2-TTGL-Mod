using EntityStates;
using EntityStates.Mage.Weapon;
using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using TTGL_Survivor.Orbs;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.BulletAttack;

namespace TTGL_Survivor.SkillStates
{
    public class YokoScepterRifle : BaseSkillState
    {
        public static int maxRicochetCount = 6;
        public static bool resetBouncedObjects = true;
        public static float explosionSizeMultiplier = 1f;
        public static float damageCoefficient = 2.5f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.6f;
        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;

        private float duration;
        private string muzzleString;
        private Ray initialAimRay;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = YokoShootRifle.baseDuration / this.attackSpeedStat;
            base.characterBody.SetAimTimer(2f);
            this.muzzleString = "Muzzle";
            base.PlayAnimation("Yoko", "ShootRifle", "ShootRifle.playbackRate", 2f * this.duration);
            base.characterBody.AddSpreadBloom(1.5f);
            EffectManager.SimpleMuzzleFlash(Modules.Assets.yokoRifleMuzzleSmallEffect, base.gameObject, this.muzzleString, false);
            Util.PlaySound("TTGLTokoRifleFire", base.gameObject);
            base.AddRecoil(-1f * YokoShootRifle.recoil, -2f * YokoShootRifle.recoil, -0.5f * YokoShootRifle.recoil, 0.5f * YokoShootRifle.recoil);
            if (base.isAuthority)
            {
                this.initialAimRay = base.GetAimRay();
            }
            if (NetworkServer.active)
            {
                this.FireServer(this.initialAimRay);
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            base.PlayAnimation("Yoko", "BufferEmpty");
            base.OnExit();
        }
        
        private void FireServer(Ray aimRay)
        {
            bool hasHit = false;
            Vector3 hitPoint = Vector3.zero;
            float hitDistance = 0f;
            HealthComponent hitHealthComponent = null;
            var bulletAttack = new BulletAttack
            {
                bulletCount = 1,
                aimVector = aimRay.direction,
                origin = aimRay.origin,
                damage = YokoShootRifle.damageCoefficient * this.damageStat,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Generic,
                falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                maxDistance = YokoShootRifle.range,
                force = YokoShootRifle.force,
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
                tracerEffectPrefab = Modules.Assets.yokoRifleBeamEffect,
                spreadPitchScale = 0f,
                spreadYawScale = 0f,
                queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                hitEffectPrefab = Modules.Assets.yokoRifleHitSmallEffect,
            };
            if (maxRicochetCount > 0 && bulletAttack.isCrit)
            {
                bulletAttack.hitCallback = delegate (BulletAttack bulletAttackRef, ref BulletHit hitInfo)
                {
                    var result = BulletAttack.defaultHitCallback(bulletAttackRef, ref hitInfo);
                    hasHit = true;
                    hitPoint = hitInfo.point;
                    hitDistance = hitInfo.distance;
                    if (hitInfo.hitHurtBox)
                    {
                        hitHealthComponent = hitInfo.hitHurtBox.healthComponent;
                    }
                    return result;
                };
            }
            bulletAttack.filterCallback = delegate (BulletAttack bulletAttackRef, ref BulletAttack.BulletHit info)
            {
                return (!info.entityObject || info.entityObject != bulletAttack.owner) && BulletAttack.defaultFilterCallback(bulletAttackRef, ref info);
            };
            bulletAttack.Fire();
            if (hasHit)
            {
                this.Explode(hitPoint, bulletAttack.isCrit, (hitHealthComponent) ? hitHealthComponent.gameObject : null);
                if (hitHealthComponent != null)
                {
                    if (bulletAttack.isCrit)
                    {
                        CritRicochetOrb critRicochetOrb = new CritRicochetOrb();
                        critRicochetOrb.bouncesRemaining = maxRicochetCount - 1;
                        critRicochetOrb.resetBouncedObjects = resetBouncedObjects;
                        critRicochetOrb.damageValue = bulletAttack.damage;
                        critRicochetOrb.isCrit = base.RollCrit();
                        critRicochetOrb.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
                        critRicochetOrb.attacker = base.gameObject;
                        critRicochetOrb.attackerBody = base.characterBody;
                        critRicochetOrb.procCoefficient = bulletAttack.procCoefficient;
                        critRicochetOrb.duration = 0.1f;
                        critRicochetOrb.bouncedObjects = new List<HealthComponent>();
                        critRicochetOrb.range = Mathf.Max(30f, hitDistance);
                        critRicochetOrb.tracerEffectPrefab = Modules.Assets.yokoRifleBeamEffect;
                        critRicochetOrb.hitEffectPrefab = Modules.Assets.yokoRifleHitSmallEffect;
                        critRicochetOrb.hitSoundString = "TTGLTokoRifleCrit";
                        critRicochetOrb.origin = hitPoint;
                        critRicochetOrb.bouncedObjects.Add(hitHealthComponent);
                        var nextTarget = critRicochetOrb.PickNextTarget(hitPoint);
                        if (nextTarget)
                        {
                            critRicochetOrb.target = nextTarget;
                            OrbManager.instance.AddOrb(critRicochetOrb);
                        }
                    }
                }
            }
        }

        private void Explode(CritRicochetOrb orb)
        {
            this.Explode(orb.target.transform.position, orb.isCrit, orb.target.gameObject);
        }

        private void Explode(Vector3 spawnPosition, bool isCrit, GameObject SoundGameObject)
        {
            Util.PlaySound("Play_item_use_fireballDash_explode", SoundGameObject ?? base.gameObject);
            EffectManager.SpawnEffect(Modules.Assets.yokoRifleExplosiveRoundExplosion, new EffectData
            {
                origin = spawnPosition,
                scale = (20f * explosionSizeMultiplier)                 
            }, true);
            new BlastAttack
            {
                position = spawnPosition,
                baseDamage = YokoScepterRifle.damageCoefficient * this.damageStat,
                baseForce = 100f,
                radius = (20f * explosionSizeMultiplier),
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

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}