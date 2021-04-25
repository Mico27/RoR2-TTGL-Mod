using UnityEngine;
using RoR2;
using static TTGL_Survivor.Modules.TTGLMusicController;

namespace TTGL_Survivor.Modules
{
    public class TTGLMusicRemote : MonoBehaviour
    {        
        private EntityStateMachine outer = null;
        private TTGLMusicController controller = null;
        
        public void Awake()
        {
            this.controller = null;
            this.outer = base.GetComponent<EntityStateMachine>();                       
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
            if (isAuthority && this.controller)
            {
                this.controller.PlayMusic(musicType);
            }
        }

        public void StopMusic(MusicType musicType)
        {
            if (isAuthority && this.controller)
            {
                this.controller.StopMusic(musicType);
            }
        }
        
        protected bool isAuthority
        {
            get
            {
                return Util.HasEffectiveAuthority(this.outer.networkIdentity);
            }
        }

    }
}