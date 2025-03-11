using System;
using Game;
using Game.Save;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Menu {
    public class MainMenuNavigation : MonoBehaviour {
        [Header("User Interface")] 
        [Title("Buttons")] 
        [SerializeField, Required] Button startGameButton;
        [SerializeField, Required] Button startNewGameButton;
        [SerializeField, Required] Button optionsButton;
        [SerializeField, Required] Button quitButton;

        EventSystem _sceneEventSystem;

        void Start() {
            SetupButtonNavigation();
        }
        void SetupButtonNavigation() {
            startGameButton.onClick.AddListener(StartGame);
            startNewGameButton.onClick.AddListener(ClearSaveData);
            optionsButton.onClick.AddListener(OpenOptions);
            quitButton.onClick.AddListener(CloseGame);
        }

        void ClearSaveData() {
            var saveManager = SaveManager.Instance;
            if(saveManager == null) {
                Debug.LogError("SaveManager is not in the scene", transform);
                return;
            }
            
            if(saveManager.HasSaveData()) {
                saveManager.ClearSaveData();
            }
            
            StartGame();
        }

        void StartGame() { 
            var sceneLoader = SceneLoader.Instance;
            if(sceneLoader == null) {
                Debug.LogError("SceneLoader is not in the scene", transform);
                return;
            }
            
            // Make the call to the SceneLoader to load the first level
            sceneLoader.StartCoroutine(sceneLoader.LoadScenesAsync(SceneData.ELevelType.Level_One));
            
            gameObject.SetActive(false);
        }
        void OpenOptions() {
            var gameBrain = GameBrain.Instance;
            if(gameBrain == null) {
                Debug.LogError("GameBrain is not in the scene", transform);
                return;
            }
            gameBrain.PauseToggleTriggered = true;
            
            // Deactivate the main menu panel including self    
            gameObject.SetActive(false);
        }

        static void CloseGame() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void OnDestroy() {
            startGameButton.onClick.RemoveListener(StartGame);
            startNewGameButton.onClick.RemoveListener(ClearSaveData);
            optionsButton.onClick.RemoveListener(OpenOptions);
            quitButton.onClick.RemoveListener(CloseGame);
        }
    }
}
