using System;
using Behavior;
using Behavior.Events.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    [RequireComponent(typeof(Slider))]
    public class AttributeBar : MonoBehaviour, IRequireAttributeEventChannel {
        EnergyValueChanged _energyValueChanged;
        Slider _healthSlider;
        bool _isInitialized;

        void Awake() {
            _healthSlider = GetComponent<Slider>();
        }
        public void InitializeEnergyChannel(EnergyValueChanged energyValueChannel, ref Action allChannelsInitialized) {
            _energyValueChanged = energyValueChannel;
            _energyValueChanged.Event += UpdateBar;
        }
        void UpdateBar(float currentHealth, Vector3? effectPosition) {
            effectPosition = null; // Discard effectPosition
            
            if(!_isInitialized) {
                _healthSlider.maxValue = currentHealth;
                _isInitialized = true;
            }
            
            if (_healthSlider != null) {
                _healthSlider.value = currentHealth;
            }
        }
        
        void OnDestroy() {
            _energyValueChanged.Event -= UpdateBar;
        }
    }
}