using EntityStates;
using EntityStates.Huntress;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class LagannToggleCanopy : BaseSkillState
    {
        public const float c_ArmorBuffAmount = 150f;
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(BaseBeginArrowBarrage.blinkSoundString, base.gameObject);

            if (NetworkServer.active)
            {
                if (this.HasBuff(Modules.Buffs.canopyBuff))
                {
                    base.characterBody.RemoveBuff(Modules.Buffs.canopyBuff);
                }
                else
                {
                    base.characterBody.AddBuff(Modules.Buffs.canopyBuff);
                }                
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

    }
}