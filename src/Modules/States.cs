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

            TTGL_SurvivorPlugin.entityStates.Add(typeof(LagannMain));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(LagannDrillRush));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(YokoShootRifle));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(YokoExplosiveRifle));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(YokoScepterRifle));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(LagannSpiralBurst));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(LagannToggleCanopy));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(PrepareLagannImpact));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(LagannImpact));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(AimLagannImpact));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(LagannCombine));

            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannMain));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannSpiralingCombo));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannBaseCombo));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannHookPunch));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannInsideCrescentKick));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannLegSweep));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannMartelo2));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannMmaKick));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannSpiralingCombo));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannStabbingLeft));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannStabbingRight));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannThrustSlash));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannUppercut));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannUpwardThrust)); 
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannThrowingShades));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannTornadoKick));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannGigaDrillMaximum));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannGigaDrillBreak));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannShadesConstrictState));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLagannSplit));

            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenMain));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenTripleSlash));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenRoll));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenHoldBoulder));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenLiftBoulder));
            TTGL_SurvivorPlugin.entityStates.Add(typeof(GurrenThrowBoulder));
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
