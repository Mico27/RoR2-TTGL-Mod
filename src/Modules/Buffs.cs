using R2API;
using RoR2;
using UnityEngine;

namespace TTGL_Survivor.Modules
{
    public static class Buffs
    {
        // armor buff gained during roll
        internal static BuffIndex maxSpiralPowerBuff;
        internal static BuffIndex maxSpiralPowerDeBuff;
        internal static BuffIndex canopyBuff;

        internal static void RegisterBuffs()
        {
            maxSpiralPowerBuff = AddNewBuff("MaxSpiralPowerBuff", "@TTGL_Survivor:texBuffSpiralIcon", Color.green, false, false);
            maxSpiralPowerDeBuff = AddNewBuff("MaxSpiralPowerDeBuff", "@TTGL_Survivor:texBuffSpiralIcon", Color.red, false, true);
            canopyBuff = AddNewBuff("CanopyBuff", "@TTGL_Survivor:texCanopyBuffIcon", new Color(1.0f,0.8f,0.8f), false, false);
        }

        // simple helper method
        internal static BuffIndex AddNewBuff(string buffName, string iconPath, Color buffColor, bool canStack, bool isDebuff)
        {
            CustomBuff tempBuff = new CustomBuff(new BuffDef
            {
                name = buffName,
                iconPath = iconPath,
                buffColor = buffColor,
                canStack = canStack,
                isDebuff = isDebuff,
                eliteIndex = EliteIndex.None
            });

            return BuffAPI.Add(tempBuff);
        }
    }
}