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
        public static float carryOverEnergyPercent = 1f;
        private float energy;

        public override void OnEnter()
        {
            base.OnEnter();
            var spiralEnergyComponent = base.characterBody.GetComponent<SpiralEnergyComponent>();
            this.energy = spiralEnergyComponent.NetworkEnergy;
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
                var newBody = master.GetBody();
                var spiralEnergyComponent = newBody.GetComponent<SpiralEnergyComponent>();
                spiralEnergyComponent.NetworkEnergy = (this.energy - (this.energy * carryOverEnergyPercent));
                var body = master.GetBodyObject();
                Popup(body, body.transform.position + Vector3.up * 5f);
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
                useAmbientLevel = new bool?(true),
                inventoryToCopy = base.characterBody.inventory,
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