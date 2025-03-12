using Extensions.FSM;
using UnityEngine;

namespace Game.State {
    public class PauseState : IState {
        readonly GameBrain _gameBrain;
        readonly Player.Input.InputReader _inputReader;
        
        TargetEntitiesUnregisteredChannel _targetEntitiesUnregisteredChannel;
        TargetEntitiesUnregisteredChannel.TargetEntitiesUnregisteredChannelEventHandler _uninitializeGameHandler;
        public bool ReadyForMainMenu { get; private set; }
        // Prevent the Player from going back into playstate/ menustate when already died, wait for game over
        public bool IsGameUnInitializing { get; private set; }

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

            var entityManager = EntityManager.Instance;
            if (entityManager != null) {
                WaitForInitialization(entityManager);
            }
        }
        
        async void WaitForInitialization(EntityManager entityManager) {
            await entityManager.WaitTillInitialized();
            _ = entityManager
                .GetEntityOfType(EntityType.Player, out _targetEntitiesUnregisteredChannel).transform;

            _uninitializeGameHandler = UninitializeGameHandler;
            _targetEntitiesUnregisteredChannel.RegisterListener(_uninitializeGameHandler);
        }
        
        void UninitializeGameHandler() {
            IsGameUnInitializing = true;
            _ = _gameBrain.UninitializeGame();
        }

        public void Tick() {
            WaitForExit();
        }
        
        async void WaitForExit() {
            if(_gameBrain.HomePressed) {
                _gameBrain.HomePressed = false;
                if (SceneLoader.Instance.IsInLevel()) {
                    await _gameBrain.UninitializeGame(false);
                }
                ReadyForMainMenu = true;
            }
        }

        public void FixedTick() { }

        public void OnExit() {
            SceneLoader.Instance.UnloadScene(SceneData.EUISceneType.PauseMenu);
            
            // Exit Condition is triggered after the PauseToggleTriggered is set to true,
            // so we can directly reset it here.
            _gameBrain.PauseToggleTriggered = false;
            ReadyForMainMenu = false;
            
            IsGameUnInitializing = false;
                
            if (_uninitializeGameHandler == null) { return; }
            if(_targetEntitiesUnregisteredChannel == null) { return; }
            _targetEntitiesUnregisteredChannel.UnregisterListener(_uninitializeGameHandler);
        }
    }
}
