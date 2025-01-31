using Player.Input;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.Menu {
    public class OptionMenuNavigation : MonoBehaviour {
        [Header("User Interface")]
        [Title("Buttons")] 
        [SerializeField, Required] Button firstSelectedOptionsButton;
        [SerializeField, Required] Button closeButton;
        
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
            
            // Check if a controller is connected
            if (InputUtils.IsUsingController()) {
                _sceneEventSystem.SetSelectedGameObject(firstSelectedOptionsButton.gameObject);
            }

            // Subscribe to device change events
            InputSystem.onDeviceChange += OnDeviceChange;
        }
        
        void Start() {
            SetupButtonNavigation();
        }

        /// <summary>
        /// Handles device change events to set the selected game object when a controller is connected.
        /// </summary>
        void OnDeviceChange(InputDevice device, InputDeviceChange change) {
            if (change == InputDeviceChange.Added && device is Gamepad) {
                _sceneEventSystem.SetSelectedGameObject(firstSelectedOptionsButton.gameObject);
            } else if (change == InputDeviceChange.Removed && device is Gamepad) {
                _sceneEventSystem.SetSelectedGameObject(null);
            }
        }

        void SetupButtonNavigation() {
            closeButton.onClick.AddListener(CloseMenu);
        }
        
        void CloseMenu() {
            // Invoke the event
            onCloseButtonPressed.Invoke();
        }
        
        
        void OnDisable() {
            // Unsubscribe from device change events
            InputSystem.onDeviceChange -= OnDeviceChange;
        }
    }
}
