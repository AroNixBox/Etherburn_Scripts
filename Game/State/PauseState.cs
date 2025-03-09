using Extensions.FSM;
using UnityEngine;

namespace Game.State {
    public class PauseState : IState {
        readonly GameBrain _gameBrain;
        readonly Player.Input.InputReader _inputReader;

        public PauseState(Player.Input.InputReader inputReader, GameBrain gameBrain) {
            _gameBrain = gameBrain;
            _inputReader = inputReader;
        }
        public void OnEnter() {
            _gameBrain.PauseToggleTriggered = false;
            _inputReader.SwitchActionMap(Player.Input.InputReader.ActionMapName.UI);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // TODO: Load Options Scene
            // Go into Main Menu Scene if not loaded
            if (!SceneLoader.Instance.IsInUIScene(SceneData.EUISceneType.PauseMenu)) {
                SceneLoader.Instance.LoadSceneAsync(SceneData.EUISceneType.PauseMenu);
            }
        }

        public void Tick() { }

        public void FixedTick() { }

        public void OnExit() {
            SceneLoader.Instance.UnloadScene(SceneData.EUISceneType.PauseMenu);
            
            // Exit Condition is triggered after the PauseToggleTriggered is set to true,
            // so we can directly reset it here.
            _gameBrain.PauseToggleTriggered = false;
            
            if(_gameBrain.HomePressed) {
                if (SceneLoader.Instance.IsInLevel()) {
                    _gameBrain.UninitializeGame(false);
                }
                _gameBrain.HomePressed = false;
            }
        }
    }
}
