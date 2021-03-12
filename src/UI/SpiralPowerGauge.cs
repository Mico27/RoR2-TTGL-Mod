using RoR2;
using TTGL_Survivor.Modules;
using UnityEngine;
using UnityEngine.UI;

namespace TTGL_Survivor.UI
{
    // just a class to run some custom code for things like weapon models
    public class SpiralPowerGauge : MonoBehaviour
    {
        public SpiralEnergyComponent source { get; set; }

        private void Awake()
        {
            m_animator = this.gameObject.GetComponent<Animator>();
        }

        private void Start()
        {
            this.UpdateSpiritPowerGauge(0f);
        }

        public void Update()
        {
            this.UpdateSpiritPowerGauge(Time.deltaTime);
        }
        
        private void UpdateSpiritPowerGauge(float deltaTime)
        {
            if (this.source && m_animator)
            {
                var newRate = this.source.charge_rate;
                if (m_rate != newRate)
                {
                    m_rate = newRate;
                    m_animator.SetFloat("rate", newRate * 3);
                }
                var newAmount = this.source.energy;
                if (m_amount != newAmount)
                {
                    m_amount = newAmount;
                    m_animator.SetFloat("amount", Mathf.Clamp01(newAmount / (SpiralEnergyComponent.C_SPIRALENERGYCAP + 1)));
                }                
            }
        }
        
        private Animator m_animator;
        private float m_rate;
        private float m_amount;
    }
}