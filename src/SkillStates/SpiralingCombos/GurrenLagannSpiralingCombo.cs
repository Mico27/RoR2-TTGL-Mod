using RoR2;
using UnityEngine;
using EntityStates;
using RoR2.Audio;
using System;
using UnityEngine.Networking;

namespace TTGL_Survivor.SkillStates
{
    public class GurrenLagannSpiralingCombo : BaseSkillState
    {
        public int comboCounter;
        public override void OnEnter()
        {
            base.OnEnter();
            if (base.isAuthority)
            {
                if (this.comboCounter >= 4)
                {
                    this.comboCounter = 0;
                }
                this.comboCounter++;
                if (comboCounter == 1)
                {
                    var randomStateIndex = new System.Random().Next(0, 3);
                    switch (randomStateIndex)
                    {
                        case 0:
                            this.outer.SetNextState(new GurrenLagannLegSweep
                            {
                                comboCounter = this.comboCounter,
                            });
                            break;
                        case 1:
                            this.outer.SetNextState(new GurrenLagannInsideCrescentKick
                            {
                                comboCounter = this.comboCounter,
                            });
                            break;
                        default:
                            this.outer.SetNextState(new GurrenLagannUppercut
                            {
                                comboCounter = this.comboCounter,
                            });
                            break;
                    }
                }
                else if (comboCounter == 2)
                {
                    var randomStateIndex = new System.Random().Next(0, 3);
                    switch (randomStateIndex)
                    {
                        case 0:
                            this.outer.SetNextState(new GurrenLagannHookPunch
                            {
                                comboCounter = this.comboCounter,
                            });
                            break;
                        case 1:
                            this.outer.SetNextState(new GurrenLagannStabbingRight
                            {
                                comboCounter = this.comboCounter,
                            });
                            break;
                        default:
                            this.outer.SetNextState(new GurrenLagannMmaKick
                            {
                                comboCounter = this.comboCounter,
                            });
                            break;
                    }
                }
                else if(comboCounter == 3)
                {
                    var randomStateIndex = new System.Random().Next(0, 3);
                    switch (randomStateIndex)
                    {
                        case 0:
                            this.outer.SetNextState(new GurrenLagannUpwardThrust
                            {
                                comboCounter = this.comboCounter,
                            });
                            break;
                        case 1:
                            this.outer.SetNextState(new GurrenLagannStabbingLeft
                            {
                                comboCounter = this.comboCounter,
                            });
                            break;
                        default:
                            this.outer.SetNextState(new GurrenLagannMartelo2
                            {
                                comboCounter = this.comboCounter,
                            });
                            break;
                    }
                }
                else
                {
                    var randomStateIndex = new System.Random().Next(0, 2);
                    switch (randomStateIndex)
                    {
                        case 0:
                            this.outer.SetNextState(new GurrenLagannMmaKick
                            {
                                comboCounter = this.comboCounter,
                            });
                            break;
                        default:
                            this.outer.SetNextState(new GurrenLagannThrustSlash
                            {
                                comboCounter = this.comboCounter,
                            });
                            break;
                    }
                }
            }
        }
        
    }
}