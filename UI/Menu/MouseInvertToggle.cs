using System;
using System.Globalization;
using System.Linq;
using Player.Input;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.Menu {
    [RequireComponent(typeof(Toggle))]
    public class MouseInvertToggle : MonoBehaviour {
        [Title("Input")]
        [SerializeField, Required] InputActionReference inputActionReference;
        [SerializeField, Required] InputReader inputReader;
        
        [SerializeField] EInputType inputType;
        [SerializeField] EProcessorType processorType;

        
        Toggle _mouseInvertToggle;

        void Awake() {
            _mouseInvertToggle = GetComponent<Toggle>();
        }

        void Start() {
            if(_mouseInvertToggle == null) {
                Debug.LogError("Mouse Invert Toggle is not set");
                return;
            }
            
            var inputActionName = inputActionReference.action.name;
            if(inputReader == null) {
                Debug.LogError("Input Reader is not set");
                return;
            }

            // Get the current input Action associated with the Reference
            var inputAction = inputReader.InputActions.FindAction(inputActionName);
            
            var schemeName = GetControlSchemeName();
            var inputActionBinding = 
                inputAction.bindings.FirstOrDefault(binding => binding.groups != null && binding.groups.Contains(schemeName));

            var processorName = GetProcessorName();
            
            var xValue = inputAction.GetParameterValue(processorName, inputActionBinding);
            var yValue = inputAction.GetParameterValue(processorName, inputActionBinding);

            var isInverted = false;
            
            switch (processorType) {
                case EProcessorType.InvertVector2X: {
                    if (xValue != null) {
                        var xValueString = xValue.Value.ToString(CultureInfo.InvariantCulture);
                        
                        isInverted = xValueString == "true";
                    }

                    break;
                }
                case EProcessorType.InvertVector2Y: {
                    if (yValue != null) {
                        var yValueString = yValue.Value.ToString(CultureInfo.InvariantCulture);
                        isInverted = yValueString == "true";
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            _mouseInvertToggle.isOn = isInverted;
            _mouseInvertToggle.onValueChanged.AddListener(ToggleMouseInvert);
        }
        
        void ToggleMouseInvert(bool isOn) {
            var inputActionName = inputActionReference.action.name;
            if(inputReader == null) {
                Debug.LogError("Input Reader is not set");
                return;
            }

            // Get the current input Action associated with the Reference
            var inputAction = inputReader.InputActions.FindAction(inputActionName);
            
            var schemeName = GetControlSchemeName();
            var inputActionBinding = 
                inputAction.bindings.FirstOrDefault(binding => binding.groups != null && binding.groups.Contains(schemeName));

            var processorName = GetProcessorName();
            
            switch (processorType) {
                case EProcessorType.InvertVector2X: {
                    inputAction.ApplyParameterOverride(processorName, isOn, inputActionBinding);
                    break;
                }
                case EProcessorType.InvertVector2Y: {
                    inputAction.ApplyParameterOverride(processorName, isOn, inputActionBinding);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
                EProcessorType.InvertVector2X => "invertVector2:invertX", // Use exact control scheme name
                EProcessorType.InvertVector2Y => "invertVector2:invertY", // Use exact control scheme name
                _ => throw new ArgumentOutOfRangeException(nameof(processorType), processorType, null)
            };
        }
        
        enum EInputType {
            Gamepad,
            KeyboardMouse
        }

        enum EProcessorType {
            InvertVector2X,
            InvertVector2Y,
        }
        
        // BUG: Remapping a key resets all processors of all bindings
        // SOLUTION: Modern Problems require Modern Solutions
        // Workaround, On Disable Update the Binding Processor
        void OnDisable() {
            ToggleMouseInvert(_mouseInvertToggle.isOn);
        }

        void OnDestroy() {
            if (_mouseInvertToggle != null) {
                _mouseInvertToggle.onValueChanged.RemoveListener(ToggleMouseInvert);
            }
        }
    }
}