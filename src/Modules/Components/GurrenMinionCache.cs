using System;
using RoR2;
using TTGL_Survivor.Modules.Survivors;
using TTGL_Survivor.SkillStates;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.Modules.Components
{
    public class GurrenMinionCache : MonoBehaviour
    {
        public CharacterMaster gurrenMinion;
        private CharacterMaster owner;

        public static GurrenMinionCache GetOrSetGurrenStatusCache(CharacterMaster owner)
        {
            var existingCache = owner.gameObject.GetComponent<GurrenMinionCache>();
            if (!existingCache)
            {
                existingCache = owner.gameObject.AddComponent<GurrenMinionCache>();
                existingCache.owner = owner;
                existingCache.SearchListForMinion();
            }            
            return existingCache;
        }

        public void Awake()
        {
            On.RoR2.CharacterMaster.OnDestroy += CharacterMaster_OnDestroy;
            CharacterMaster.onStartGlobal += CharacterMaster_onStartGlobal;
        }


        public void OnDestroy()
        {
            On.RoR2.CharacterMaster.OnDestroy -= CharacterMaster_OnDestroy;
            CharacterMaster.onStartGlobal -= CharacterMaster_onStartGlobal;
        }

        private void CharacterMaster_OnDestroy(On.RoR2.CharacterMaster.orig_OnDestroy orig, CharacterMaster self)
        {
            if (gurrenMinion == self)
            {
                gurrenMinion = null;
            }
            orig(self);
        }

        private void CharacterMaster_onStartGlobal(CharacterMaster obj)
        {
            AssignMinion(obj);
        }

        private void SearchListForMinion()
        {
            if (this.owner)
            {
                var players = TeamComponent.GetTeamMembers(TeamIndex.Player);
                foreach (var player in players)
                {
                    if (player.body && player.body.master)
                    {
                        AssignMinion(player.body.master);
                    }
                }
            }
        }

        private void AssignMinion(CharacterMaster obj)
        {
            if (obj.masterIndex == MasterCatalog.FindMasterIndex("GurrenAllyMaster") &&
                obj.minionOwnership &&
                obj.minionOwnership.ownerMaster == this.owner)
            {
                gurrenMinion = obj;
            }
        }
    }
}