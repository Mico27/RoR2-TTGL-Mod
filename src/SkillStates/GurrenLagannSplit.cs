using EntityStates;
using EntityStates.Huntress;
using RoR2;
using RoR2.Projectile;
using TTGL_Survivor.Modules;
using TTGL_Survivor.Modules.Survivors;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenLagannSplit : BaseSkillState
    {
        public static float baseDuration = 18f;

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnExit()
        {
            TransformToLagann();
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();        
            if (base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        private void TransformToLagann()
        {
            Util.PlaySound(BaseBeginArrowBarrage.blinkSoundString, base.gameObject);
            if (NetworkServer.active && base.characterBody && base.characterBody.master)
            {
                var master = base.characterBody.master;
                master.TransformBody("LagannBody");                
                SpawnGurren();
                var body = master.GetBodyObject();
                Popup(body, body.transform.position + Vector3.up * 5f);
                var extraSkillLocator = body.GetComponent<ExtraSkillSlots.ExtraSkillLocator>();
                extraSkillLocator.extraFirst.RemoveAllStocks();
            }            
        }

        private void SpawnGurren()
        {
            float d = 0f;
            CharacterMaster characterMaster = new MasterSummon
            {
                masterPrefab = Gurren.allyPrefab,
                position = base.characterBody.transform.position + Vector3.up * d,
                rotation = base.characterBody.transform.rotation,
                summonerBodyObject = ((base.characterBody != null) ? base.characterBody.gameObject : null),
                ignoreTeamMemberLimit = true,
                useAmbientLevel = new bool?(true)
            }.Perform();
        }

        private void Popup(GameObject character, Vector3 newPosition)
        {
            if (character)
            {
                var velocity = Vector3.up * 20f;
                CharacterMotor characterMotor = character.GetComponent<CharacterMotor>();
                if (characterMotor && characterMotor.Motor)
                {
                    characterMotor.Motor.SetPosition(newPosition, true);
                    characterMotor.Motor.BaseVelocity = velocity;
                    characterMotor.velocity = velocity;
                    return;
                }
                gameObject.transform.position = newPosition;
            }
        }
    }
}