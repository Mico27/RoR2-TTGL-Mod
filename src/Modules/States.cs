using R2API;
using TTGL_Survivor.SkillStates;

namespace TTGL_Survivor.Modules
{
    public static class States
    {
        public static void RegisterStates()
        {
            LoadoutAPI.AddSkill(typeof(LagannMain));
            LoadoutAPI.AddSkill(typeof(LagannDrillRush));
            LoadoutAPI.AddSkill(typeof(YokoShootRifle));
            LoadoutAPI.AddSkill(typeof(YokoExplosiveRifle));
            LoadoutAPI.AddSkill(typeof(YokoScepterRifle));
            LoadoutAPI.AddSkill(typeof(LagannSpiralBurst));
            LoadoutAPI.AddSkill(typeof(LagannToggleCanopy));
            LoadoutAPI.AddSkill(typeof(PrepareLagannImpact));

            LoadoutAPI.AddSkill(typeof(GurrenLagannMain));
            LoadoutAPI.AddSkill(typeof(GurrenLagannSpiralingCombo));
            LoadoutAPI.AddSkill(typeof(GurrenLagannThrowingShades));
            LoadoutAPI.AddSkill(typeof(GurrenLagannTornadoKick));
            LoadoutAPI.AddSkill(typeof(GurrenLagannGigaDrillMaximum));
            LoadoutAPI.AddSkill(typeof(GurrenLagannGigaDrillBreak));
        }
    }
}