using Extensions.FSM;
using Player.Input;
using UnityEngine;

namespace Game.State {
    public class GameOverState : IState{
        readonly GameBrain _gameBrain;
        readonly InputReader _inputReader;

        public GameOverState(InputReader inputReader, GameBrain gameBrain) {
            _inputReader = inputReader;
            _gameBrain = gameBrain;
        }
        public void OnEnter() {
            // Entry Condition
            _gameBrain.GameOverTriggered = false;
            
            SceneLoader.Instance.LoadSceneAsync(SceneData.EUISceneType.GameOver);
            
            _inputReader.SwitchActionMap(InputReader.ActionMapName.UI);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            _gameBrain.menuCamera.gameObject.SetActive(true);
        }

        public void Tick() {
            
        }

        public void FixedTick() {
            
        }

        public void OnExit() {
            SceneLoader.Instance.UnloadScene(SceneData.EUISceneType.GameOver);
            
            // Quit Untrigger
            _gameBrain.QuitTriggered = false;
        }
    }
}