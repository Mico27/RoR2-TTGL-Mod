using System;
using RoR2;
using TTGL_Survivor.Modules.Survivors;
using TTGL_Survivor.SkillStates;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.Modules.Components
{
    // just a class to run some custom code for things like weapon models
    public class GurrenController : MonoBehaviour
    {
        private Animator animator;

        public void Awake()
        {
            var model = base.GetComponent<ModelLocator>();
            this.animator = model.modelTransform.GetComponent<Animator>();
        }


        private void FixedUpdate()
        {
            if (this.animator)
            {
                float i = 1;
                if (this.animator.GetBool("isGrounded")) i = 0;
                this.animator.SetFloat("inAir", i);
            }
        }

    }
}