using System.Linq;
using UnityEngine.InputSystem;

namespace Player.Input {
    public static class InputUtils {
        public static bool IsUsingController() {
            // Check if any gamepad is connected
            return Gamepad.all.Count > 0 &&
                   // Check if the last input was from a gamepad
                   Gamepad.all.Any(gamepad => gamepad.lastUpdateTime > Keyboard.current.lastUpdateTime);
        }
        
        public static bool IsControllerConnected() {
            return Gamepad.all.Count > 0;
        }
    }
}