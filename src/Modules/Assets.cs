using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using System.IO;
using RoR2.Audio;
using System.Collections.Generic;
using System;
using R2API;

namespace TTGL_Survivor.Modules
{
    internal static class Assets
    {
        // the assetbundle to load assets from
        internal static AssetBundle mainAssetBundle;

        // particle effects
        internal static GameObject punchImpactEffect;
        internal static GameObject yokoRifleHitSmallEffect;
        internal static GameObject yokoRifleMuzzleBigEffect;
        internal static GameObject yokoRifleMuzzleSmallEffect;
        internal static GameObject yokoRifleExplosiveRoundExplosion;
        internal static GameObject specialExplosion;

        internal static NetworkSoundEventDef fullBuffPlaySoundEvent;
        internal static NetworkSoundEventDef genericHitSoundEvent;
        internal static NetworkSoundEventDef lagannImpactFireSoundEvent;
        internal static NetworkSoundEventDef drillRushHitSoundEvent;
        internal static NetworkSoundEventDef fullBuffResumeSoundEvent;
        internal static NetworkSoundEventDef fullBuffPauseSoundEvent;
        internal static NetworkSoundEventDef fullBuffStopSoundEvent;
        internal static NetworkSoundEventDef tokoRifleFireSoundEvent;
        internal static NetworkSoundEventDef tokoRifleCritSoundEvent;
        internal static NetworkSoundEventDef gigaDrillBreakSoundEvent;

        // cache these and use to create our own materials
        public static Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/HGStandard");
        public static Material commandoMat;

        internal static void PopulateAssets()
        {
            if (mainAssetBundle == null)
            {
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TTGL_Survivor.ttglsurvivorbundle"))
                {
                    mainAssetBundle = AssetBundle.LoadFromStream(assetStream);                    
                }
            }
                        
            using (Stream manifestResourceStream3 = Assembly.GetExecutingAssembly().GetManifestResourceStream("TTGL_Survivor.TTGLSoundbank.bnk"))
            {
                byte[] array = new byte[manifestResourceStream3.Length];
                manifestResourceStream3.Read(array, 0, array.Length);
                SoundAPI.SoundBanks.Add(array);
                //MusicTrackOverride
            }
            fullBuffPlaySoundEvent = CreateNetworkSoundEventDef("TTGLFullBuffPlay");
            genericHitSoundEvent = CreateNetworkSoundEventDef("TTGLGenericHit");
            lagannImpactFireSoundEvent = CreateNetworkSoundEventDef("TTGLLagannImpactFire");
            drillRushHitSoundEvent = CreateNetworkSoundEventDef("TTGLDrillRushHit");
            fullBuffResumeSoundEvent = CreateNetworkSoundEventDef("TTGLFullBuffResume");
            fullBuffPauseSoundEvent = CreateNetworkSoundEventDef("TTGLFullBuffPause");
            fullBuffStopSoundEvent = CreateNetworkSoundEventDef("TTGLFullBuffStop");
            tokoRifleFireSoundEvent = CreateNetworkSoundEventDef("TTGLTokoRifleFire");
            tokoRifleCritSoundEvent = CreateNetworkSoundEventDef("TTGLTokoRifleCrit");
            gigaDrillBreakSoundEvent = CreateNetworkSoundEventDef("TTGLGigaDrillBreak");

            punchImpactEffect = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniImpactVFXLoader"), "TTGLImpactPunch");
            punchImpactEffect.AddComponent<NetworkIdentity>();

            TTGL_SurvivorPlugin.effectDefs.Add(new EffectDef()
            {
                prefab = punchImpactEffect,
                prefabEffectComponent = punchImpactEffect.GetComponent<EffectComponent>(),
                prefabVfxAttributes = punchImpactEffect.GetComponent<VFXAttributes>(),
                prefabName = punchImpactEffect.name,
                spawnSoundEventName = punchImpactEffect.GetComponent<EffectComponent>().soundName
            });

            yokoRifleHitSmallEffect = Assets.LoadEffect("YokoRifleHitSmallEffect", 1.0f);
            yokoRifleMuzzleBigEffect = Assets.LoadEffect("YokoRifleMuzzleBigEffect", 1.0f);
            yokoRifleMuzzleSmallEffect = Assets.LoadEffect("YokoRifleMuzzleSmallEffect", 1.0f);
            yokoRifleExplosiveRoundExplosion = Assets.LoadEffect("YokoRifleExplosiveRoundExplosion", 1.0f);
            specialExplosion = Assets.LoadEffect("SpecialExplosion", 5.0f);
        }

        internal static NetworkSoundEventDef CreateNetworkSoundEventDef(string eventName)
        {
            NetworkSoundEventDef networkSoundEventDef = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            networkSoundEventDef.akId = AkSoundEngine.GetIDFromString(eventName);
            networkSoundEventDef.eventName = eventName;
            TTGL_SurvivorPlugin.networkSoundEventDefs.Add(networkSoundEventDef);
            return networkSoundEventDef;
        }

        internal static void ConvertAllRenderersToHopooShader(GameObject objectToConvert)
        {
            foreach (Renderer i in objectToConvert.GetComponentsInChildren<Renderer>())
            {
                if (i)
                {
                    if (i.material)
                    {
                        i.material.shader = hotpoo;
                    }
                }
            }
        }

        private static GameObject LoadEffect(string resourceName, float duration)
        {
            GameObject newEffect = mainAssetBundle.LoadAsset<GameObject>(resourceName);

            newEffect.AddComponent<DestroyOnTimer>().duration = duration;
            newEffect.AddComponent<NetworkIdentity>();
            var vfx = newEffect.AddComponent<VFXAttributes>();
            vfx.vfxPriority = VFXAttributes.VFXPriority.Always;
            var effect = newEffect.AddComponent<EffectComponent>();
            effect.applyScale = false;
            effect.effectIndex = EffectIndex.Invalid;
            effect.parentToReferencedTransform = true;
            effect.positionAtReferencedTransform = true;

            TTGL_SurvivorPlugin.effectDefs.Add(new EffectDef()
            {
                prefab = newEffect,
                prefabEffectComponent = effect,
                prefabVfxAttributes = vfx,
                prefabName = newEffect.name,
                spawnSoundEventName = effect.soundName
            });

            return newEffect;
        }

    }
}