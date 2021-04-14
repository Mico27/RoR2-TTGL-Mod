using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using EntityStates;
using TTGL_Survivor.SkillStates;

namespace TTGL_Survivor.Modules
{
    public class GurrenLagannShadesConstrictComponent : MonoBehaviour
    {
        public float duration;
        public int visualState;
        public float durationCounter;
        public Transform parentTransform;
        public bool displayHole;
        Animator animator;
        Transform gapingHole;
        bool restricted;        
        float deleteCounter;

        public void Start()
        {
            visualState = 0;
            durationCounter = 0f;
            deleteCounter = 0f;
            this.animator = base.GetComponent<Animator>();
            var childLocator = base.GetComponent<ChildLocator>();
            if (childLocator)
            {
                this.gapingHole = childLocator.FindChild("GapingHole");
            }
            RestrictTarget();
        }

        public void OnDestroy()
        {
            UnrestirctTarget();
        }

        private void FixedUpdate()
        {
            durationCounter += Time.fixedDeltaTime;
            if (durationCounter >= duration)
            {
                this.visualState = 1;
            }
            UpdateHole();
            UpdateAnimator();
            RestrictTarget();
            UnrestirctTarget();
            DeleteDelayed();
        }
        private void UpdateHole()
        {
            if (this.gapingHole && this.gapingHole.gameObject.activeSelf != this.displayHole)
            {
                this.gapingHole.gameObject.SetActive(this.displayHole);
            }
        }

        private void UpdateAnimator()
        {
            if (this.animator)
            {
                this.animator.SetInteger("visualState", this.visualState);
            }
        }

        private void RestrictTarget()
        {
            if (!this.restricted && this.visualState == 0 && this.parentTransform)
            {
                this.restricted = true;
                if (NetworkServer.active)
                {
                    var entityStateMachines = this.parentTransform.GetComponents<EntityStateMachine>();
                    if (entityStateMachines != null && entityStateMachines.Length > 0)
                    {
                        foreach(var entityStateMachine in entityStateMachines)
                        {
                            if (!(entityStateMachine.state is GurrenLagannShadesConstrictState))
                            {
                                GurrenLagannShadesConstrictState stunState2 = new GurrenLagannShadesConstrictState();
                                entityStateMachine.SetInterruptState(stunState2, InterruptPriority.Death);
                            }
                        }                        
                    }
                }
            }
        }

        private void UnrestirctTarget()
        {
            if (this.restricted && this.visualState == 1 && this.parentTransform)
            {
                this.restricted = false;
                if (NetworkServer.active)
                {
                    var entityStateMachines = this.parentTransform.GetComponents<EntityStateMachine>();
                    if (entityStateMachines != null && entityStateMachines.Length > 0)
                    {
                        foreach (var entityStateMachine in entityStateMachines)
                        {
                            if (entityStateMachine.state is GurrenLagannShadesConstrictState)
                            {
                                entityStateMachine.SetInterruptState(EntityStateCatalog.InstantiateState(entityStateMachine.mainStateType), InterruptPriority.Death);
                            }
                        }
                    }
                }
            }
        }

        private void DeleteDelayed()
        {
            if (this.visualState == 1)
            {
                deleteCounter += Time.fixedDeltaTime;
                if (deleteCounter >= 0.5f)
                {
                    UnityEngine.Object.Destroy(base.gameObject);
                }
            }
        }
    }
}