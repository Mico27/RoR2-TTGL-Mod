using System;
using System.Collections.Generic;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace TTGL_Survivor.Modules
{
    // Token: 0x0200004C RID: 76
    public class ContentPacks
    {
        // Token: 0x060001A1 RID: 417 RVA: 0x0000BEBC File Offset: 0x0000A0BC
        public static void CreateContentPack()
        {
            ContentPacks.contentPack = new RoR2.ContentPack
            {
                artifactDefs = new RoR2.ArtifactDef[0],
                bodyPrefabs = bodyPrefabs.ToArray(),
                buffDefs = buffDefs.ToArray(),
                effectDefs = effectDefs.ToArray(),
                eliteDefs = new RoR2.EliteDef[0],
                entityStateConfigurations = new RoR2.EntityStateConfiguration[0],
                entityStateTypes = entityStates.ToArray(),
                equipmentDefs = new RoR2.EquipmentDef[0],
                gameEndingDefs = new RoR2.GameEndingDef[0],
                gameModePrefabs = new RoR2.Run[0],
                itemDefs = new RoR2.ItemDef[0],
                masterPrefabs = masterPrefabs.ToArray(),
                musicTrackDefs = new RoR2.MusicTrackDef[0],
                networkedObjectPrefabs = new GameObject[0],
                networkSoundEventDefs = networkSoundEventDefs.ToArray(),
                projectilePrefabs = projectilePrefabs.ToArray(),
                sceneDefs = new RoR2.SceneDef[0],
                skillDefs = skillDefs.ToArray(),
                skillFamilies = skillFamilies.ToArray(),
                surfaceDefs = new RoR2.SurfaceDef[0],
                survivorDefs = survivorDefinitions.ToArray(),
                unlockableDefs = new RoR2.UnlockableDef[0]
            };
            On.RoR2.ContentManager.SetContentPacks += AddContent;
        }

        // Token: 0x060001A2 RID: 418 RVA: 0x0000C018 File Offset: 0x0000A218
        private static void AddContent(On.RoR2.ContentManager.orig_SetContentPacks orig, List<RoR2.ContentPack> newContentPacks)
        {
            newContentPacks.Add(ContentPacks.contentPack);
            orig(newContentPacks);
        }

        // Token: 0x040001A7 RID: 423
        internal static RoR2.ContentPack contentPack;

        internal static List<BuffDef> buffDefs = new List<BuffDef>();
        internal static List<SurvivorDef> survivorDefinitions = new List<SurvivorDef>();
        internal static List<GameObject> bodyPrefabs = new List<GameObject>();
        internal static List<GameObject> masterPrefabs = new List<GameObject>();
        internal static List<GameObject> projectilePrefabs = new List<GameObject>();
        internal static List<EffectDef> effectDefs = new List<EffectDef>();
        internal static List<NetworkSoundEventDef> networkSoundEventDefs = new List<NetworkSoundEventDef>();
        internal static List<SkillFamily> skillFamilies = new List<SkillFamily>();
        internal static List<SkillDef> skillDefs = new List<SkillDef>();
        internal static List<Type> entityStates = new List<Type>();
    }
}
