using EntityStates;
using EntityStates.Mage.Weapon;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using System.Collections.Generic;
using TTGL_Survivor.Modules;
using TTGL_Survivor.Modules.Components;
using TTGL_Survivor.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class YokoPiercingRifle : BaseSkillState
    {
        public static float damageCoefficient = 1.5f;
        public static float procCoefficient = 1.5f;
        public static float baseDuration = 0.6f;
        public static float throwForce = 200f;
        public static float recoil = 3f;
        // public static GameObject tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");

        public float charge = 1f;
        private float duration;
        private string muzzleString;
        private Ray initialAimRay;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = YokoPiercingRifle.baseDuration / this.attackSpeedStat;
            base.characterBody.SetAimTimer(2f);
            this.muzzleString = "Muzzle";
            base.PlayAnimation("Gesture, Override", "ShootRifle", "ShootRifle.playbackRate", 2f * this.duration);
            base.characterBody.AddSpreadBloom(1.5f);
            EffectManager.SimpleMuzzleFlash(Modules.Assets.yokoRifleMuzzleSmallEffect, base.gameObject, this.muzzleString, false);
            Util.PlaySound("TTGLTokoRifleFire", base.gameObject);
            base.AddRecoil(-1f * YokoPiercingRifle.recoil, -2f * YokoPiercingRifle.recoil, -0.5f * YokoPiercingRifle.recoil, 0.5f * YokoPiercingRifle.recoil);
            if (base.isAuthority)
            {
                this.initialAimRay = base.GetAimRay();
            }
            if (NetworkServer.active)
            {
                this.FireServer(this.initialAimRay, this.charge);
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
            base.OnExit();
        }

        private void FireServer(Ray aimRay, float scale)
        {
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = Projectiles.yokoPiercingRoundPrefab,
                position = aimRay.origin,
                rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                owner = base.gameObject,
                damage = YokoPiercingRifle.damageCoefficient * this.damageStat * scale,
                force = 100f,
                crit = base.RollCrit(),
                damageColorIndex = DamageColorIndex.Default,
                target = null,
                speedOverride = YokoPiercingRifle.throwForce,
                fuseOverride = -1f,
                damageTypeOverride = null
            };
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(fireProjectileInfo.projectilePrefab, fireProjectileInfo.position, fireProjectileInfo.rotation);           
            ProjectileController component = gameObject.GetComponent<ProjectileController>();
            component.NetworkpredictionId = 0;
            ProjectileManager.InitializeProjectile(component, fireProjectileInfo);
            var scaleProjectileController = gameObject.GetComponent<ScaleProjectileController>();
            scaleProjectileController.NetworkChargeRate = scale;
            NetworkServer.Spawn(gameObject);
            
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            Vector3 origin = this.initialAimRay.origin;
            Vector3 direction = this.initialAimRay.direction;
            writer.Write(charge);
            writer.Write(origin);
            writer.Write(direction);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.charge = reader.ReadSingle();
            Vector3 origin = reader.ReadVector3();
            Vector3 direction = reader.ReadVector3();
            this.initialAimRay = new Ray(origin, direction);
        }
    }
}