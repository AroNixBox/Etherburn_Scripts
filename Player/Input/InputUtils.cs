using System.Linq;
using UnityEngine.InputSystem;

namespace Player.Input {
    public static class InputUtils {
        public static bool WasLastInputController() {
            var lastInputTime = Keyboard.current.lastUpdateTime;

            if (Mouse.current.lastUpdateTime > lastInputTime) {
                lastInputTime = Mouse.current.lastUpdateTime;
            }

            return Gamepad.all.Count > 0 && Gamepad.all.Any(gamepad => gamepad.lastUpdateTime > lastInputTime);
        }
        
        public static bool IsControllerConnected() {
            return Gamepad.all.Count > 0;
        }
    }
}