using System;
using System.Threading.Tasks;
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
            inputReader.InitializeInputActionAsset();
            _inputActions = inputReader.InputActions;
        }

        public void StartRebind(string actionName, int bindingIndex, bool excludeMouse, Action rebindCompleted) {
            InputAction action = _inputActions.FindAction(actionName);

            if (action == null || action.bindings.Count <= bindingIndex) {
                Debug.LogError("Action or Binding not Found, out of Index");
                return;
            }

            if (action.bindings[bindingIndex].isComposite) {
                PerformCompositeRebind(action, bindingIndex, excludeMouse, rebindCompleted);
            } else {
                PerformRebind(action, bindingIndex, excludeMouse, rebindCompleted);
            }
        }

        void PerformRebind(InputAction actionToRebind, int bindingIndex, bool excludeMouse, Action rebindCompleted) {
            if (actionToRebind == null || bindingIndex < 0) {
                Debug.LogError("Action or Binding not found");
                return;
            }

            Debug.Log($"Rebinding action: {actionToRebind.name}, binding index: {bindingIndex}");

            rebindOverlay.gameObject.SetActive(true);
            // Change to text to show the current action being rebound, if it is a composite, we show the name of the composite part
            var bindingName = actionToRebind.bindings[bindingIndex].isPartOfComposite
                ? actionToRebind.bindings[bindingIndex].name
                : actionToRebind.name;
            
            var rebindInformation = $"Rebinding: <b>{bindingName}</b>, press ANY key to rebind";
            rebindOverlayText.text = rebindInformation;
            actionToRebind.Disable();

            var rebind = actionToRebind.PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough("<Keyboard>/escape")
                .OnComplete(operation => {
                    Debug.Log("Rebind completed, Rebinded: " + actionToRebind.name + " with: " + operation.selectedControl.displayName);

                    rebindOverlay.gameObject.SetActive(false);
                    actionToRebind.Enable();
                    Debug.Log("Current binding: " + actionToRebind.GetBindingDisplayString(bindingIndex));

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
                    actionToRebind.Enable();
                    operation.Dispose();
                });

            if (excludeMouse) {
                rebind.WithControlsExcluding("<Mouse>/escape");
            }

            _rebindStarted?.Invoke(actionToRebind, bindingIndex);
            rebind.Start();
        }

        void PerformCompositeRebind(InputAction actionToRebind, int bindingIndex, bool excludeMouse, Action rebindCompleted) {
            void RebindNextPart(int partIndex) {
                if (partIndex < actionToRebind.bindings.Count && actionToRebind.bindings[partIndex].isPartOfComposite) {
                    PerformRebind(actionToRebind, partIndex, excludeMouse, () => RebindNextPart(partIndex + 1));
                } else {
                    rebindCompleted?.Invoke();
                }
            }

            RebindNextPart(bindingIndex + 1);
        }

        /// <summary>
        /// Retrieve the name of a binding.
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="bindingIndex"></param>
        /// <returns></returns>
        public string GetBindingName(string actionName, int bindingIndex) { InputAction action = _inputActions.FindAction(actionName); return action.GetBindingDisplayString(bindingIndex); }
        public string GetActionName(string actionName) { return _inputActions.FindAction(actionName).name; }
        
        public bool IsChildOfComposite(string actionName, int bindingIndex) {
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