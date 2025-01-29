using Game;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Menu {
    public class MainMenuNavigation : MonoBehaviour {
        [Header("References")]
        [SerializeField, Required] SceneLoader sceneLoader;
        
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
            
            _sceneEventSystem.SetSelectedGameObject(startGameButton.gameObject);
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
            
            // Make the call to the SceneLoader to load the first level
            sceneLoader.StartCoroutine(sceneLoader.LoadScenesAsync(SceneData.ELevelType.Level_One));
        }
        void OpenOptions() {
            optionsPanel.SetActive(true);
            
            // Deactivate the main menu panel including self    
            mainMenuPanel.SetActive(false);
        }
        void CloseGame() {
            Application.Quit();
        }
    }
}
