using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Menu {
    public class OptionMenuNavigation : MonoBehaviour {
        [Header("User Interface")]
        [Title("Buttons")] 
        [SerializeField, Required] Button firstSelectedOptionsButton;
        [SerializeField, Required] Button closeButton;

        [Title("Panels")] 
        [SerializeField, Required] GameObject optionsPanel;
        
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
            
            _sceneEventSystem.SetSelectedGameObject(firstSelectedOptionsButton.gameObject);
        }

        void Start() {
            SetupButtonNavigation();
            
            // Find all RebindButtons in children
        }

        void SetupButtonNavigation() {
            closeButton.onClick.AddListener(CloseMenu);
        }
        
        void CloseMenu() {
            // Invoke the event
            onCloseButtonPressed.Invoke();
        }
    }
}
