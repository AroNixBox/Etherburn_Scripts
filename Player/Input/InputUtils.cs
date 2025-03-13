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
        
        public static bool IsPlaystationControllerConnected(string deviceLayoutName) {
            return Gamepad.all.Count > 0 && (
                InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "DualShockGamepad")
                || InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "DualShock4GamepadHID") 
                || InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "PS5DualSenseGamepad") 
                || InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "DualSenseGamepadHID") 
                || InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "XInputControllerWindows") 
            );
        }
        
        public static bool IsXboxControllerConnected(string deviceLayoutName) {
            return Gamepad.all.Count > 0 && InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Gamepad");
        }
        
        public static bool IsSwitchControllerConnected(string deviceLayoutName) {
            return Gamepad.all.Count > 0 && InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "SwitchProControllerHID");
        }
        
        public static bool WasLastInputKeyboardAndMouse() {
            return Keyboard.current.lastUpdateTime > Mouse.current.lastUpdateTime;
        }
        
        public static bool IsControllerConnected() {
            return Gamepad.all.Count > 0;
        }
        

        /// <param name="inputAction">Make sure you dont accidently pass in InputActionReference that is implicitly converted to an InputAction. Will not return the rebinded Key then.</param>
        public static string GetBindingFancyName(InputAction inputAction, int bindingIndex, string controlPath, string deviceLayoutName) {
            if (inputAction == null) return "Invalid action";
            
            var binding = inputAction.bindings[bindingIndex];
            
            UnityEngine.Debug.Log($"Binding: {binding.name}, ControlPath: {controlPath}, DeviceLayoutName: {deviceLayoutName}");
            
            // Check if this is part of a composite, so we can return all bindings inside the composite
            if (binding.isComposite) {
                // Find all parts of this composite and concatenate them
                string compositeName = "";
                // Start from the next binding (first part)
                int partIndex = bindingIndex + 1;
                
                // Collect all parts of the composite
                while (partIndex < inputAction.bindings.Count && inputAction.bindings[partIndex].isPartOfComposite) {
                    // Get the part's display name
                    inputAction.GetBindingDisplayString(partIndex, out var partDeviceLayout, out var partControlPath);
                    
                    string partName = GetSingleBindingName(inputAction.bindings[partIndex], partControlPath, partDeviceLayout);
                    string partBindingName = inputAction.bindings[partIndex].name;
                    
                    // Add separator if not the first part
                    if (!string.IsNullOrEmpty(compositeName)) {
                        compositeName += " / ";
                    }
                    compositeName += partBindingName + ": " + partName;
                    
                    partIndex++;
                }
                
                return compositeName;
            }
            
            // Handle part of composite (when specifically requesting a single part)
            if (binding.isPartOfComposite) {
                return binding.name + ": " + GetSingleBindingName(binding, controlPath, deviceLayoutName);
            }
            
            // Handle normal bindings
            return GetSingleBindingName(binding, controlPath, deviceLayoutName);
        }
        
        // Helper method to get name for a single binding
        static string GetSingleBindingName(InputBinding binding, string controlPath, string deviceLayoutName) {
            // bindigs.groups can start with e.g. "GamepadOrKeyboard&Mouse;Gamepad" And then there can also be multiple split by ;
            // Same thing goes for keyboard&mouse
            var isGamepad = binding.groups != null && binding.groups.Split(';')
                                .Any(group => group is "Gamepad");
            var isKeyboardAndMouse = binding.groups != null && binding.groups.Split(';')
                                .Any(group => group == "Keyboard&Mouse");
            
            if(!isGamepad && !isKeyboardAndMouse) {
                UnityEngine.Debug.Log("<color=red><b>binding groups: " + binding.groups + "</b></color>");
            }
            
            if (isGamepad) {
                if (IsPlaystationControllerConnected(deviceLayoutName)) {
                    return MapToPlayStationControl(controlPath);
                }
                if (IsXboxControllerConnected(deviceLayoutName)) {
                    return MapToXboxControl(controlPath);
                }
                if (IsSwitchControllerConnected(deviceLayoutName)) {
                    return MapToSwitchControl(controlPath);
                }
                return controlPath;
            }
            
            return isKeyboardAndMouse ? MapToKeyboardAndMouseControl(controlPath) : controlPath;
        }
        public static int GetNextNonCompositeChildIndex(InputActionReference inputActionReference, int startIndex) {
            var bindingIndex = startIndex;
            while (IsChildOfComposite(inputActionReference, bindingIndex)) {
                bindingIndex++;
            }
            return bindingIndex;
        }
        static bool IsChildOfComposite(InputActionReference inputActionReference, int bindingIndex) {
            return inputActionReference.action.bindings[bindingIndex].isPartOfComposite;
        }
        public static int GetBindingIndex(InputActionReference inputActionReference, EDeviceType deviceType) {
            var action = inputActionReference.action;

            string deviceName = GetDeviceName(deviceType);

            // First pass: Look for direct non-composite matches
            for (int i = 0; i < action.bindings.Count; i++) {
                var binding = action.bindings[i];
    
                if (binding.isPartOfComposite) {
                    continue;
                }
    
                if (!binding.isComposite && binding.groups != null && 
                    binding.groups.Split(';').Any(group => group == deviceName)) {
                    return i;
                }
            }

            // Second pass: Check composite parts for the device type
            for (int i = 0; i < action.bindings.Count; i++) {
                var binding = action.bindings[i];
        
                if (binding is { isPartOfComposite: true, groups: not null } && 
                    binding.groups.Split(';').Any(group => group == deviceName)) {
                    // Found a matching composite part, find its parent composite
                    int parentIndex = i;
                    while (parentIndex > 0 && !action.bindings[parentIndex].isComposite) {
                        parentIndex--;
                    }
            
                    if (action.bindings[parentIndex].isComposite) {
                        return parentIndex; // Return parent composite's index
                    }
                }
            }

            // No matching binding found
            return -1;
        }
        
        static string GetDeviceName(EDeviceType deviceType) {
            return deviceType switch {
                EDeviceType.Gamepad => "Gamepad",
                EDeviceType.KeyboardAndMouse => "Keyboard&Mouse",
                _ => null
            };
        }
        public static EDeviceType[] GetDeviceTypes() {
            return new[] { EDeviceType.Gamepad, EDeviceType.KeyboardAndMouse };
        }

        public enum EDeviceType {
            KeyboardAndMouse,
            Gamepad
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