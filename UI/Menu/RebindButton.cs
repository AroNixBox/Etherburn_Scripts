using Player.Input;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.Menu {
    [RequireComponent(typeof(Button))]
    public class RebindButton : MonoBehaviour {
        [Title("Settings")]
        [SerializeField] InputUtils.EDeviceType deviceType;
        
        [Title("References")]
        [SerializeField] TMP_Text bindingDisplayText;
        
        RebindHandler _rebindHandler;
        InputActionReference _inputActionReference;
        Button _rebindButton;
        string _inputActionName;
        public void Initialize(InputActionReference inputActionReference, RebindHandler rebindHandler) {
            _rebindHandler = rebindHandler;
            _inputActionReference = inputActionReference;
            _inputActionName = _inputActionReference.action.name;
            
            _rebindButton = GetComponent<Button>();
            _rebindButton.onClick.AddListener(Rebind);
            
            SetBindingDisplayText();
        }
        void SetBindingDisplayText() {
            var bindingIndex = InputUtils.GetBindingIndex(_inputActionReference, deviceType);
            var inputAction = _rebindHandler.GetAction(_inputActionReference);
            
            _ = inputAction.GetBindingDisplayString(
                bindingIndex, 
                // Name of the device layout that the binding is using (Gamepad, Keyboard&Mouse, etc.)
                out var deviceLayoutName, 
                out var controlPath, 
                InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
            
            // For Future: Icons instead of text
            var bindingName = InputUtils.GetBindingFancyName(inputAction, bindingIndex, controlPath, deviceLayoutName);

            bindingDisplayText.text = bindingName;
        }
        void Rebind() {
            // Get the binding index of the action combined with the checkmark in the inputactionasset
            var bindingIndex = InputUtils.GetBindingIndex(_inputActionReference, deviceType);
            _rebindHandler.StartRebind(_inputActionName, bindingIndex, SetBindingDisplayText);
        }
        
        void OnDestroy() {
            if (_rebindButton != null) {
                _rebindButton.onClick.RemoveListener(Rebind);
            }
        }
    }
}
