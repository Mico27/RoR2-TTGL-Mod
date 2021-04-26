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
                m_animator.SetFloat("rate", this.source.charge_rate * 3);
                m_animator.SetFloat("amount", Mathf.Clamp(this.source.energy / SpiralEnergyComponent.C_SPIRALENERGYCAP, 0f, 0.99f));
            }
        }
        
        private Animator m_animator;
    }
}