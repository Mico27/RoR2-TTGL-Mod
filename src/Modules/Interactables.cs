using System;
using System.Linq;
using R2API;
using RoR2;
using RoR2.Hologram;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.Events;
using TTGL_Survivor.Modules.Components;
using TTGL_Survivor.Modules.Survivors;
using UnityEngine.AddressableAssets;

namespace TTGL_Survivor.Modules
{
    internal static class Interactables
    {
        public static PurchaseInteraction gurrenInteractPurchaseInteraction;
        public static InteractableSpawnCard gurrenInteractSpawnCard;

        internal static void RegisterInteractables()
        {
            CreateGurrenSummonInteractableSpawnCard();
            On.RoR2.SceneDirector.PopulateScene += SceneDirector_PopulateScene;
        }


        private static void SceneDirector_PopulateScene(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            if (gurrenInteractSpawnCard  && Run.instance.userMasters.Values.Any((x) =>
            {
                if (x != null && x.bodyPrefab != null)
                {
                    var body = x.bodyPrefab.GetComponent<CharacterBody>();
                    if (body)
                    {
                        var found = body.bodyIndex == BodyCatalog.FindBodyIndex("LagannBody");
                        if (found)
                        {                            
                            var gurrenMinionCache = GurrenMinionCache.GetOrSetGurrenStatusCache(x);
                            if (!gurrenMinionCache.gurrenMinion)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }))
            {
                TTGL_SurvivorPlugin.instance.Logger.LogMessage("Added Gurren On Level");
                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(gurrenInteractSpawnCard, new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                }, self.rng));
            }
            orig(self);
        }
        

        private static void CreateGurrenSummonInteractableSpawnCard()
        {
            gurrenInteractSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            var gurrenInteractPrefab = Modules.Assets.LoadAsset<GameObject>("GurrenInteractPrefab");            
            gurrenInteractPrefab.AddComponent<NetworkIdentity>();
            var childLocator = gurrenInteractPrefab.GetComponent<ChildLocator>();
            var model = childLocator.FindChild("Model");
            var hologramPivot = childLocator.FindChild("HologramPivot");           
            var modelLocator = model.gameObject.AddComponent<ModelLocator>();
            modelLocator.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            var entityLocator = model.gameObject.AddComponent<EntityLocator>();
            entityLocator.entity = gurrenInteractPrefab;
            var hightlight = gurrenInteractPrefab.AddComponent<Highlight>();
            hightlight.targetRenderer = gurrenInteractPrefab.GetComponentInChildren<Renderer>();
            hightlight.highlightColor = Highlight.HighlightColor.interactive;
            var hologramProjector = gurrenInteractPrefab.AddComponent<HologramProjector>();
            hologramProjector.hologramPivot = hologramPivot;
            gurrenInteractPurchaseInteraction = gurrenInteractPrefab.AddComponent<PurchaseInteraction>();
            gurrenInteractPurchaseInteraction.available = true;
            gurrenInteractPurchaseInteraction.displayNameToken = TTGL_SurvivorPlugin.developerPrefix + "_GURREN_INTERACTABLE_NAME";
            gurrenInteractPurchaseInteraction.contextToken = TTGL_SurvivorPlugin.developerPrefix + "_GURREN_INTERACTABLE_CONTEXT";
            //var summonMasterBehavior = gurrenInteractPrefab.AddComponent<SummonMasterBehavior>();
            //summonMasterBehavior.masterPrefab = CreateGurrenAIMaster();
            gurrenInteractPrefab.AddComponent<GurrenSpawnerBehavior>();
            gurrenInteractSpawnCard.prefab = gurrenInteractPrefab;
            gurrenInteractSpawnCard.sendOverNetwork = true;
            gurrenInteractSpawnCard.hullSize = HullClassification.Golem;
            gurrenInteractSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            gurrenInteractSpawnCard.requiredFlags = NodeFlags.None;
            gurrenInteractSpawnCard.forbiddenFlags = NodeFlags.None;
            gurrenInteractSpawnCard.directorCreditCost = 15;
            gurrenInteractSpawnCard.occupyPosition = true;
            gurrenInteractSpawnCard.eliteRules = SpawnCard.EliteRules.Default;
            gurrenInteractSpawnCard.orientToFloor = true;
            gurrenInteractSpawnCard.slightlyRandomizeOrientation = false;
            gurrenInteractSpawnCard.skipSpawnWhenSacrificeArtifactEnabled = false;
            if (gurrenInteractPrefab == null)
            {
                TTGL_SurvivorPlugin.instance.Logger.LogError("Could not load GurrenInteractPrefab");
            }
            PrefabAPI.RegisterNetworkPrefab(gurrenInteractPrefab);
            //TTGL_SurvivorPlugin.networkPrefabs.Add(gurrenInteractPrefab);
        }

        private static GameObject CreateGurrenAIMaster()
        {
            return Gurren.allyPrefab;
            //return LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/MegaDroneMaster");
        }
    }
}