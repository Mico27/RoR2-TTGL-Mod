using R2API;
using TTGL_Survivor.SkillStates;
using TTGL_Survivor.SkillStates.BaseStates;

namespace TTGL_Survivor.Modules
{
    public static class States
    {
        public static void RegisterStates()
        {
            LoadoutAPI.AddSkill(typeof(LagannMain));
            LoadoutAPI.AddSkill(typeof(BaseMeleeAttack));
            LoadoutAPI.AddSkill(typeof(DrillRush));
            LoadoutAPI.AddSkill(typeof(ShootRifle));
            LoadoutAPI.AddSkill(typeof(SpiralBurst));
            LoadoutAPI.AddSkill(typeof(LagannImpact));
        }
    }
}