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
            
            if (inPauseMenu) {
                optionMenuNavigation.mainOptionsPanel.gameObject.SetActive(false);
                optionMenuNavigation.systemPanel.gameObject.SetActive(false);
                optionMenuNavigation.controlsPanel.gameObject.SetActive(false);
                
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else {
                optionMenuNavigation.mainOptionsPanel.gameObject.SetActive(true);
                
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        
        void OnDestroy() {
            _inputReader.Pause -= OnPause;
        }
    }
}
