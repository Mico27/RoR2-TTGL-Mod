using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using System.IO;
using RoR2.Audio;
using System.Collections.Generic;
using System;
using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace TTGL_Survivor.Modules
{
    public static class Assets
    {
        public static string ResourcesPath => System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        // the assetbundle to load assets from
        internal static uint soundBankId;

        // particle effects
        internal static GameObject punchImpactEffect;
        internal static GameObject yokoRifleBeamEffect;
        internal static GameObject yokoRifleHitSmallEffect;
        internal static GameObject yokoRifleMuzzleBigEffect;
        internal static GameObject yokoRifleMuzzleSmallEffect;
        internal static GameObject yokoRifleExplosiveRoundExplosion;
        internal static GameObject gurrenBrokenBoulderEffect;
        internal static GameObject specialExplosion;
        internal static GameObject drillPopEffect;
        internal static GameObject earthMoundEffect;

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
        public static Shader hotpoo = LegacyResourcesAPI.Load<Shader>("Shaders/Deferred/HGStandard");
        public static Material commandoMat;

        internal static void PopulateAssets()
        {
            //if (mainAssetBundle == null)
            //{
            //using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TTGL_Survivor.ttglsurvivorbundle"))
            //{
            //mainAssetBundle = AssetBundle.LoadFromStream(assetStream);                    
            //}
            //}

            var test = System.IO.Path.Combine(ResourcesPath, "Resources", "catalog_2022.03.28.04.16.55.json");
            Addressables.LoadContentCatalogAsync(test, true).WaitForCompletion();


            using (Stream manifestResourceStream3 = Assembly.GetExecutingAssembly().GetManifestResourceStream("TTGL_Survivor.TTGLSoundbank.bnk"))
            {
                byte[] array = new byte[manifestResourceStream3.Length];
                manifestResourceStream3.Read(array, 0, array.Length);
                soundBankId = SoundAPI.SoundBanks.Add(array);
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

            var omniImpactVFXLoader = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniImpactVFXLoader");
            if (omniImpactVFXLoader == null)
            {
                TTGL_SurvivorPlugin.instance.Logger.LogError("Could not load Prefabs/Effects/OmniEffect/OmniImpactVFXLoader");
            }
            punchImpactEffect = PrefabAPI.InstantiateClone(omniImpactVFXLoader, "TTGLImpactPunch");
            punchImpactEffect.AddComponent<NetworkIdentity>();

            TTGL_SurvivorPlugin.effectDefs.Add(new EffectDef()
            {
                prefab = punchImpactEffect,
                prefabEffectComponent = punchImpactEffect.GetComponent<EffectComponent>(),
                prefabVfxAttributes = punchImpactEffect.GetComponent<VFXAttributes>(),
                prefabName = punchImpactEffect.name,
                spawnSoundEventName = punchImpactEffect.GetComponent<EffectComponent>().soundName
            });
            var tracerHuntressSnipe = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerHuntressSnipe");
            if (tracerHuntressSnipe == null)
            {
                TTGL_SurvivorPlugin.instance.Logger.LogError("Could not load Prefabs/Effects/Tracers/TracerHuntressSnipe");
            }
            yokoRifleBeamEffect = PrefabAPI.InstantiateClone(tracerHuntressSnipe, "TTGLYokoRifleBeamEffect");
            yokoRifleBeamEffect.AddComponent<NetworkIdentity>();
            yokoRifleBeamEffect.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            TTGL_SurvivorPlugin.effectDefs.Add(new EffectDef()
            {
                prefab = yokoRifleBeamEffect,
                prefabEffectComponent = yokoRifleBeamEffect.GetComponent<EffectComponent>(),
                prefabVfxAttributes = yokoRifleBeamEffect.GetComponent<VFXAttributes>(),
                prefabName = yokoRifleBeamEffect.name,
                spawnSoundEventName = yokoRifleBeamEffect.GetComponent<EffectComponent>().soundName
            });

            yokoRifleHitSmallEffect = Assets.LoadEffect("YokoRifleHitSmallEffect", 1.0f);
            yokoRifleMuzzleBigEffect = Assets.LoadEffect("YokoRifleMuzzleBigEffect", 1.0f);
            yokoRifleMuzzleSmallEffect = Assets.LoadEffect("YokoRifleMuzzleSmallEffect", 1.0f);
            yokoRifleExplosiveRoundExplosion = Assets.LoadEffect("YokoRifleExplosiveRoundExplosion", 1.0f);
            gurrenBrokenBoulderEffect = Assets.LoadEffect("BigBoulderBrokenPrefab", 5.0f);
            specialExplosion = Assets.LoadEffect("SpecialExplosion", 5.0f);
            drillPopEffect = Assets.LoadEffect("DrillPopEffect", 2.0f, true);
            earthMoundEffect = Assets.LoadEffect("EarthMoundEffect", 2.0f);
        }

        public static T LoadAsset<T>(string ressourceName)
        {
            var result = Addressables.LoadAssetAsync<T>(ressourceName).WaitForCompletion();
            if (result != null)
            {
                return result;
            } 
            else
            {
                TTGL_SurvivorPlugin.instance.Logger.LogError("Failed to LoadAsset - ressourceName: " + ressourceName);
            }
            return default(T);
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

        private static GameObject LoadEffect(string resourceName, float duration, bool applyScale = false)
        {
            GameObject newEffect = Modules.Assets.LoadAsset<GameObject>(resourceName);

            newEffect.AddComponent<DestroyOnTimer>().duration = duration;
            newEffect.AddComponent<NetworkIdentity>();
            var vfx = newEffect.AddComponent<VFXAttributes>();
            vfx.vfxPriority = VFXAttributes.VFXPriority.Always;
            var effect = newEffect.AddComponent<EffectComponent>();
            effect.applyScale = applyScale;
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