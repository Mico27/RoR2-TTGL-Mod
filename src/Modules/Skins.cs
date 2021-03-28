using RoR2;
using System;
using UnityEngine;

namespace TTGL_Survivor.Modules
{
    public static class Skins
    {
        public static RoR2.SkinDef CreateSkinDef(string skinName, Sprite skinIcon, RoR2.CharacterModel.RendererInfo[] rendererInfos, SkinnedMeshRenderer mainRenderer, GameObject root)
        {
            return Skins.CreateSkinDef(skinName, skinIcon, rendererInfos, mainRenderer, root, "");
        }

        public static RoR2.SkinDef CreateSkinDef(string skinName, Sprite skinIcon, RoR2.CharacterModel.RendererInfo[] rendererInfos, SkinnedMeshRenderer mainRenderer, GameObject root, string unlockName)
        {
            On.RoR2.SkinDef.Awake += Skins.DoNothing;
            RoR2.SkinDef skinDef = ScriptableObject.CreateInstance<RoR2.SkinDef>();
            skinDef.baseSkins = Array.Empty<RoR2.SkinDef>();
            skinDef.icon = skinIcon;
            skinDef.unlockableName = unlockName;
            skinDef.rootObject = root;
            skinDef.rendererInfos = rendererInfos;
            skinDef.gameObjectActivations = new RoR2.SkinDef.GameObjectActivation[0];
            skinDef.meshReplacements = new RoR2.SkinDef.MeshReplacement[0];
            skinDef.projectileGhostReplacements = new RoR2.SkinDef.ProjectileGhostReplacement[0];
            skinDef.minionSkinReplacements = new RoR2.SkinDef.MinionSkinReplacement[0];
            skinDef.nameToken = skinName;
            skinDef.name = skinName;
            On.RoR2.SkinDef.Awake -= Skins.DoNothing;
            return skinDef;
        }

        private static void DoNothing(On.RoR2.SkinDef.orig_Awake orig, RoR2.SkinDef self)
        {
        }
    }
}