using EntityStates;
using MonoMod.RuntimeDetour;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using TTGL_Survivor.SkillStates;
namespace TTGL_Survivor.Modules
{
    public static class States
    {
        private static Hook set_stateTypeHook;
        private static Hook set_typeNameHook;
        private static readonly BindingFlags allFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        private delegate void set_stateTypeDelegate(ref SerializableEntityStateType self, Type value);
        private delegate void set_typeNameDelegate(ref SerializableEntityStateType self, String value);

        public static void RegisterStates()
        {
            Type type = typeof(SerializableEntityStateType);
            HookConfig cfg = default;
            cfg.Priority = Int32.MinValue;
            set_stateTypeHook = new Hook(type.GetMethod("set_stateType", allFlags), new set_stateTypeDelegate(SetStateTypeHook), cfg);
            set_typeNameHook = new Hook(type.GetMethod("set_typeName", allFlags), new set_typeNameDelegate(SetTypeName), cfg);

            ContentPacks.entityStates.Add(typeof(LagannMain));
            ContentPacks.entityStates.Add(typeof(LagannDrillRush));
            ContentPacks.entityStates.Add(typeof(YokoShootRifle));
            ContentPacks.entityStates.Add(typeof(YokoExplosiveRifle));
            ContentPacks.entityStates.Add(typeof(YokoScepterRifle));
            ContentPacks.entityStates.Add(typeof(LagannSpiralBurst));
            ContentPacks.entityStates.Add(typeof(LagannToggleCanopy));
            ContentPacks.entityStates.Add(typeof(PrepareLagannImpact));
            ContentPacks.entityStates.Add(typeof(LagannImpact));
            ContentPacks.entityStates.Add(typeof(AimLagannImpact));

            ContentPacks.entityStates.Add(typeof(GurrenLagannMain));
            ContentPacks.entityStates.Add(typeof(GurrenLagannSpiralingCombo));
            ContentPacks.entityStates.Add(typeof(GurrenLagannThrowingShades));
            ContentPacks.entityStates.Add(typeof(GurrenLagannTornadoKick));
            ContentPacks.entityStates.Add(typeof(GurrenLagannGigaDrillMaximum));
            ContentPacks.entityStates.Add(typeof(GurrenLagannGigaDrillBreak));
        }

        private static void SetStateTypeHook(ref this SerializableEntityStateType self, Type value)
        {
            self._typeName = value.AssemblyQualifiedName;
        }

        private static void SetTypeName(ref this SerializableEntityStateType self, String value)
        {
            Type t = GetTypeFromName(value);
            if (t != null)
            {
                self.SetStateTypeHook(t);
            }
        }

        private static Type GetTypeFromName(String name)
        {
            Type[] types = EntityStateCatalog.stateIndexToType;
            return Type.GetType(name);
        }
    }
}
