using System;
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
        [SerializeField, Required] Button closeButton;
        [SerializeField, Required] Button firstSelectedButton;
        
        [Header("Events")]
        [SerializeField] UnityEvent onCloseButtonPressed;
        
        EventSystem _sceneEventSystem;

        // Assuming this gets disabled when another window pops up and re-enabled when its opened
        void OnEnable() {
            _sceneEventSystem ??= EventSystem.current;

            if (_sceneEventSystem == null) {
                Debug.LogError("No EventSystem found in scene");
                return;
            }
            
            // Check if the menu was opened with a controller
            if (InputUtils.WasLastInputController()) {
                _sceneEventSystem.SetSelectedGameObject(firstSelectedButton.gameObject);
            }

            // Subscribe to device change events
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        void Start() {
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
    }
}
