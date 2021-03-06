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

        internal static void RegisterProjectiles()
        {
            // only separating into separate methods for my sanity
            CreateBomb();

            ProjectileCatalog.getAdditionalEntries += list =>
            {
                list.Add(explosiveRifleRoundPrefab);
            };
        }

        private static void CreateBomb()
        {
            explosiveRifleRoundPrefab = CloneProjectilePrefab("CommandoGrenadeProjectile", "HenryBombProjectile");

            ProjectileImpactExplosion impactExplosion = explosiveRifleRoundPrefab.GetComponent<ProjectileImpactExplosion>();
            InitializeImpactExplosion(impactExplosion);

            impactExplosion.blastRadius = 20f;
            impactExplosion.destroyOnEnemy = true;
            impactExplosion.lifetime = 12f;
            impactExplosion.impactEffect = Modules.Assets.yokoRifleExplosiveRoundExplosion;
            impactExplosion.explosionSoundString = "HenryBombExplosion";
            impactExplosion.timerAfterImpact = true;
            impactExplosion.lifetimeAfterImpact = 0.1f;

            ProjectileController projectileController = explosiveRifleRoundPrefab.GetComponent<ProjectileController>();
            projectileController.ghostPrefab = CreateGhostPrefab("YokoRifleExplosiveRound");
            projectileController.startSound = "";
        }

        private static void InitializeImpactExplosion(ProjectileImpactExplosion projectileImpactExplosion)
        {
            projectileImpactExplosion.blastDamageCoefficient = 1f;
            projectileImpactExplosion.blastProcCoefficient = 1f;
            projectileImpactExplosion.bonusBlastForce = Vector3.zero;
            projectileImpactExplosion.childrenCount = 0;
            projectileImpactExplosion.childrenDamageCoefficient = 0f;
            projectileImpactExplosion.childrenProjectilePrefab = null;
            projectileImpactExplosion.destroyOnEnemy = false;
            projectileImpactExplosion.destroyOnWorld = false;
            projectileImpactExplosion.explosionSoundString = "";
            projectileImpactExplosion.falloffModel = RoR2.BlastAttack.FalloffModel.None;
            projectileImpactExplosion.fireChildren = false;
            projectileImpactExplosion.impactEffect = null;
            projectileImpactExplosion.lifetime = 0f;
            projectileImpactExplosion.lifetimeAfterImpact = 0f;
            projectileImpactExplosion.lifetimeExpiredSoundString = "";
            projectileImpactExplosion.lifetimeRandomOffset = 0f;
            projectileImpactExplosion.offsetForLifetimeExpiredSound = 0f;
            projectileImpactExplosion.timerAfterImpact = false;

            projectileImpactExplosion.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;
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