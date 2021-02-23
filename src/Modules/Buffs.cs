using R2API;
using RoR2;
using UnityEngine;

namespace TTGL_Survivor.Modules
{
    public static class Buffs
    {
        // armor buff gained during roll
        internal static BuffIndex kaminaSpiritBuff;

        internal static void RegisterBuffs()
        {
            kaminaSpiritBuff = AddNewBuff("KaminaSpiritBuff", "Textures/BuffIcons/texBuffGenericShield", Color.green, false, false);
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