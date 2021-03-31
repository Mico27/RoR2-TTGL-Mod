using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TTGL_Survivor.Modules
{
    public static class Buffs
    {
        // armor buff gained during roll
        internal static BuffDef maxSpiralPowerBuff;
        internal static BuffDef maxSpiralPowerDeBuff;
        internal static BuffDef canopyBuff;

        internal static void RegisterBuffs()
        {
            maxSpiralPowerBuff = AddNewBuff("MaxSpiralPowerBuff", "texBuffSpiralIcon", Color.green, false, false);
            maxSpiralPowerDeBuff = AddNewBuff("MaxSpiralPowerDeBuff", "texBuffSpiralIcon", Color.red, false, true);
            canopyBuff = AddNewBuff("CanopyBuff", "texCanopyBuffIcon", new Color(1.0f,0.8f,0.8f), false, false);
            
            On.RoR2.BuffCatalog.Init += BuffCatalog_Init;
        }

        private static void BuffCatalog_Init(On.RoR2.BuffCatalog.orig_Init orig)
        {
            BuffDef[] array = ContentManager.buffDefs;
            BuffCatalog.SetBuffDefs(array);
        }

        // simple helper method
        internal static BuffDef AddNewBuff(string buffName, string iconPath, Color buffColor, bool canStack, bool isDebuff)
        {
            var iconSprite = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>(iconPath);
            if (iconSprite)
            {
                TTGL_SurvivorPlugin.instance.Logger.LogMessage("buffSprite: " + iconSprite.name);
            }
            var buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = buffName;
            buffDef.iconSprite = iconSprite;
            buffDef.buffColor = buffColor;
            buffDef.canStack = canStack;
            buffDef.isDebuff = isDebuff;
            ContentPacks.buffDefs.Add(buffDef);            
            return buffDef;
        }
    }
}