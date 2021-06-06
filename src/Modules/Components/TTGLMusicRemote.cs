using UnityEngine;
using RoR2;
using static TTGL_Survivor.Modules.TTGLMusicController;

namespace TTGL_Survivor.Modules
{
    public class TTGLMusicRemote : MonoBehaviour
    {        
        private TTGLMusicController controller = null;
        
        public void Awake()
        {
            this.controller = null;                    
            MusicController.pickTrackHook += MusicController_pickTrackHook;
        }

        public void OnDestroy()
        {
            MusicController.pickTrackHook -= MusicController_pickTrackHook;
        }

        private void MusicController_pickTrackHook(MusicController musicController, ref MusicTrackDef newTrack)
        {
            if (!this.controller && musicController && musicController.gameObject)
            {
                this.controller = musicController.gameObject.GetComponent<TTGLMusicController>();
                if (!this.controller)
                {
                    this.controller = musicController.gameObject.AddComponent<TTGLMusicController>();
                }
                MusicController.pickTrackHook -= MusicController_pickTrackHook;
            }
        }


        public void PlayMusic(MusicType musicType)
        {
            if (CanPlayMusic() && this.controller)
            {
                this.controller.PlayMusic(musicType);
            }
        }

        public void StopMusic(MusicType musicType)
        {
            if (CanPlayMusic() && this.controller)
            {
                this.controller.StopMusic(musicType);
            }
        }
        
        protected bool CanPlayMusic()
        {
            return CameraRigController.IsObjectSpectatedByAnyCamera(base.gameObject);
        }

    }
}