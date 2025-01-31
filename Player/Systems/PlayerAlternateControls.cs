using Player.Input;
using UI.Menu;
using UnityEngine;

namespace Player.Systems {
    public class PlayerAlternateControls : MonoBehaviour {
        [SerializeField] References references;
        [SerializeField] OptionMenuNavigation optionMenuNavigation;
        
        InputReader _inputReader;
        
        void Start() {
            _inputReader = references.input;
            _inputReader.Pause += OnPause;
        }

        public void OnPause() {
            //Time.timeScale = arg0 ? 0 : 1;
            var inPauseMenu = optionMenuNavigation.IsAnyPanelActive();
            var actionMap = inPauseMenu ? InputReader.ActionMapName.Player : InputReader.ActionMapName.UI;
            _inputReader.SwitchActionMap(actionMap);
            
            if (InputUtils.WasLastInputController()) {
                // Enable Mouse and Unlock Cursor if not in pause menu
                Cursor.lockState = inPauseMenu ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !inPauseMenu;
            }
            
            if (inPauseMenu) {
                optionMenuNavigation.mainOptionsPanel.gameObject.SetActive(false);
                optionMenuNavigation.systemPanel.gameObject.SetActive(false);
                optionMenuNavigation.controlsPanel.gameObject.SetActive(false);
            }
            else {
                
                
                optionMenuNavigation.mainOptionsPanel.gameObject.SetActive(true);
            }
        }
        
        void OnDestroy() {
            _inputReader.Pause -= OnPause;
        }
    }
}
