using Extensions.FSM;
using Player.Input;
using UnityEngine;

namespace Game.State {
    public class MenuState : IState{
        readonly GameBrain _gameBrain;
        readonly InputReader _inputReader;

        public MenuState(InputReader inputReader, GameBrain gameBrain) {
            _inputReader = inputReader;
            _gameBrain = gameBrain;
        }
        public void OnEnter() {
            // Entry Conditions
            _gameBrain.PauseToggleTriggered = false;
            
            // Go into Main Menu Scene if not loaded
            if (!SceneLoader.Instance.IsInUIScene(SceneData.EUISceneType.MainMenu)) {
                SceneLoader.Instance.LoadSceneAsync(SceneData.EUISceneType.MainMenu);
            }
            
            _inputReader.SwitchActionMap(InputReader.ActionMapName.UI);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            _gameBrain.menuCamera.gameObject.SetActive(true);
            
            // Load our Save Data, cant do this in PlayState, due to its too late.
            var saveManager = Save.SaveManager.Instance;
            if (saveManager == null) {
                Debug.LogError("SaveManager is not set in the inspector");
                return;
            }
            
            saveManager.LoadSaveData();
        }

        public void Tick() {
            
        }

        public void FixedTick() {
            
        }

        public void OnExit() {
            SceneLoader.Instance.UnloadScene(SceneData.EUISceneType.MainMenu);
        }
    }
}