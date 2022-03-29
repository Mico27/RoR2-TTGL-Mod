using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using HG.BlendableTypes;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TTGL_Survivor.Modules.Components;
using TTGL_Survivor.SkillStates;
using TTGL_Survivor.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Animations;

namespace TTGL_Survivor.Modules.Survivors
{
    public class TTGLIntro
    {
        public void CreateScene()
        {
            // this creates a config option to enable the character- feel free to remove if the character is the only thing in your mod
            //sceneEnabled = Modules.Config.CharacterEnableConfig("TTGLIntro");

            if (true)//sceneEnabled.Value)
            {                
                var sceneDef = ScriptableObject.CreateInstance<SceneDef>();
                ((ScriptableObject)sceneDef).name = "TTGL_intro";
                sceneDef.cachedName = "TTGL_intro";
                sceneDef.sceneAddress = new AssetReferenceScene("4fcac21c79f25cd40bb148a83606f1e0");
                sceneDef.sceneType = SceneType.Menu;
                sceneDef.isOfflineScene = true;
                sceneDef.loreToken = "TTGL_intro_LoreToken";
                sceneDef.shouldIncludeInLogbook = false;
                TTGL_SurvivorPlugin.sceneDefs.Add(sceneDef);

                On.RoR2.SceneCatalog.Init += SceneCatalog_Init;
                //Music;
                
                var d = new SoundAPI.Music.CustomMusicData();
                d.BanksFolderPath = System.IO.Path.Combine(Assets.ResourcesPath, "Banks");
                d.BepInPlugin = TTGL_SurvivorPlugin.instance.Info.Metadata;
                d.InitBankName = "TTGL_Init.bnk";
                d.PlayMusicSystemEventName = "Play_TTGLMusic";
                d.SoundBankName = "TTGLSceneMusic.bnk";

                d.SceneDefToTracks = new Dictionary<SceneDef, IEnumerable<SoundAPI.Music.MainAndBossTracks>>();

                var introScene = LegacyResourcesAPI.Load<SceneDef>("SceneDefs/intro");

                // that following block of code create a track def for a custom music in the logbook s
                var TTGL_introTrackDef = ScriptableObject.CreateInstance<SoundAPI.Music.CustomMusicTrackDef>();
                TTGL_introTrackDef.cachedName = "TTGL_intro";
                TTGL_introTrackDef.SoundBankName = d.SoundBankName;
                TTGL_introTrackDef.CustomStates = new List<SoundAPI.Music.CustomMusicTrackDef.CustomState>();
                var cstate1 = new SoundAPI.Music.CustomMusicTrackDef.CustomState();
                cstate1.GroupId = 1598298728U; // Music_menu
                cstate1.StateId = 158671394U; // TTGLIntroCutscene
                TTGL_introTrackDef.CustomStates.Add(cstate1);
                var cstate2 = new SoundAPI.Music.CustomMusicTrackDef.CustomState();
                cstate2.GroupId = 792781730U; // Music_system
                cstate2.StateId = 2607556080U; // Menu
                TTGL_introTrackDef.CustomStates.Add(cstate2);

                d.SceneDefToTracks.Add(introScene, new List<SoundAPI.Music.MainAndBossTracks>() { new SoundAPI.Music.MainAndBossTracks(TTGL_introTrackDef, null) });

                SoundAPI.Music.Add(d);
                
            }
        }

        private void SceneCatalog_Init(On.RoR2.SceneCatalog.orig_Init orig)
        {
            //var intro = ContentManager.sceneDefs.FirstOrDefault(x => x.cachedName == "intro");
            //((ScriptableObject)intro).name = "oldintro";
            //intro.cachedName = "oldintro";
            //var ttgl_intro = ContentManager.sceneDefs.FirstOrDefault(x => x.cachedName == "TTGL_intro");
            //((ScriptableObject)ttgl_intro).name = "intro";
            //ttgl_intro.cachedName = "intro";
            orig();
        }
    }
}