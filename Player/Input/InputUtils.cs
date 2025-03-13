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
        
        public static bool WasLastInputPlayStationController(string deviceLayoutName) {
            return Gamepad.all.Count > 0 && InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "DualShockGamepad");
        }
        
        public static bool WasLastInputXboxController(string deviceLayoutName) {
            return Gamepad.all.Count > 0 && InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Gamepad");
        }
        
        public static bool WasLastInputSwitchController(string deviceLayoutName) {
            return Gamepad.all.Count > 0 && InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "SwitchProControllerHID");
        }
        
        public static bool WasLastInputKeyboardAndMouse() {
            return Keyboard.current.lastUpdateTime > Mouse.current.lastUpdateTime;
        }
        
        public static bool IsControllerConnected() {
            return Gamepad.all.Count > 0;
        }
        
        public static string MapToPlayStationControl(string controlPath) {
            return controlPath switch {
                "buttonSouth" => "⊙",
                "buttonNorth" => "▲",
                "buttonEast" => "○",
                "buttonWest" => "✖",
                "start" => "start ⏸",
                "select" => "select ☰",
                "leftTrigger" => "L2",
                "rightTrigger" => "R2",
                "leftShoulder" => "L1",
                "rightShoulder" => "R1",
                "dpad" => "dpad",
                "dpad/up" => "dpad ⬆",
                "dpad/down" => "dpad ⬇",
                "dpad/left" => "dpad ⬅",
                "dpad/right" => "dpad ➡",
                "leftStick" => "left stick",
                "rightStick" => "right stick",
                "leftStickPress" => "left stick press",
                "rightStickPress" => "right stick press",
                _ => null
            };
        }
        public static string MapToXboxControl(string controlPath) {
            return controlPath switch {
                "buttonSouth" => "A",
                "buttonNorth" => "Y",
                "buttonEast" => "B",
                "buttonWest" => "X",
                "start" => "Menu ≡",
                "select" => "View ⧉",
                "leftTrigger" => "LT",
                "rightTrigger" => "RT",
                "leftShoulder" => "LB",
                "rightShoulder" => "RB",
                "dpad" => "D-pad",
                "dpad/up" => "D-pad ⬆",
                "dpad/down" => "D-pad ⬇",
                "dpad/left" => "D-pad ⬅",
                "dpad/right" => "D-pad ➡",
                "leftStick" => "left stick",
                "rightStick" => "right stick",
                "leftStickPress" => "left stick press",
                "rightStickPress" => "right stick press",
                _ => null
            };
        }
        
        public static string MapToSwitchControl(string controlPath) {
            return controlPath switch {
                "buttonSouth" => "B",
                "buttonNorth" => "X", 
                "buttonEast" => "A",
                "buttonWest" => "Y",
                "start" => "+ Button",
                "select" => "- Button",
                "leftTrigger" => "ZL",
                "rightTrigger" => "ZR",
                "leftShoulder" => "L",
                "rightShoulder" => "R",
                "dpad" => "D-pad",
                "dpad/up" => "D-pad ⬆",
                "dpad/down" => "D-pad ⬇",
                "dpad/left" => "D-pad ⬅",
                "dpad/right" => "D-pad ➡",
                "leftStick" => "left stick",
                "rightStick" => "right stick",
                "leftStickPress" => "left stick press",
                "rightStickPress" => "right stick press",
                _ => null
            };
        }
        
        public static string MapToKeyboardAndMouseControl(string controlPath) {
            return controlPath switch {
                // Mouse Buttons with clearer names and symbols
                "leftButton" => "Mouse Left Button",
                "rightButton" => "Mouse Right Button",
                "middleButton" => "Mouse Middle Button",
                "backButton" => "Mouse Thumb Button 1",
                "forwardButton" => "Mouse Thumb Button 2",
        
                // Additional Mouse Buttons (for gaming mice)
                "button4" => "🖱️ Button 4",
                "button5" => "🖱️ Button 5",
                "button6" => "🖱️ Button 6",
                "button7" => "🖱️ Button 7",
                "button8" => "🖱️ Button 8",
                "button9" => "🖱️ Button 9",
                "button10" => "🖱️ Button 10",
        
                // Mouse Movement with symbols
                "position" => "Mouse Position",
                "delta" => "Mouse Movement",
                "scroll" => "Mouse Scroll",
                "scroll/up" => "Mouse Wheel ↑", 
                "scroll/down" => "Mouse Wheel ↓",
                "scroll/left" => "Mouse Wheel ←",
                "scroll/right" => "Mouse Wheel →",
        
                // Keyboard Keys - Letters
                "a" => "A",
                "b" => "B",
                "c" => "C",
                "d" => "D",
                "e" => "E",
                "f" => "F",
                "g" => "G",
                "h" => "H",
                "i" => "I",
                "j" => "J",
                "k" => "K",
                "l" => "L",
                "m" => "M",
                "n" => "N",
                "o" => "O",
                "p" => "P",
                "q" => "Q",
                "r" => "R",
                "s" => "S",
                "t" => "T",
                "u" => "U",
                "v" => "V",
                "w" => "W",
                "x" => "X",
                "y" => "Y",
                "z" => "Z",
                
                // Keyboard Keys - Numbers
                "1" => "1",
                "2" => "2",
                "3" => "3",
                "4" => "4",
                "5" => "5",
                "6" => "6",
                "7" => "7",
                "8" => "8",
                "9" => "9",
                "0" => "0",
                
                // Keyboard Keys - Function keys
                "f1" => "F1",
                "f2" => "F2",
                "f3" => "F3",
                "f4" => "F4",
                "f5" => "F5",
                "f6" => "F6",
                "f7" => "F7",
                "f8" => "F8",
                "f9" => "F9",
                "f10" => "F10",
                "f11" => "F11",
                "f12" => "F12",
                
                // Keyboard Keys - Special keys
                "escape" => "Esc",
                "tab" => "Tab",
                "shift" => "Left Shift ↑",
                "rightShift" => "Right Shift ↑",
                "leftCtrl" => "Left Ctrl",
                "rightCtrl" => "Right Ctrl",
                "leftAlt" => "Left Alt",
                "rightAlt" => "Right Alt",
                "space" => "Space",
                "enter" => "Enter",
                "backspace" => "Backspace",
                "delete" => "Delete",
                "pageUp" => "Page Up",
                "pageDown" => "Page Down",
                "home" => "Home",
                "end" => "End",
                "insert" => "Insert",
                "numpadEnter" => "Numpad Enter",
                
                // Keyboard Keys - Arrow keys
                "upArrow" => "↑",
                "downArrow" => "↓",
                "leftArrow" => "←",
                "rightArrow" => "→",
                
                // Numeric Keypad
                "numpad0" => "Numpad 0",
                "numpad1" => "Numpad 1",
                "numpad2" => "Numpad 2",
                "numpad3" => "Numpad 3",
                "numpad4" => "Numpad 4",
                "numpad5" => "Numpad 5",
                "numpad6" => "Numpad 6",
                "numpad7" => "Numpad 7",
                "numpad8" => "Numpad 8",
                "numpad9" => "Numpad 9",
                "numpadDivide" => "Numpad /",
                "numpadMultiply" => "Numpad *",
                "numpadMinus" => "Numpad -",
                "numpadPlus" => "Numpad +",
                "numpadPeriod" => "Numpad .",
        
                _ => controlPath
            };
        }
    }
}