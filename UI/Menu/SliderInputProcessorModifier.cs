using System;
using System.Collections.Generic;
using System.Linq;
using Player.Input;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.Menu {
    public class SliderInputProcessorModifier : MonoBehaviour {
        [Header("References")]
        [Title("User Interface")]
        [SerializeField, Required] Slider slider;
        [SerializeField, Required] TMP_Text actionNameText;
        
        [Title("Input")]
        [SerializeField, Required] InputActionReference inputActionReference;
        [SerializeField, Required] InputReader inputReader;
        
        [Header("Settings")]
        [SerializeField] string modifierInteraction = "Sensitivity";
        [SerializeField] EInputType inputType;
        [SerializeField] EProcessorType processorType;

        const float MinValue = 0f;
        const float MaxValue = 2f;

        void Start() {
            slider.minValue = MinValue;
            slider.maxValue = MaxValue;
            
            var inputActionName = inputActionReference.action.name;
            if(inputReader == null) {
                Debug.LogError("Input Reader is not set");
                return;
            }

            // Get the current input Action associated with the Reference
            var inputAction = inputReader.InputActions.FindAction(inputActionName);
            
            var schemeName = GetControlSchemeName();
            var inputDeviceActionBindings = inputAction.bindings.Where(binding => 
                binding.groups != null && binding.groups.Contains(schemeName)).ToArray();

            var inputDeviceActionBindingValues = new List<float>();
            var processorName = GetProcessorName();
            
            foreach (var inputDeviceActionBinding in inputDeviceActionBindings) {
                var xScale = inputAction.GetParameterValue(processorName + ":x", inputDeviceActionBinding);
                var yScale = inputAction.GetParameterValue(processorName + ":y", inputDeviceActionBinding);
                if (xScale != null) { 
                    inputDeviceActionBindingValues.Add(xScale.Value.ToSingle());
                }
                if (yScale != null) { 
                    inputDeviceActionBindingValues.Add(yScale.Value.ToSingle());
                }
            }
            
            // get the average value of the gamepad bindings
            if (inputDeviceActionBindingValues.Count <= 0) {
                Debug.LogError("No gamepad bindings found");
                return;
            }
            
            var averageValue = inputDeviceActionBindingValues.Average();
                
            // Set the slider value to the average value
            slider.value = averageValue;
            
            actionNameText.text = $"{inputActionName} {modifierInteraction}";
            slider.onValueChanged.AddListener(UpdateBindingProcessor);
        }
        
        string GetControlSchemeName() {
            return inputType switch {
                EInputType.Gamepad => "Gamepad", // Use exact control scheme name from Input Action Asset
                EInputType.KeyboardMouse => "Keyboard&Mouse", // Use exact control scheme name
                _ => throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null)
            };
        }
        
        string GetProcessorName() {
            return processorType switch {
                EProcessorType.ScaleVector2 => "scaleVector2", // Use exact control scheme name from Input Action Asset
                _ => throw new ArgumentOutOfRangeException(nameof(processorType), processorType, null)
            };
        }

        void OnDestroy() {
            slider.onValueChanged.RemoveListener(UpdateBindingProcessor);
        }

        void UpdateBindingProcessor(float sliderValue) {
            var inputActionName = inputActionReference.action.name;
            if (inputReader == null) {
                Debug.LogError("Rebind Handler is not set");
                return;
            }

            // Get the current input Action associated with the Reference
            var inputAction = inputReader.InputActions.FindAction(inputActionName);
            var schemeName = GetControlSchemeName();
            
            var inputDeviceActionBindings = inputAction.bindings.Where(binding => 
                binding.groups != null && binding.groups.Contains(schemeName)).ToArray();
            
            var processorName = GetProcessorName();
            
            foreach (var inputDeviceActionBinding in inputDeviceActionBindings) {
                // Set the value of the input action to the slider value
                inputAction.ApplyParameterOverride(processorName + ":x", sliderValue, inputDeviceActionBinding);
                inputAction.ApplyParameterOverride(processorName + ":y", sliderValue, inputDeviceActionBinding);
            }
        }

        // BUG: Remapping a key resets all processors of all bindings
        // SOLUTION: Modern Problems require Modern Solutions
        // Workaround, On Disable Update the Binding Processor
        void OnDisable() {
            UpdateBindingProcessor(slider.value);
        }

        enum EInputType {
            Gamepad,
            KeyboardMouse
        }

        enum EProcessorType {
            ScaleVector2
        }
    }
}
