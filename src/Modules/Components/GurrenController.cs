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
        public const float passiveDistance = 40f;
        private CharacterBody body;
        private Animator animator;
        private float checkPassiveInterval = 0.5f;
        private float checkPassiveStopWatch = 0f;

        public void Awake()
        {
            this.body = base.GetComponent<CharacterBody>();
            var model = base.GetComponent<ModelLocator>();
            this.animator = model.modelTransform.GetComponent<Animator>();
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }
        public void OnDestroy()
        {
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
        }

        private void FixedUpdate()
        {            
            if (this.animator)
            {
                float i = 1;
                if (this.animator.GetBool("isGrounded")) i = 0;
                this.animator.SetFloat("inAir", i);
            }
            DoPassive();
        }

        private void DoPassive()
        {
            if (!NetworkClient.active)
            {
                return;
            }
            checkPassiveStopWatch += Time.fixedDeltaTime;
            if (checkPassiveStopWatch > checkPassiveInterval)
            {
                checkPassiveStopWatch = 0f;
                if (this.body)
                {
                    int buffCounts = 0;
                    var allies = TeamComponent.GetTeamMembers(TeamIndex.Player);
                    foreach (var ally in allies)
                    {
                        var allyBody = ally.body;
                        if (allyBody && allyBody != this.body &&
                            (Vector3.Distance(allyBody.transform.position, this.body.transform.position) <= GurrenController.passiveDistance))
                        {
                            buffCounts++;
                        }
                    }
                    this.body.SetBuffCount(Buffs.kaminaBuff.buffIndex, buffCounts);
                }
            }
        }
        
        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self == this.body && self.HasBuff(Modules.Buffs.kaminaBuff))
            {
                var kaminBuffCount = self.GetBuffCount(Modules.Buffs.kaminaBuff);
                self.damage *= (1 + (Modules.Buffs.kaminaBuffDmgModifier * kaminBuffCount));
            }
        }
    }
}