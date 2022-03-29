using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TTGL_Survivor.Modules
{
    public static class Buffs
    {
        public const float kaminaBuffDmgModifier = 0.15f;
        // armor buff gained during roll
        internal static BuffDef maxSpiralPowerBuff;
        internal static BuffDef maxSpiralPowerDeBuff;
        internal static BuffDef canopyBuff;
        internal static BuffDef kaminaBuff;

        internal static void RegisterBuffs()
        {
            maxSpiralPowerBuff = AddNewBuff("MaxSpiralPowerBuff", "texBuffSpiralIcon", Color.green, false, false, false);
            maxSpiralPowerDeBuff = AddNewBuff("MaxSpiralPowerDeBuff", "texBuffSpiralIcon", Color.red, false, true, true);
            canopyBuff = AddNewBuff("CanopyBuff", "texCanopyBuffIcon", new Color(1.0f,0.8f,0.8f), false, false, false);
            kaminaBuff = AddNewBuff("KaminaBuff", "texKaminaBuffIcon", Color.white, true, false, false);
        }

        // simple helper method
        internal static BuffDef AddNewBuff(string buffName, string iconPath, Color buffColor, bool canStack, bool isDebuff, bool isHidden)
        {
            var buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = buffName;
            buffDef.iconSprite = Modules.Assets.LoadAsset<Sprite>(iconPath);
            buffDef.buffColor = buffColor;
            buffDef.canStack = canStack;
            buffDef.isDebuff = isDebuff;
            buffDef.isHidden = isHidden;
            TTGL_SurvivorPlugin.buffDefs.Add(buffDef);            
            return buffDef;
        }
    }
}