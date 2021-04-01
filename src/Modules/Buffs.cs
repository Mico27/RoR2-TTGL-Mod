using Mono.Cecil.Cil;
using MonoMod.Cil;
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
            IL.RoR2.BuffCatalog.Init += FixBuffCatalog;
            maxSpiralPowerBuff = AddNewBuff("MaxSpiralPowerBuff", "texBuffSpiralIcon", Color.green, false, false);
            maxSpiralPowerDeBuff = AddNewBuff("MaxSpiralPowerDeBuff", "texBuffSpiralIcon", Color.red, false, true);
            canopyBuff = AddNewBuff("CanopyBuff", "texCanopyBuffIcon", new Color(1.0f,0.8f,0.8f), false, false);            
        }

        internal static void FixBuffCatalog(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                if (!c.Next.MatchLdsfld(typeof(RoR2Content.Buffs), nameof(RoR2Content.Buffs.buffDefs)))
                {
                    return;
                }
                c.Remove();
                c.Emit(OpCodes.Ldsfld, typeof(ContentManager).GetField(nameof(ContentManager.buffDefs)));
            }
            catch
            {

            }
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