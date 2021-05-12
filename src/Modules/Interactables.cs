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

namespace TTGL_Survivor.Modules
{
    internal static class Interactables
    {
        public static PurchaseInteraction gurrenInteractPurchaseInteraction;
        public static InteractableSpawnCard gurrenInteractSpawnCard;
        public static bool gurrenFound = false;

        internal static void RegisterInteractables()
        {
            CreateGurrenSummonInteractableSpawnCard();
            On.RoR2.SceneDirector.PopulateScene += SceneDirector_PopulateScene;
            On.RoR2.PurchaseInteraction.GetInteractability += PurchaseInteraction_GetInteractability;            
        }

        private static Interactability PurchaseInteraction_GetInteractability(On.RoR2.PurchaseInteraction.orig_GetInteractability orig, PurchaseInteraction self, Interactor activator)
        {
            Interactability result = orig(self, activator);
            if (self == gurrenInteractPurchaseInteraction)
            {
                TTGL_SurvivorPlugin.instance.Logger.LogMessage("PurchaseInteraction_GetInteractability result: " + result);
            }
            return result;
        }

        private static void SceneDirector_PopulateScene(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            if (!gurrenFound && gurrenInteractSpawnCard && Run.instance.userMasters.Values.Any((x) =>
            {
                if (x != null && x.bodyPrefab != null)
                {
                    var body = x.bodyPrefab.GetComponent<CharacterBody>();
                    if (body)
                    {
                        var found = body.bodyIndex == BodyCatalog.FindBodyIndex("LagannBody");
                        if (found)
                        {
                            TTGL_SurvivorPlugin.instance.Logger.LogMessage("LagannBody found");
                            return true;
                        }
                    }
                }
                TTGL_SurvivorPlugin.instance.Logger.LogMessage("LagannBody not found");
                return false;
            }))
            {
                self.directorCore.TrySpawnObject(new DirectorSpawnRequest(gurrenInteractSpawnCard, new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                }, self.rng));
            }
            orig(self);
        }
        

        private static void CreateGurrenSummonInteractableSpawnCard()
        {
            gurrenInteractSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();            
            var gurrenInteractPrefab = Modules.Assets.mainAssetBundle.LoadAsset<GameObject>("GurrenInteractPrefab");            
            gurrenInteractPrefab.AddComponent<NetworkIdentity>();
            var childLocator = gurrenInteractPrefab.GetComponent<ChildLocator>();
            var model = childLocator.FindChild("Model");
            var hologramPivot = childLocator.FindChild("HologramPivot");           
            var modelLocator = model.gameObject.AddComponent<ModelLocator>();
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
            var summonMasterBehavior = gurrenInteractPrefab.AddComponent<SummonMasterBehavior>();
            summonMasterBehavior.masterPrefab = CreateGurrenAIMaster();
            summonMasterBehavior.destroyAfterSummoning = false;
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
            
            TTGL_SurvivorPlugin.networkPrefabs.Add(gurrenInteractPrefab);
        }

        private static GameObject CreateGurrenAIMaster()
        {
            return Gurren.allyPrefab;
            //return Resources.Load<GameObject>("Prefabs/CharacterMasters/MegaDroneMaster");
        }
    }
}