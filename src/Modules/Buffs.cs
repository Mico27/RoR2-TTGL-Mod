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

        internal static void RegisterBuffs()
        {
            maxSpiralPowerBuff = AddNewBuff("MaxSpiralPowerBuff", "Textures/BuffIcons/texBuffGenericShield", Color.green, false, false);
            maxSpiralPowerDeBuff = AddNewBuff("MaxSpiralPowerDeBuff", "Textures/BuffIcons/texBuffGenericShield", Color.red, false, true);
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