using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.Menu {
    public class RebindButton : MonoBehaviour {
        [SerializeField] Button rebindButton;
        [SerializeField] InputActionReference inputActionReference;
        [SerializeField] bool excludeMouse;
        
        [SerializeField] RebindHandler rebindHandler;
        string _inputActionName;
        
        void Start() {
            _inputActionName = inputActionReference.action.name;
            
            rebindButton.onClick.AddListener(Rebind);
        }

        void Rebind() {
            var lastDeviceIndex = GetLastActiveDevice();
            rebindHandler.StartRebind(_inputActionName, lastDeviceIndex, excludeMouse);
        }
        
        // Determine if the last active device was a keyboard/mouse or gamepad
        int GetLastActiveDevice() {
            var lastActiveDevice = InputSystem.devices.OrderByDescending(d => d.lastUpdateTime).FirstOrDefault();
            return lastActiveDevice switch {
                Gamepad => 1,
                Keyboard or Mouse => 0,
                _ => -1
            };
        }
    }
}
