using System;
using Player.Input;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UI.Menu {
    public class RebindHandler : MonoBehaviour {
        [SerializeField] InputReader inputReader;
        [SerializeField] RectTransform rebindOverlay;

        PlayerInputActions _inputActions;
        Action _rebindCanceled;
        Action _rebindCompleted;
        Action<InputAction, int> _rebindStarted;

        void Start() {
            inputReader.InitializeInputActionAsset();
        }

        public void StartRebind(string actionName, int bindingIndex, bool excludeMouse) {
            _inputActions = inputReader.InputActions;
            
            // Find the Input Action based on its name
            InputAction action = _inputActions.FindAction(actionName);

            if (action == null || action.bindings.Count <= bindingIndex) {
                Debug.LogError("Action or Binding not Found, out of Index");
                return;
            }

            // If it is valid check if it is a composite
            // Iterate through each each composite part and rebind it
            if (action.bindings[bindingIndex].isComposite) {
                // TODO: Change here that we dont change all composite parts in a row but rather one by one
                
                var firstPartIndex = bindingIndex + 1;

                if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isComposite) {
                    PerformRebind(action, bindingIndex, true, excludeMouse);
                }
            }
            else {
                PerformRebind(action, bindingIndex, false, excludeMouse);
            }
        }

        void PerformRebind(InputAction actionToRebind, int bindingIndex, bool allCompositeParts, bool excludeMouse) {
            if (actionToRebind == null || bindingIndex < 0) {
                Debug.LogError("Action or Binding not found");
                return;
            }

            Debug.Log($"Rebinding action: {actionToRebind.name}, binding index: {bindingIndex}");
            // Print the old binding
            Debug.Log("Old binding: " + actionToRebind.GetBindingDisplayString(bindingIndex));

            rebindOverlay.gameObject.SetActive(true); // enable the overlay
            actionToRebind.Disable();

            var rebind = actionToRebind.PerformInteractiveRebinding(bindingIndex);
            
            // Handle rebind completion
            rebind.OnComplete(operation => {
                Debug.Log("Rebind completed, Rebinded: " + actionToRebind.name + " with: " + operation.selectedControl.displayName);
                
                rebindOverlay.gameObject.SetActive(false);
                actionToRebind.Enable();
                // Print the current binding that we overrode
                Debug.Log("Current binding: " + actionToRebind.GetBindingDisplayString(bindingIndex));
                
                operation.Dispose();
                

                if (allCompositeParts) {
                    var nextBindingIndex = bindingIndex + 1;
                    if (nextBindingIndex < actionToRebind.bindings.Count &&
                        actionToRebind.bindings[nextBindingIndex].isComposite) {
                        Debug.Log("Rebinding next composite part");
                        PerformRebind(actionToRebind, nextBindingIndex, true, excludeMouse);
                    }
                }
                _rebindCompleted?.Invoke();
            });

            // Handle rebind cancel
            rebind.OnCancel(operation => {
                Debug.Log("Rebind canceled - Checking active input devices:");

                foreach (var device in InputSystem.devices) {
                    Debug.Log($"Device: {device.name}, enabled: {device.enabled}");
                }

                rebindOverlay.gameObject.SetActive(false);
                actionToRebind.Enable();
                operation.Dispose();
            });

            // Cancel rebind if pressing escape
            rebind.WithCancelingThrough("<Keyboard>/escape");

            // Exclude mouse
            if (excludeMouse) {
                rebind.WithControlsExcluding("<Mouse>/escape");
            }

            _rebindStarted?.Invoke(actionToRebind, bindingIndex);

            // Actually start the rebind process
            rebind.Start();
        }

        /// <summary>
        /// Retrieve the name of a binding.
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="bindingIndex"></param>
        /// <returns></returns>
        public string GetBindingName(string actionName, int bindingIndex) {
            InputAction action = _inputActions.FindAction(actionName);
            return action.GetBindingDisplayString(bindingIndex);
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