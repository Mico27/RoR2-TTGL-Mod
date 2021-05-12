using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.Modules
{
    internal static class Projectiles
    {
        internal static GameObject explosiveRifleRoundPrefab;
        internal static GameObject shadesWhirlPrefab;
        internal static GameObject bigBoulderPrefab;

        internal static void RegisterProjectiles()
        {
            // only separating into separate methods for my sanity
            CreateBomb();
            CreateGurrenLagannShadesProjectile();
            CreateBigBoulder();

            TTGL_SurvivorPlugin.projectilePrefabs.Add(explosiveRifleRoundPrefab);
            TTGL_SurvivorPlugin.projectilePrefabs.Add(shadesWhirlPrefab);
            TTGL_SurvivorPlugin.projectilePrefabs.Add(bigBoulderPrefab);
        }

        private static void CreateBomb()
        {
            explosiveRifleRoundPrefab = CloneProjectilePrefab("CommandoGrenadeProjectile", "YokoExplosiveRifleProjectile");
            ProjectileImpactExplosion impactExplosion = explosiveRifleRoundPrefab.GetComponent<ProjectileImpactExplosion>();
            impactExplosion.blastDamageCoefficient = 1f;
            impactExplosion.blastProcCoefficient = 1f;
            impactExplosion.bonusBlastForce = Vector3.zero;
            impactExplosion.childrenCount = 0;
            impactExplosion.childrenDamageCoefficient = 0f;
            impactExplosion.childrenProjectilePrefab = null;
            impactExplosion.destroyOnWorld = true;
            impactExplosion.destroyOnEnemy = true;
            impactExplosion.falloffModel = RoR2.BlastAttack.FalloffModel.Linear;
            impactExplosion.fireChildren = false;
            impactExplosion.lifetimeRandomOffset = 0f;
            impactExplosion.offsetForLifetimeExpiredSound = 0f;
            impactExplosion.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;
            impactExplosion.blastRadius = 20f;           
            impactExplosion.lifetime = 12f;
            impactExplosion.impactEffect = Modules.Assets.yokoRifleExplosiveRoundExplosion;
            impactExplosion.timerAfterImpact = true;
            impactExplosion.lifetimeAfterImpact = 0.1f;
            ProjectileController projectileController = explosiveRifleRoundPrefab.GetComponent<ProjectileController>();
            projectileController.ghostPrefab = CreateGhostPrefab("YokoRifleExplosiveRound");
            projectileController.startSound = "";
        }


        private static void CreateGurrenLagannShadesProjectile()
        {
            shadesWhirlPrefab = CloneProjectilePrefab("Sawmerang", "GurrenLagannShadesProjectile");
            ProjectileController projectileController = shadesWhirlPrefab.GetComponent<ProjectileController>();
            projectileController.ghostPrefab = CreateGhostPrefab("ShadesWhirlind");
            projectileController.startSound = "";

            BoomerangProjectile boomerangProjectile = shadesWhirlPrefab.GetComponent<BoomerangProjectile>();
            GurrenLagannShadesProjectile gurrenLagannShadesProjectile = shadesWhirlPrefab.AddComponent<GurrenLagannShadesProjectile>();
            gurrenLagannShadesProjectile.canHitWorld = true;
            gurrenLagannShadesProjectile.crosshairPrefab = boomerangProjectile.crosshairPrefab;
            gurrenLagannShadesProjectile.impactSpark = boomerangProjectile.impactSpark;

            var collider = shadesWhirlPrefab.GetComponent<BoxCollider>();
            collider.size = new Vector3(6.0f, 1.0f, 6.0f);

            var hitbox = shadesWhirlPrefab.GetComponentInChildren<HitBox>();
            hitbox.transform.localScale = new Vector3(8.0f, 3.0f, 8.0f);

            TTGL_SurvivorPlugin.DestroyImmediate(boomerangProjectile);

        }
        private static void CreateBigBoulder()
        {
            bigBoulderPrefab = CloneProjectilePrefab("CommandoGrenadeProjectile", "GurrenBigBoulderProjectile");
            var collider = bigBoulderPrefab.GetComponent<SphereCollider>();
            collider.radius = 4f;
            ProjectileImpactExplosion impactExplosion = bigBoulderPrefab.GetComponent<ProjectileImpactExplosion>();
            impactExplosion.blastDamageCoefficient = 1f;
            impactExplosion.blastProcCoefficient = 1f;
            impactExplosion.bonusBlastForce = Vector3.up * 10f;
            impactExplosion.childrenCount = 0;
            impactExplosion.childrenDamageCoefficient = 0f;
            impactExplosion.childrenProjectilePrefab = null;
            impactExplosion.destroyOnWorld = true;
            impactExplosion.destroyOnEnemy = true;
            impactExplosion.falloffModel = RoR2.BlastAttack.FalloffModel.None;
            impactExplosion.fireChildren = false;
            impactExplosion.lifetimeRandomOffset = 0f;
            impactExplosion.offsetForLifetimeExpiredSound = 0f;
            impactExplosion.GetComponent<ProjectileDamage>().damageType = DamageType.Stun1s;
            impactExplosion.blastRadius = 40f;            
            impactExplosion.lifetime = 12f;
            impactExplosion.timerAfterImpact = false;
            impactExplosion.impactEffect = Modules.Assets.gurrenBrokenBoulderEffect;
            impactExplosion.explosionSoundString = "Play_golem_impact";
            ProjectileController projectileController = bigBoulderPrefab.GetComponent<ProjectileController>();
            projectileController.ghostPrefab = CreateGhostPrefab("BigBoulderPrefab");
            projectileController.startSound = "";
            //anim params = isHoldingObject
            //states = GURREN_LiftingObject, GURREN_HoldingObject, GURREN_ThrowingObject
        }
        private static GameObject CreateGhostPrefab(string ghostName)
        {
            GameObject ghostPrefab = Modules.Assets.mainAssetBundle.LoadAsset<GameObject>(ghostName);
            ghostPrefab.AddComponent<NetworkIdentity>();
            ghostPrefab.AddComponent<ProjectileGhostController>();

            //Modules.Assets.ConvertAllRenderersToHopooShader(ghostPrefab);

            return ghostPrefab;
        }

        private static GameObject CloneProjectilePrefab(string prefabName, string newPrefabName)
        {
            GameObject newPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/" + prefabName), newPrefabName);
            return newPrefab;
        }
    }
}