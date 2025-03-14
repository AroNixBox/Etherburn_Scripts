using Extensions.FSM;
using Player.Input;
using UnityEngine;

namespace Game.State {
    public class GameOverState : IState{
        readonly GameBrain _gameBrain;
        readonly InputReader _inputReader;
        readonly GameOverType _gameOverType;

        public enum GameOverType {
            Lost,
            Won
        }
        SceneData.EUISceneType GetGameOverType() {
            return _gameOverType switch {
                GameOverType.Lost => SceneData.EUISceneType.GameOver,
                GameOverType.Won => SceneData.EUISceneType.WinScreen,
                _ => SceneData.EUISceneType.GameOver
            };
        }
        
        public GameOverState(InputReader inputReader, GameBrain gameBrain, GameOverType gameOverType) {
            _inputReader = inputReader;
            _gameBrain = gameBrain;
            _gameOverType = gameOverType;
        }
        public void OnEnter() {
            // Entry Condition
            _gameBrain.GameOverTriggered = false;
            _gameBrain.WinTriggered = false;
            
            SceneLoader.Instance.LoadSceneAsync(GetGameOverType());
            
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
            SceneLoader.Instance.UnloadScene(GetGameOverType());
            
            // Quit Untrigger
            _gameBrain.QuitTriggered = false;
        }
    }
}