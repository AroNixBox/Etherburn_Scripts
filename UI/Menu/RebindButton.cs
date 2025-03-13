using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.Menu {
    [RequireComponent(typeof(Button))]
    public class RebindButton : MonoBehaviour {
        [Title("Settings")]
        [SerializeField] DeviceType deviceType;
        
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
            var bindingIndex = GetNextNonCompositeChildIndex(_inputActionName, (int)deviceType);
            var inputAction = _rebindHandler.GetAction(_inputActionReference);
            
            _ = inputAction.GetBindingDisplayString(
                bindingIndex, 
                // Name of the device layout that the binding is using (Gamepad, Keyboard&Mouse, etc.)
                out var deviceLayoutName, 
                out var controlPath, 
                InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
            
            // For Future: Icons instead of text
            var bindingName = _rebindHandler.GetBindingName(_inputActionReference, bindingIndex, controlPath, deviceLayoutName);

            bindingDisplayText.text = bindingName;
        }
        void Rebind() {
            var bindingIndex = GetNextNonCompositeChildIndex(_inputActionName, (int)deviceType);
            _rebindHandler.StartRebind(_inputActionName, bindingIndex, SetBindingDisplayText);
        }
        int GetNextNonCompositeChildIndex(string actionName, int startIndex) {
            var bindingIndex = startIndex;
            while (_rebindHandler.IsChildOfComposite(actionName, bindingIndex)) {
                bindingIndex++;
            }
            return bindingIndex;
        }

        enum DeviceType {
            None = -1,
            KeyboardAndMouse = 0,
            Gamepad = 1,
        }
    }
}
