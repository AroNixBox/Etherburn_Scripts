using Interfaces.Attribute;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    [RequireComponent(typeof(Slider))]
    public class AttributeBar : MonoBehaviour {
        [ValidateInput("@ClassExtensions.IsClass<IAttribute, MonoBehaviour>(attribute)", 
            "The assigned object must implement IAttribute.")]
        [SerializeField] MonoBehaviour attribute;
        IAttribute _attribute;
        Slider _healthSlider;

        void Awake() {
            _attribute = (IAttribute) attribute;
            _healthSlider = GetComponent<Slider>();
        }
        void OnEnable() {
            if (_attribute != null) {
                _attribute.OnAttributeMaxValueSet += SetupMaxValue;
                _attribute.OnAttributeValueIncreased += UpdateBar;
                _attribute.OnAttributeValueDecresed += UpdateBar;
            }
        }
        void OnDisable() {
            if (_attribute != null) {
                _attribute.OnAttributeMaxValueSet -= SetupMaxValue;
                _attribute.OnAttributeValueIncreased -= UpdateBar;
                _attribute.OnAttributeValueDecresed -= UpdateBar;
            }
        }
        void SetupMaxValue(float maxValue) {
            if (_healthSlider != null) {
                _healthSlider.maxValue = maxValue;
            }
        }
        void UpdateBar() {
            UpdateBar(0);
        }
        void UpdateBar(float currentHealth) {
            if (_healthSlider != null) {
                _healthSlider.value = currentHealth;
            }
        }
        
    }
}