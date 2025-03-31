using System;
using System.Collections.Generic;
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

            var rebind = actionToRebind
                .PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough(excludingDevice)
                // BUG: Remapping a key resets all processors of all bindings
                // SOLUTION: Modern Problems require Modern Solutions
                // Currently simply every slider/ toggle that updates a binding processor
                // overrides the binding processor on disable, so the pre-value is reapplied
                // no idea wtf is going on
                .OnComplete(operation => {
                    rebindOverlay.gameObject.SetActive(false);
                    
                    // Reenable all the disabled maps
                    foreach (var disabledMap in disabledMaps) {
                        inputReader.EnableActionMap(disabledMap);
                    }

                    rebindCompleted?.Invoke();
                    operation.Dispose();
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