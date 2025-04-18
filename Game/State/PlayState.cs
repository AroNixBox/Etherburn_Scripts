using Extensions.FSM;
using UnityEngine;

namespace Game.State {
    public class PlayState : IState {
        readonly GameBrain _gameBrain;
        readonly Player.Input.InputReader _inputReader;
        
        // Prevent the Player from going back into optionsstate/ menustate when already died, wait for game over
        public bool IsGameUnInitializing { get; private set; }
        
        TargetEntitiesUnregisteredChannel _targetEntitiesUnregisteredChannel;
        TargetEntitiesUnregisteredChannel.TargetEntitiesUnregisteredChannelEventHandler _uninitializeGameHandler;
        public PlayState(Player.Input.InputReader inputReader, GameBrain gameBrain) {
            _gameBrain = gameBrain;
            _inputReader = inputReader;
        }
        public void OnEnter() {
            // One of the two entry conditons,
            // the other one is the pausetriggered and gets reset in PauseState OnExit
            _gameBrain.PlayTriggered = false;
            
            _inputReader.SwitchActionMap(Player.Input.InputReader.ActionMapName.Player);
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            _gameBrain.menuCamera.gameObject.SetActive(false);
            
            WaitForInitialization();
        }
        
        async void WaitForInitialization() {
            await EntityManager.Instance.WaitTillInitialized();
            _ = EntityManager.Instance
                .GetEntityOfType(EntityType.Player, out _targetEntitiesUnregisteredChannel).transform;

            _uninitializeGameHandler = UninitializeGameHandler;
            _targetEntitiesUnregisteredChannel.RegisterListener(_uninitializeGameHandler);
        }

        void UninitializeGameHandler() {
            IsGameUnInitializing = true;
            _ = _gameBrain.UninitializeGame();
        }

        public void Tick() { }

        public void FixedTick() { }

        public void OnExit() {
            IsGameUnInitializing = false;
            
            // We need to unregister here, because we can only die in PlayState!            
            if (_uninitializeGameHandler == null) { return; }
            if(_targetEntitiesUnregisteredChannel == null) { return; }
            _targetEntitiesUnregisteredChannel.UnregisterListener(_uninitializeGameHandler);
        }
    }
}