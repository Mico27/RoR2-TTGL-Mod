using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.Modules.Components
{
    // just a class to run some custom code for things like weapon models
    public class GurrenLagannController : MonoBehaviour
    {
        private CharacterBody characterBody;

        private void Awake()
        {
            this.characterBody = this.gameObject.GetComponent<CharacterBody>();
        }


        public void OnGurrenShadesDestroy()
        {
            if (this.characterBody.hasAuthority)
            {
                this.characterBody.skillLocator.secondary.AddOneStock();
            }
        }
    }
}