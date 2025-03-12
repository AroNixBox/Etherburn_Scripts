using System;
using System.Collections.Generic;
using Player.Input;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.Menu {
    public class BaseMenuNavigation : MonoBehaviour {
        [Header("User Interface")]
        [SerializeField, Required] Button firstSelectedButton;
        [SerializeField] bool ensureSetFirstSelectedButtonActive;
        [SerializeField] bool closable = true;
        [SerializeField, Required] [ShowIf("@closable")] Button closeButton;
        [BoxGroup("Selectables")]
        [SerializeField, Required] List<Selectable> selectables = new();

        
        [Button("Capture Selectables In Order")]
        [BoxGroup("Selectables")]
        void CaptureSelectablesInOrder() {
            selectables.Clear();
            selectables.AddRange(GetComponentsInChildren<Selectable>(true));
        }
        
        [Header("Events")]
        [ShowIf("@closable")] 
        public UnityEvent onCloseButtonPressed;
        EventSystem _sceneEventSystem;

        // Assuming this gets disabled when another window pops up and re-enabled when its opened
        void OnEnable() {
            _sceneEventSystem ??= EventSystem.current;

            if (_sceneEventSystem == null) {
                Debug.LogError("No EventSystem found in scene");
                return;
            }
            SetNavigationOrder();
            
            // Check if the menu was opened with a controller
            if (InputUtils.WasLastInputController() || ensureSetFirstSelectedButtonActive) {
                _sceneEventSystem.SetSelectedGameObject(firstSelectedButton.gameObject);
            }

            // Subscribe to device change events
            InputSystem.onDeviceChange += OnDeviceChange;
        }
        
        void SetNavigationOrder() {
            for (int i = 0; i < selectables.Count; i++) {
                Navigation nav = new Navigation {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = i > 0 ? selectables[i - 1] : null, // Vorheriges Element
                    selectOnDown = i < selectables.Count - 1 ? selectables[i + 1] : null // Nächstes Element
                };
                selectables[i].navigation = nav;
            }
        }

        void Start() {
            if (!closable) { return; }
            if(closeButton == null) {
                Debug.LogError("Close Button is not set in the inspector", transform);
                return;
            }
            
            closeButton.onClick.AddListener(() => {
                onCloseButtonPressed?.Invoke();
            });
        }

        /// <summary>
        /// Handles device change events to set the selected game object when a controller is connected.
        /// </summary>
        void OnDeviceChange(InputDevice device, InputDeviceChange change) {
            if (change == InputDeviceChange.Added && device is Gamepad) {
                _sceneEventSystem.SetSelectedGameObject(firstSelectedButton.gameObject);
            } else if (change == InputDeviceChange.Removed && device is Gamepad) {
                _sceneEventSystem.SetSelectedGameObject(null);
            }
        }

        void OnDisable() {
            // Unsubscribe from device change events
            InputSystem.onDeviceChange -= OnDeviceChange;
        }
    }
}
