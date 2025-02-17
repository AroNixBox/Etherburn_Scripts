using Extensions.FSM;
using UnityEngine;

namespace Game.State {
    public class PauseState : IState {
        readonly GameBrain _gameBrain;
        readonly Player.Input.InputReader _inputReader;
        readonly UI.Menu.OptionMenuNavigation _optionMenuNavigation;

        public PauseState(Player.Input.InputReader inputReader, GameBrain gameBrain, UI.Menu.OptionMenuNavigation optionMenuNavigation) {
            _optionMenuNavigation = optionMenuNavigation;
            _gameBrain = gameBrain;
            _inputReader = inputReader;
        }
        public void OnEnter() {
            _gameBrain.PauseToggleTriggered = false;
            _inputReader.SwitchActionMap(Player.Input.InputReader.ActionMapName.UI);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            _optionMenuNavigation.mainOptionsPanel.gameObject.SetActive(true);
            
            // TODO: Close Main Menu Panel if open
        }

        public void Tick() { }

        public void FixedTick() { }

        public void OnExit() {
            _optionMenuNavigation.mainOptionsPanel.gameObject.SetActive(false);
            _optionMenuNavigation.systemPanel.gameObject.SetActive(false);
            _optionMenuNavigation.controlsPanel.gameObject.SetActive(false);
            
            // Exit Condition is triggered after the PauseToggleTriggered is set to true,
            // so we can directly reset it here.
            _gameBrain.PauseToggleTriggered = false;
        }
    }
}
