using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Player.Input {
    // Interface for input reading. Use this when you want to create different input readers
    public interface IInputReader {
        Vector2 Direction { get; }
        void EnablePlayerActions();
    }


    [CreateAssetMenu(fileName = "InputReader", menuName = "InputReader")]
    public class InputReader : ScriptableObject, PlayerInputActions.IPlayerActions, PlayerInputActions.IUIActions, PlayerInputActions.IGlobalActions, IInputReader {
        [SerializeField] ActionMapName initialActionMap = ActionMapName.Player;
        // The actual input actions asset. This will be initialized in EnablePlayerActions
        public PlayerInputActions InputActions { get; private set; }
        ActionMapName _currentActionMap;
        readonly Dictionary<ActionMapName, InputActionMap> _actionMaps = new();
        
        #region Player Map Input Action Callbacks

        // Events for different input actions. Subscribe to these in game logic
        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<Vector2, bool> Look = delegate { };
        public event UnityAction<bool> IsLooking = delegate { };
        public event UnityAction<bool> Dodge = delegate { };
        public event UnityAction<bool> Run = delegate { };
        public event UnityAction<bool> Attack = delegate { };
        public event UnityAction<bool> SecondAttack = delegate { };
        public event UnityAction<bool> Ultimate = delegate { };
        public event UnityAction LockOnTarget = delegate { };

        // Helper method to check if the jump key is currently pressed
        public bool IsJumpKeyPressed() => InputActions.Player.Jump.IsPressed();
        
        // Properties to get current movement and look directions
        public Vector2 Direction => InputActions.Player.Move.ReadValue<Vector2>();
        public Vector2 LookDirection => InputActions.Player.Look.ReadValue<Vector2>();
        
        #endregion
        
        #region UI Map Input Action Callbacks
        public event UnityAction<Vector2, bool> NavigateUI = delegate { };
        /// <summary> Tracks if we hit submit (enter) </summary>
        public event UnityAction SubmitUI = delegate { };
        /// <summary> Tracks if we hit cancel (escape) </summary>
        public event UnityAction CancelUI = delegate { };
        /// <summary> Tracks the position of the "look" - point input and if our Input device is a mouse </summary>
        public event UnityAction<Vector2, bool> PointUI = delegate { };
        /// <summary> Transports if we hit the UI with that click or not, can use if needed </summary>
        public event UnityAction<bool> ClickUI = delegate { };
        /// <summary>Transports the scroll wheel value </summary>
        public event UnityAction<Vector2> ScrollWheelUI = delegate { };

        /// <summary> Tracks if we clicked with the right mouse button </summary>
        public event UnityAction<bool> RightClickUI = delegate { };
        
        #endregion

        #region Global Map Input Action Callbacks

        /// <summary> Tracks if we clicked with the middle mouse button </summary>
        public event UnityAction<bool> MiddleClickUI = delegate { };

        #endregion 
        
        #region Player Map Input Action Reader Methods

        // The following methods are callbacks for different input actions
        // They're called automatically by the input system when the corresponding action occurs

        // Called when the player moves
        public void OnMove(InputAction.CallbackContext context) {
            Move.Invoke(context.ReadValue<Vector2>());
        }

        // Called when the player looks around
        public void OnLook(InputAction.CallbackContext context) {
            switch (context) {
                case {phase: InputActionPhase.Started}:
                    IsLooking.Invoke(true);
                    break;
                case {phase: InputActionPhase.Canceled}:
                    IsLooking.Invoke(false);
                    break;
                case {phase: InputActionPhase.Performed}:
                    Look.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
                    break;
            }
        }

        // Helper method to check if the input device is a mouse
        bool IsDeviceMouse(InputAction.CallbackContext context) {
            // Debug.Log($"Device name: {context.control.device.name}");
            return context.control.device.name == "Mouse";
        }

        // Called when the player fires (e.g., clicks)
        // Dont Call the Action on Performed, its redundant since we only need started and canceled
        public void OnFire(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    Attack.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Attack.Invoke(false);
                    break;
            }
        }

        public void OnSecondFire(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    SecondAttack.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    SecondAttack.Invoke(false);
                    break;
            }
        }

        public void OnUltimate(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    Ultimate.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Ultimate.Invoke(false);
                    break;
            }
        }

        // Called when the player starts or stops running
        public void OnRun(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    Run.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Run.Invoke(false);
                    break;
            }
        }

        // Called when the player starts or stops jumping
        public void OnJump(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    Dodge.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Dodge.Invoke(false);
                    break;
            }
        }

        public void OnLockOnTarget(InputAction.CallbackContext context) {
            if(context.phase == InputActionPhase.Started) {
                LockOnTarget.Invoke();
            }
        }

        #endregion

        #region UI Map Input Action Reader Methods

        public void OnNavigate(InputAction.CallbackContext context) {
            NavigateUI.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
        }

        public void OnSubmit(InputAction.CallbackContext context) {
            if(context.phase == InputActionPhase.Started) {
                SubmitUI.Invoke();
            }
        }

        public void OnCancel(InputAction.CallbackContext context) {
            if(context.phase == InputActionPhase.Started) {
                CancelUI.Invoke();
            }
        }

        public void OnPoint(InputAction.CallbackContext context) {
            // We are not only tracking the active state if moving the mouse, but always
            PointUI.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
        }

        public void OnClick(InputAction.CallbackContext context) {
            if(context.phase == InputActionPhase.Started) {
                ClickUI.Invoke(IsDeviceMouse(context));
            }
        }

        public void OnScrollWheel(InputAction.CallbackContext context) {
            ScrollWheelUI.Invoke(context.ReadValue<Vector2>());
        }
        public void OnRightClick(InputAction.CallbackContext context) {
            if (context.phase == InputActionPhase.Started) {
                RightClickUI.Invoke(context.phase == InputActionPhase.Started);
            }
        }

        // Not used, not VR
        public void OnTrackedDevicePosition(InputAction.CallbackContext context) { }

        public void OnTrackedDeviceOrientation(InputAction.CallbackContext context) { }

        #endregion
        
        #region Global Map Input Reader Methods
        
        public void OnMiddleClick(InputAction.CallbackContext context) {
            if (context.phase is InputActionPhase.Started or InputActionPhase.Canceled) {
                bool pressed = context.phase == InputActionPhase.Started;
                MiddleClickUI.Invoke(pressed);
            }
        }

        #endregion

        public void InitializeInputActionAsset() {
            InputActions = new PlayerInputActions();
        }
        
        // Called in IInputReader on Enable
        public void EnablePlayerActions() {
            if(InputActions == null) {
                InitializeInputActionAsset();
            }
            
            InputActions.Player.SetCallbacks(this);
            InputActions.UI.SetCallbacks(this);
            InputActions.Global.SetCallbacks(this);
            InitializeActionMaps();

            switch (initialActionMap) {
                case ActionMapName.Player:
                    InputActions.Player.Enable();
                    _currentActionMap = ActionMapName.Player;
                    break;
                case ActionMapName.UI:
                    InputActions.UI.Enable();
                    _currentActionMap = ActionMapName.UI;
                    break;
                default: 
                    Debug.LogError("ActionMapName is not Added to the switch statement");
                    break;
            }
            
            InputActions.Global.Enable();
        }
        public bool IsActionMapActive(ActionMapName mapName) => _currentActionMap == mapName;
        void InitializeActionMaps() {
            AddActionMap(ActionMapName.Player, InputActions.Player);
            AddActionMap(ActionMapName.UI, InputActions.UI);
            AddActionMap(ActionMapName.Global, InputActions.Global);
            // TODO: Add more action maps here
        }
        void AddActionMap(ActionMapName mapName, InputActionMap actionMap) {
            if (!_actionMaps.TryAdd(mapName, actionMap)) {
                Debug.LogWarning($"Action map {mapName} already exists. Skipping addition.");
            }
        }

        public void SwitchActionMap(ActionMapName newMap) {
            if (InputActions == null) {
                Debug.LogError("InputActions not initialized");
                return;
            }

            if (_actionMaps.TryGetValue(_currentActionMap, out var currentActionMap)) {
                currentActionMap.Disable();
            }

            if (_actionMaps.TryGetValue(newMap, out var newActionMap)) {
                newActionMap.Enable();
                _currentActionMap = newMap;
            }else {
                Debug.LogError($"Action map {newMap} not found. Make sure to add it to the InitializeActionMaps method.");
            }
        }
        public enum ActionMapName {
            Player,
            UI, 
            // Global is treated different, we never disable it after enabling it in the beginning
            Global
        }
    }
}
