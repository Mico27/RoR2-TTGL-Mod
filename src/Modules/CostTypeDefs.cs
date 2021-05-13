using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Hologram;
using TTGL_Survivor.Modules.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.Modules
{
    public static class CostTypeDefs
    {
        public static CostTypeDef costTypeDefGurrenSummon;

        static CostTypeDefs()
        {
            IL.RoR2.CostTypeCatalog.GetCostTypeDef += CostTypeCatalog_GetCostTypeDef;
            CostTypeCatalog.modHelper.getAdditionalEntries += delegate (List<CostTypeDef> list)
            {
                list.AddRange(CostTypeDefs.costTypeDefs);
            };
        }

        internal static void RegisterCostTypeDefs()
        {
            CreateCostDefGurrenSummon();
        }

        private static void CostTypeCatalog_GetCostTypeDef(ILContext il)
        {
            ILCursor ilcursor = new ILCursor(il);
            ILCursor ilcursor2 = ilcursor;
            Func<Instruction, bool>[] array = new Func<Instruction, bool>[1];
            array[0] = ((Instruction x) => ILPatternMatchingExt.MatchLdcI4(x, 13));
            bool flag = ilcursor2.TryGotoNext(array);
            bool flag2 = flag;
            if (flag2)
            {
                ilcursor.Remove();
                ilcursor.Emit(OpCodes.Call, typeof(CostTypeCatalog).GetProperty("costTypeCount").GetGetMethod());
            }
        }

        public static int getCostTypeIndex(CostTypeDef costTypeDef)
        {
            return Array.IndexOf<CostTypeDef>(CostTypeCatalog.costTypeDefs, costTypeDef);
        }

        public static void Add(CostTypeDef costTypeDef)
        {
            CostTypeDefs.costTypeDefs.Add(costTypeDef);
        }

        private static void CreateCostDefGurrenSummon()
        {
            costTypeDefGurrenSummon = new CostTypeDef();
            costTypeDefGurrenSummon.costStringFormatToken = "COST_LUNARCOIN_FORMAT";
            costTypeDefGurrenSummon.isAffordable = delegate (CostTypeDef costTypeDef, CostTypeDef.IsAffordableContext context)
            {
                CharacterBody characterBody = context.activator.GetComponent<CharacterBody>();
                bool result = false;
                if (characterBody && characterBody.bodyIndex == BodyCatalog.FindBodyIndex("LagannBody"))
                {
                    var master = characterBody.master;
                    if (master)
                    {
                        var gurrenMinionCache = GurrenMinionCache.GetOrSetGurrenStatusCache(master);
                        NetworkUser networkUser = Util.LookUpBodyNetworkUser(context.activator.gameObject);
                        result = (networkUser &&
                        (ulong)networkUser.lunarCoins >= (ulong)((long)context.cost) &&
                        !gurrenMinionCache.gurrenMinion);
                    }
                }                
                return result;
            };
            costTypeDefGurrenSummon.payCost = delegate (CostTypeDef costTypeDef, CostTypeDef.PayCostContext context)
            {
                NetworkUser networkUser = Util.LookUpBodyNetworkUser(context.activator.gameObject);
                bool flag = networkUser;
                if (flag)
                {
                    networkUser.DeductLunarCoins((uint)context.cost);
                }
            };
            costTypeDefGurrenSummon.colorIndex = ColorCatalog.ColorIndex.LunarCoin;
            CostTypeDefs.Add(costTypeDefGurrenSummon);
        }

        internal static List<CostTypeDef> costTypeDefs = new List<CostTypeDef>();
    }
}