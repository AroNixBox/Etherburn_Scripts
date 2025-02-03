using Game;
using Player.Input;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.Menu {
    public class MainMenuNavigation : MonoBehaviour {
        [Header("User Interface")] 
        [Title("Buttons")] 
        [SerializeField, Required] Button startGameButton;
        [SerializeField, Required] Button optionsButton;
        [SerializeField, Required] Button quitButton;

        // Panels hardcoded, since there is only one Main Menu
        [Title("Panels")] 
        [SerializeField, Required] GameObject mainMenuPanel;
        [SerializeField, Required] GameObject optionsPanel;

        EventSystem _sceneEventSystem;

        // Assuming this gets disabled when another window pops up and re-enabled when its opened
        void OnEnable() {
            _sceneEventSystem ??= EventSystem.current;

            if (_sceneEventSystem == null) {
                Debug.LogError("No EventSystem found in scene");
                return;
            }

            // Check if a controller is connected
            if (InputUtils.IsControllerConnected()) {
                _sceneEventSystem.SetSelectedGameObject(startGameButton.gameObject);
            }

            // Subscribe to device change events
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        void OnDisable() {
            // Unsubscribe from device change events
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        /// <summary>
        /// Handles device change events to set the selected game object when a controller is connected.
        /// </summary>
        void OnDeviceChange(InputDevice device, InputDeviceChange change) {
            if (change == InputDeviceChange.Added && device is Gamepad) {
                _sceneEventSystem.SetSelectedGameObject(startGameButton.gameObject);
            } else if (change == InputDeviceChange.Removed && device is Gamepad) {
                _sceneEventSystem.SetSelectedGameObject(null);
            }
        }

        void Start() {
            SetupButtonNavigation();
        }

        void SetupButtonNavigation() {
            startGameButton.onClick.AddListener(StartGame);
            optionsButton.onClick.AddListener(OpenOptions);
            quitButton.onClick.AddListener(CloseGame);
        }
        void StartGame() { 
            // Disable all UI elements
            mainMenuPanel.SetActive(false);
            
            var sceneLoader = SceneLoader.Instance;
            if(sceneLoader == null) {
                Debug.LogError("SceneLoader is not in the scene", transform);
                return;
            }
            
            // Make the call to the SceneLoader to load the first level
            sceneLoader.StartCoroutine(sceneLoader.LoadScenesAsync(SceneData.ELevelType.Level_One));
        }
        void OpenOptions() {
            optionsPanel.SetActive(true);
            
            // Deactivate the main menu panel including self    
            mainMenuPanel.SetActive(false);
        }

        static void CloseGame() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
