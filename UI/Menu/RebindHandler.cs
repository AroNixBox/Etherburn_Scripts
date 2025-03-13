using System;
using System.Linq;
using Player.Input;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Menu {
    public class RebindHandler : MonoBehaviour {
        [SerializeField] InputReader inputReader;
        [SerializeField] RectTransform rebindOverlay;
        [SerializeField] TMP_Text rebindOverlayText;

        PlayerInputActions _inputActions;
        Action _rebindCanceled;
        Action _rebindCompleted;
        Action<InputAction, int> _rebindStarted;

        void Awake() {
            _inputActions = inputReader.InputActions;
        }

        public void StartRebind(string actionName, int bindingIndex, Action rebindCompleted) {
            InputAction action = _inputActions.FindAction(actionName);

            if (action == null || action.bindings.Count <= bindingIndex) {
                Debug.LogError("Action or Binding not Found, out of Index");
                return;
            }

            if (action.bindings[bindingIndex].isComposite) {
                PerformCompositeRebind(action, bindingIndex, rebindCompleted);
            } else {
                PerformRebind(action, bindingIndex, rebindCompleted);
            }
        }

        void PerformRebind(InputAction actionToRebind, int bindingIndex, Action rebindCompleted) {
            if (actionToRebind == null || bindingIndex < 0) {
                Debug.LogError("Action or Binding not found");
                return;
            }
            
            rebindOverlay.gameObject.SetActive(true);
            // Change to text to show the current action being rebound, if it is a composite, we show the name of the composite part
            var bindingName = actionToRebind.bindings[bindingIndex].isPartOfComposite
                ? actionToRebind.bindings[bindingIndex].name
                : actionToRebind.name;
            
            var rebindInformation = $"Rebinding: <b>{bindingName}</b>, press ANY key to rebind";
            rebindOverlayText.text = rebindInformation;
            // Disable all Input Actions to prevent conflicts
            var disabledMaps = inputReader.GetAllActiveActionMaps();
            foreach (var disabledMap in disabledMaps) {
                inputReader.DisableActionMap(disabledMap);
            }
            
            // Debug all enabled action maps
            var enabledMaps = inputReader.GetAllActiveActionMaps();
            foreach (var enabledMap in enabledMaps) {
                Debug.Log("Enabled Map: " + enabledMap);
            }

            // Workouround, only one .WithCancelingThrough() is allowed
            // https://discussions.unity.com/t/withcancelingthrough-not-accepting-more-than-one-binding/804335/2
            var excludingGamepadDevice = "<Gamepad>/select";
            var excludingKeyboardDevice = "<Keyboard>/escape";
            var excludingDevice = InputUtils.WasLastInputController() ? excludingGamepadDevice : excludingKeyboardDevice;

            var rebind = actionToRebind.PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough(excludingDevice)
                .OnComplete(operation => {
                    rebindOverlay.gameObject.SetActive(false);

                    // Reenable all the disabled maps
                    foreach (var disabledMap in disabledMaps) {
                        inputReader.EnableActionMap(disabledMap);
                    }

                    operation.Dispose();
                    rebindCompleted?.Invoke();
                    _rebindCompleted?.Invoke();
                })
                .OnCancel(operation => {
                    Debug.Log("Rebind canceled - Checking active input devices:");

                    foreach (var device in InputSystem.devices) {
                        Debug.Log($"Device: {device.name}, enabled: {device.enabled}");
                    }

                    rebindOverlay.gameObject.SetActive(false);

                    // Reenable all the disabled maps
                    foreach (var disabledMap in disabledMaps) {
                        inputReader.EnableActionMap(disabledMap);
                    }

                    operation.Dispose();
                });

            rebind.Start();

            _rebindStarted?.Invoke(actionToRebind, bindingIndex);
            rebind.Start();
        }

        void PerformCompositeRebind(InputAction actionToRebind, int bindingIndex, Action rebindCompleted) {
            void RebindNextPart(int partIndex) {
                if (partIndex < actionToRebind.bindings.Count && actionToRebind.bindings[partIndex].isPartOfComposite) {
                    PerformRebind(actionToRebind, partIndex, () => RebindNextPart(partIndex + 1));
                } else {
                    rebindCompleted?.Invoke();
                }
            }

            RebindNextPart(bindingIndex + 1);
        }
        
        /// <returns>Identifier of the DeviceLayout (PlaystationController, XBOX)</returns>
        public string GetBindingName(InputActionReference inputActionReference, int bindingIndex, string controlPath, string deviceLayoutName) {
            var action = GetAction(inputActionReference);
            if (action == null) return "Invalid action";
            
            var binding = action.bindings[bindingIndex];
            
            Debug.Log($"Binding: {binding.name}, ControlPath: {controlPath}, DeviceLayoutName: {deviceLayoutName}");
            
            // Check if this is part of a composite
            if (binding.isComposite) {
                // Find all parts of this composite and concatenate them
                string compositeName = "";
                // Start from the next binding (first part)
                int partIndex = bindingIndex + 1;
                
                // Collect all parts of the composite
                while (partIndex < action.bindings.Count && action.bindings[partIndex].isPartOfComposite) {
                    // Get the part's display name
                    action.GetBindingDisplayString(partIndex, out var partDeviceLayout, out var partControlPath);
                    
                    string partName = GetSingleBindingName(action.bindings[partIndex], partControlPath, partDeviceLayout);
                    string partBindingName = action.bindings[partIndex].name;
                    
                    // Add separator if not the first part
                    if (!string.IsNullOrEmpty(compositeName)) {
                        compositeName += " / ";
                    }
                    compositeName += partBindingName + ": " + partName;
                    
                    partIndex++;
                }
                
                return compositeName;
            }
            // Handle part of composite (when specifically requesting a single part)
            else if (binding.isPartOfComposite) {
                return binding.name + ": " + GetSingleBindingName(binding, controlPath, deviceLayoutName);
            }
            // Handle normal bindings
            else {
                return GetSingleBindingName(binding, controlPath, deviceLayoutName);
            }
        }
        
        // Helper method to get name for a single binding
        string GetSingleBindingName(InputBinding binding, string controlPath, string deviceLayoutName) {
            // bindigs.groups can start with e.g. "GamepadOrKeyboard&Mouse;Gamepad" And then there can also be multiple split by ;
            // Same thing goes for keyboard&mouse
            var isGamepad = binding.groups != null && binding.groups.Split(';')
                                .Any(group => group == "Gamepad");
            var isKeyboardAndMouse = binding.groups != null && binding.groups.Split(';')
                                .Any(group => group == "Keyboard&Mouse");
            
            if(!isGamepad && !isKeyboardAndMouse) {
                Debug.Log("<color=red><b>binding groups: " + binding.groups + "</b></color>");
            }
            
            if (isGamepad) {
                if (InputUtils.IsPlaystationControllerConnected(deviceLayoutName)) {
                    return InputUtils.MapToPlayStationControl(controlPath);
                }
                if (InputUtils.IsXboxControllerConnected(deviceLayoutName)) {
                    return InputUtils.MapToXboxControl(controlPath);
                }
                if (InputUtils.IsSwitchControllerConnected(deviceLayoutName)) {
                    return InputUtils.MapToSwitchControl(controlPath);
                }
                return controlPath;
            }
            
            return isKeyboardAndMouse ? InputUtils.MapToKeyboardAndMouseControl(controlPath) : controlPath;
        }
        public InputAction GetAction(InputActionReference actionReference) {
            if(_inputActions == null) {
                Debug.LogError("Input Actions not found");
                return null;
            }
            
            if(_inputActions.FindAction(actionReference.action.name) == null) {
                Debug.LogError("Action not found");
                return null;
            }
            
            return _inputActions.FindAction(actionReference.action.name);
        }
        
        public bool IsChildOfComposite(string actionName, int bindingIndex) {
            if(_inputActions == null) {
                Debug.LogError("Input Actions not found");
                return false;
            }
            
            if(_inputActions.FindAction(actionName) == null) {
                Debug.LogError("Action not found");
                return false;
            }
            
            InputAction action = _inputActions.FindAction(actionName);
            return action.bindings[bindingIndex].isPartOfComposite;
        }
        // Save the bindings into player prefs for each action
        static void SaveBindingOverride(InputAction action) {
            for (int i = 0; i < action.bindings.Count; i++) {
                PlayerPrefs.SetString(action.actionMap + action.name + i, action.bindings[i].overridePath);
            }
        }
        
        public void LoadBindingOverride(string actionName) {
            // Gather the Input Action given its name
            InputAction action = _inputActions.FindAction(actionName);
        
            // For each binding apply the binding from PlayerPrefs
            for (int i = 0; i < action.bindings.Count; i++) {
                if (!string.IsNullOrEmpty(PlayerPrefs.GetString(action.actionMap + action.name + i)))
                    action.ApplyBindingOverride(i, PlayerPrefs.GetString(action.actionMap + action.name + i));
            }
        }

        /// <summary>
        /// Reset the bindings for the given action
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="bindingIndex"></param>
        public void ResetBinding(string actionName, int bindingIndex) {
            // Gather the Input Action given its name
            InputAction action = _inputActions.FindAction(actionName);

            if (action == null || action.bindings.Count <= bindingIndex) {
                Debug.LogError("Action or Binding not found");
                return;
            }
            
            if (action.bindings[bindingIndex].isComposite) {
                for (int i = bindingIndex; i < action.bindings.Count && action.bindings[i].isComposite; i++) {
                    action.RemoveBindingOverride(i);
                }
            }
            else {
                action.RemoveBindingOverride(bindingIndex);
            }

            // TODO: Save the default rebind
            // SaveBindingOverride(action);
        }
    }
}