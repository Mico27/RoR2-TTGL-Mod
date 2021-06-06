using System;
using System.Collections;
using RoR2;
using TTGL_Survivor.Modules.Survivors;
using TTGL_Survivor.SkillStates;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.Modules.Components
{
    [RequireComponent(typeof(PurchaseInteraction))]
    public class GurrenSpawnerBehavior : NetworkBehaviour
    {
        public static event Action onGurrenSpawnedGlobal;

        public void Awake()
        {
            this.animator = base.GetComponentInChildren<Animator>();
            //this.summonMasterBehavior = base.GetComponent<SummonMasterBehavior>();
            this.purchaseInteraction = base.GetComponent<PurchaseInteraction>();
            if (this.purchaseInteraction)
            {
                //this.purchaseInteraction.costType = CostTypeIndex.Money;
                this.purchaseInteraction.costType = (CostTypeIndex)CostTypeDefs.getCostTypeIndex(CostTypeDefs.costTypeDefGurrenSummon);
                this.purchaseInteraction.cost = 2;
                this.purchaseInteraction.Networkcost = 2;
                this.purchaseInteraction.onPurchase.AddListener(delegate (Interactor interactor)
                {
                    this.SpawnGurrenMinion(interactor);
                });
            }
        }

        public void SpawnGurrenMinion(Interactor interactor)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            else
            {                
                CharacterBody characterBody = interactor.GetComponent<CharacterBody>();
                if (characterBody)
                {
                    //Spawn gurren
                    Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                    {
                        subjectAsCharacterBody = characterBody,
                        baseToken = TTGL_SurvivorPlugin.developerPrefix + "_GURREN_SPAWNER_USE_MESSAGE"
                    });
                }
                OnGurrenSpawned();
                SpawnGurren(interactor);
                NetworkServer.Destroy(base.gameObject);
                //TODO set a starting animation for when the summon is done.
            }
        }
        private void SpawnGurren(Interactor interactor)
        {
            var characterBody = interactor.GetComponent<CharacterBody>();
            float d = 0f;
            CharacterMaster characterMaster = new MasterSummon
            {
                masterPrefab = Gurren.allyPrefab,
                position = base.transform.position + Vector3.up * d,
                rotation = base.transform.rotation,
                summonerBodyObject = characterBody.gameObject,
                ignoreTeamMemberLimit = true,
                useAmbientLevel = new bool?(true),
                inventoryToCopy = characterBody.inventory,
            }.Perform();
        }

        private void UNetVersion()
        {
        }
        
        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            return false;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
        }
        
        private void OnGurrenSpawned()
        {
            Action action = onGurrenSpawnedGlobal;
            if (action == null)
            {
                return;
            }
            action();
        }

        public PurchaseInteraction purchaseInteraction;
        
        public Animator animator;

        private static int kRpcRpcSpawnGurrenMinionClient = 2035029027;

    }
}