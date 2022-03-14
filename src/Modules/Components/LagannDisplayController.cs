using RoR2;
using RoR2.SurvivorMannequins;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.Modules.Components
{
    public class LagannDisplayController : MonoBehaviour
    {
        uint eyeCatchEventId;
        Animator modelAnimator;
        bool isAuthority;
        static int previousIndex = -1;
        public void OnEnable()
        {
            var mannequinSlotController = this.gameObject.GetComponentInParent<SurvivorMannequinSlotController>();            ;
            isAuthority = NetworkUser.readOnlyLocalPlayersList.Any((x) => x == mannequinSlotController.networkUser);
            if (isAuthority)
            {
                modelAnimator = this.GetComponent<Animator>();
                eyeCatchEventId = AkSoundEngine.PostEvent("TTGLEyeCatch", this.gameObject);
                this.PlayAnimation("EyeCatch", "TTGLEyeCatchAnim");
                var randomStateIndex = new System.Random().Next(0, 3);
                if (randomStateIndex == previousIndex)
                {
                    randomStateIndex++;
                    if (randomStateIndex == 3)
                    {
                        randomStateIndex = 0;
                    }
                }
                previousIndex = randomStateIndex;
                switch (randomStateIndex)
                {
                    case 0:
                        this.PlayAnimation("Base Layer", "TTGLSurvivorMenuSingle1");
                        break;
                    case 1:
                        this.PlayAnimation("Base Layer", "TTGLSurvivorMenuSingle2");
                        break;
                    default:
                        this.PlayAnimation("Base Layer", "TTGLSurvivorMenuSingle3");
                        break;
                }                
            }
        }
        
        public void OnDisable()
        {
            if (eyeCatchEventId != 0)
            {
                AkSoundEngine.StopPlayingID(eyeCatchEventId);
            }
        }

        private void PlayAnimation(string layerName, string animationStateName)
        {
            int layerIndex = modelAnimator.GetLayerIndex(layerName);
            modelAnimator.speed = 1f;
            modelAnimator.Update(0f);
            modelAnimator.PlayInFixedTime(animationStateName, layerIndex, 0f);
        }
    }
}