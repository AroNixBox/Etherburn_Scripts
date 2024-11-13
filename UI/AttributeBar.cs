using Interfaces.Attribute;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    [RequireComponent(typeof(Slider))]
    public class AttributeBar : MonoBehaviour {
        [ValidateInput("@attribute is IAttribute", "Attribute must implement IAttribute")]
        [SerializeField] MonoBehaviour attribute;
        IAttribute _attribute;
        Slider _healthSlider;
        bool _isInitialized;

        void Awake() {
            _attribute = (IAttribute) attribute;
            _healthSlider = GetComponent<Slider>();
        }
        void OnEnable() {
            if (_attribute != null) {
                _attribute.OnAttributeValueIncreased += UpdateBar;
                _attribute.OnAttributeValueDecresed += UpdateBar;
            }
        }
        void OnDisable() {
            if (_attribute != null) {
                _attribute.OnAttributeValueIncreased -= UpdateBar;
                _attribute.OnAttributeValueDecresed -= UpdateBar;
            }
        }
        void UpdateBar(float currentHealth) {
            if(!_isInitialized) {
                _healthSlider.maxValue = currentHealth;
                _isInitialized = true;
            }
            
            if (_healthSlider != null) {
                _healthSlider.value = currentHealth;
            }
        }
    }
}