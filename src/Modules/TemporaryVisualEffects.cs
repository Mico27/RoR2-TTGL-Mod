using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.Modules
{
    internal static class TemporaryVisualEffects
    {
        internal static GameObject gurrenLagannShadesBindingEffect;

        internal static void RegisterTemporaryVisualEffects()
        {
            CreateGurrenLagannShadesConstrictVisualEffect();

        }

        private static void CreateGurrenLagannShadesConstrictVisualEffect()
        {
            gurrenLagannShadesBindingEffect = Assets.mainAssetBundle.LoadAsset<GameObject>("GurrenLagannShadesBindingEffect");
            gurrenLagannShadesBindingEffect.AddComponent<GurrenLagannShadesConstrictComponent>();

        }

    }
}