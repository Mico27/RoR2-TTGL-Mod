using UnityEngine;
using RoR2;
using UnityEngine.SceneManagement;

namespace TTGL_Survivor.Modules
{
    public class TTGLMusicController : MonoBehaviour
    {        
        public bool wasPaused;
        public bool ttglMusicEnabled;
        public bool playedMusic;
        public MusicType currentMusicType;

        public enum MusicType
        {
            FullBuff,
            Combine,
        }

        public void Awake()
        {                      
            MusicController.pickTrackHook += MusicController_pickTrackHook;
            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
            this.ttglMusicEnabled = Modules.Config.ttglMusicEnabled;
        }

        public void OnDestroy()
        {
            MusicController.pickTrackHook -= MusicController_pickTrackHook;
            SceneManager.sceneUnloaded -= SceneManager_sceneUnloaded;
            this.StopMusic(this.currentMusicType);
        }
        private void LateUpdate()
        {
            if (this.playedMusic)
            {
                bool flag = Time.timeScale == 0f;
                if (this.wasPaused != flag)
                {
                    switch (this.currentMusicType)
                    {
                        case MusicType.FullBuff:
                            AkSoundEngine.PostEvent(flag ? "TTGLFullBuffPause" : "TTGLFullBuffResume", base.gameObject);
                            break;
                        case MusicType.Combine:
                            AkSoundEngine.PostEvent(flag ? "TTGLCombineMusicPause" : "TTGLCombineMusicResume", base.gameObject);
                            break;
                    }                    
                    this.wasPaused = flag;
                }
            }            
        }

        private void SceneManager_sceneUnloaded(Scene arg0)
        {
            this.StopMusic(this.currentMusicType);
        }

        private void MusicController_pickTrackHook(MusicController musicController, ref MusicTrackDef newTrack)
        {
            if (this.playedMusic)
            {
                newTrack = null;
            }
        }
        

        public void PlayMusic(MusicType musicType)
        {
            if (!playedMusic && ttglMusicEnabled)
            {
                this.currentMusicType = musicType;
                switch (this.currentMusicType)
                {
                    case MusicType.FullBuff:
                        AkSoundEngine.PostEvent("TTGLFullBuffPlay", this.gameObject, (uint)AkCallbackType.AK_CallbackBits, this.MusicEventCallback, null);
                        break;
                    case MusicType.Combine:
                        AkSoundEngine.PostEvent("TTGLCombineMusicPlay", this.gameObject, (uint)AkCallbackType.AK_CallbackBits, this.MusicEventCallback, null);
                        break;
                }                
                playedMusic = true;
                TTGL_SurvivorPlugin.instance.Logger.LogMessage("PlayMusic Called");
            }
        }

        public void StopMusic(MusicType musicType)
        {
            if (playedMusic && ttglMusicEnabled && this.currentMusicType == musicType)
            {
                switch (this.currentMusicType)
                {
                    case MusicType.FullBuff:
                        AkSoundEngine.PostEvent("TTGLFullBuffStop", this.gameObject);
                        break;
                    case MusicType.Combine:
                        AkSoundEngine.PostEvent("TTGLCombineMusicStop", this.gameObject);
                        break;
                }
                playedMusic = false;
                TTGL_SurvivorPlugin.instance.Logger.LogMessage("StopMusic Called");
            }
        }
        
        private void MusicEventCallback(object cookie, AkCallbackType in_type, AkCallbackInfo in_info)
        {
            TTGL_SurvivorPlugin.instance.Logger.LogMessage("MusicEventCallback Called : "+ in_type.ToString());
            if (in_type == AkCallbackType.AK_EndOfEvent)
            {
               playedMusic = false;                
            }
        }
        
    }
}