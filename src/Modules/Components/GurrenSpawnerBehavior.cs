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
            this.summonMasterBehavior = base.GetComponent<SummonMasterBehavior>();
            this.purchaseInteraction = base.GetComponent<PurchaseInteraction>();
            if (this.purchaseInteraction)
            {
                //this.purchaseInteraction.costType = CostTypeIndex.Money;
                this.purchaseInteraction.costType = (CostTypeIndex)CostTypeDefs.getCostTypeIndex(CostTypeDefs.costTypeDefGurrenSummon);
                this.purchaseInteraction.cost = 5;
                this.purchaseInteraction.Networkcost = 5;
                this.purchaseInteraction.onPurchase.AddListener(delegate (Interactor interactor)
                {
                    this.SpawnGurrenMinion(interactor);
                });
            }
        }

        [ClientRpc]
        public void RpcSpawnGurrenMinionClient()
        {
            TTGL_SurvivorPlugin.instance.Logger.LogMessage("RpcSpawnGurrenMinionClient called");
            Interactables.gurrenFound = true;
            this.AnimateMounting();
            OnGurrenSpawned();
        }

        public void SpawnGurrenMinion(Interactor interactor)
        {
            this.CallRpcSpawnGurrenMinionClient();
            bool flag = !NetworkServer.active;
            if (flag)
            {
                Debug.LogWarning("[Server] function 'System.Void TTGL_Survivor.Modules.Components.GurrenSpawnerBehavior::SpawnGurrenMinion()' called on client");
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
                Interactables.gurrenFound = true;
                this.AnimateMounting();
                OnGurrenSpawned();
                StartCoroutine(DelaySummon(interactor));                
            }
        }

        IEnumerator DelaySummon(Interactor interactor)
        {
            yield return new WaitForSeconds(6f);
            if (NetworkServer.active && interactor && this.summonMasterBehavior)
            {
                if (this.summonMasterBehavior)
                {
                    NetworkServer.Destroy(base.gameObject);
                    this.summonMasterBehavior.OpenSummon(interactor);                   
                }
            }
        }

        private void AnimateMounting()
        {
            if (this.animator)
            {
                int layerIndex = this.animator.GetLayerIndex("Base Layer");
                this.animator.speed = 1f;
                this.animator.Update(0f);
                this.animator.PlayInFixedTime("GURREN_Interact_Activate", layerIndex, 0f);
            }
        }

        private void UNetVersion()
        {
        }
        
        public void CallRpcSpawnGurrenMinionClient()
        {
            if (!NetworkServer.active)
            {
                Debug.LogError("RPC Function RpcSpawnGurrenMinionClient called on client.");
                return;
            }
            NetworkWriter networkWriter = new NetworkWriter();
            networkWriter.Write(0);
            networkWriter.Write((short)((ushort)2));
            networkWriter.WritePackedUInt32((uint)GurrenSpawnerBehavior.kRpcRpcSpawnGurrenMinionClient);
            networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
            this.SendRPCInternal(networkWriter, 0, "RpcSpawnGurrenMinionClient");
        }

        static GurrenSpawnerBehavior()
        {
            NetworkBehaviour.RegisterRpcDelegate(typeof(GurrenSpawnerBehavior), GurrenSpawnerBehavior.kRpcRpcSpawnGurrenMinionClient, new NetworkBehaviour.CmdDelegate(GurrenSpawnerBehavior.InvokeRpcRpcSpawnGurrenMinionClient));
            NetworkCRC.RegisterBehaviour("GurrenSpawnerBehavior", 0);
            GurrenSpawnerBehavior.kRpcRpcSpawnGurrenMinionClient = 2035029027;
        }

        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            return false;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
        }

        protected static void InvokeRpcRpcSpawnGurrenMinionClient(NetworkBehaviour obj, NetworkReader reader)
        {
            if (!NetworkClient.active)
            {
                Debug.LogError("RPC RpcSpawnGurrenMinionClient called on server.");
                return;
            }
            ((GurrenSpawnerBehavior)obj).RpcSpawnGurrenMinionClient();
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

        public SummonMasterBehavior summonMasterBehavior;

        public Animator animator;

        private static int kRpcRpcSpawnGurrenMinionClient = 2035029027;

    }
}