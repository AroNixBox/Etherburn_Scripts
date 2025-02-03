using System;
using System.Collections;
using Behavior;
using Behavior.Events.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    [RequireComponent(typeof(Slider))]
    public class AttributeBar : MonoBehaviour, IRequireAttributeEventChannel {
        [SerializeField] bool smoothUpdate;
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

            if (!_isInitialized) {
                _healthSlider.maxValue = currentHealth;
                _isInitialized = true;
            }

            if (_healthSlider != null) {
                if (smoothUpdate) {
                    StartCoroutine(SmoothUpdate(currentHealth));
                } else {
                    _healthSlider.value = currentHealth;
                }
            }
        }

        IEnumerator SmoothUpdate(float targetValue) {
            float elapsedTime = 0f;
            float duration = 0.5f; // Duration of the smooth update
            float startValue = _healthSlider.value;

            while (elapsedTime < duration) {
                elapsedTime += Time.deltaTime;
                _healthSlider.value = Mathf.Lerp(startValue, targetValue, elapsedTime / duration);
                yield return null;
            }

            _healthSlider.value = targetValue; // Ensure the final value is set
        }
        
        void OnDestroy() {
            _energyValueChanged.Event -= UpdateBar;
        }
    }
}