using R2API;

namespace TTGL_Survivor.Modules
{
    internal static class Unlockables
    {
        internal static void RegisterUnlockables()
        {
            UnlockablesAPI.AddUnlockable<Achievements.LagannUnlockAchievement>(true);
        }
    }
}