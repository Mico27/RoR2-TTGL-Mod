using EntityStates;
using EntityStates.Huntress;
using EntityStates.Loader;
using RoR2;
using RoR2.Projectile;
using TTGL_Survivor.Modules;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenLagannSpiralConversion : BaseSpiralConversion
    {        
        protected override void PlayEnterAnimation()
        {
            base.PlayAnimation("FullBody, Override", "GurrenLagannSpiralConversion");
        }

        protected override void PlayExitAnimation()
        {
            base.PlayAnimation("FullBody, Override", "GurrenLagannSpiralConversionExit");
        }

    }
}