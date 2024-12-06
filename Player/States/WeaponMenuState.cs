using Extensions;
using Extensions.FSM;
using Game;
using Player.Input;
using UI;
using UnityEngine;

namespace Player.States {
    public class WeaponMenuState : IState {
        readonly InputReader _input;
        readonly RadialSelection _radialSelection;
        readonly Animation.AnimationController _animationController;
        
        Vector2 _inputPosition;
        bool _isInputMouse;

        // For Stop Walking/ Running smoothly
        readonly float _lerpSpeed;
        float _currentSpeed;
        Vector2 _currentVelocity;
        
        public WeaponMenuState(References references) {
            _input = references.input;
            _radialSelection = references.radialSelection;
            _animationController = references.animationController;
            _lerpSpeed = references.stopLerpSpeedWhenNoInputEnabled;
        }

        public void OnEnter() {
            _input.PointUI += TrackPointerInputPosition;
            if (CursorManager.Instance == null) {
                Debug.LogError("Cursor Manager not in the scene");
            }
            else {
                if (!CursorManager.Instance.GetCursorVisible()) {
                    CursorManager.Instance.SetCursorVisible(true);
                    CursorManager.Instance.SetCursorLockMode(CursorLockMode.None);
                }
            }
            _input.SwitchActionMap(InputReader.ActionMapName.UI);
            _radialSelection.EnableRadialSelection(true);
            
            ReducePlayerSpeedToZero();
        }
        void ReducePlayerSpeedToZero() {
            _currentSpeed = _animationController.GetAnimatorFloat(Animation.AnimationParameters.Speed);
            _currentVelocity = new Vector2(_animationController.GetAnimatorFloat(Animation.AnimationParameters.VelocityX), 
                _animationController.GetAnimatorFloat(Animation.AnimationParameters.VelocityZ));
        }

        // Cache the Player Input
        void TrackPointerInputPosition(Vector2 inputPosition, bool isInputMouse) {
            _inputPosition = inputPosition;
            _isInputMouse = isInputMouse;
        }

        public void Tick() {
            _radialSelection.SelectRadialPart(_inputPosition, _isInputMouse);
            
            // Lerp Speed to zero to slow down and stand when opening the weapon menu
            _currentSpeed = MathHelper.LerpToZero(_currentSpeed, _lerpSpeed);
            _currentVelocity = MathHelper.LerpToZero(_currentVelocity, _lerpSpeed);

            if (_currentSpeed > 0) {
                _animationController.UpdateAnimatorSpeed(_currentSpeed);
            }
            if (_currentVelocity != Vector2.zero) {
                _animationController.UpdateAnimatorVelocity(_currentVelocity);
            }
        }

        public void FixedTick() { }

        public void OnExit() {
            // We dont need to track the input anymore
            _input.PointUI -= TrackPointerInputPosition;
            
            if (CursorManager.Instance == null) {
                Debug.LogError("Cursor Manager not in the scene");
            }
            else {
                if (!CursorManager.Instance.GetCursorVisible()) {
                    CursorManager.Instance.SetCursorVisible(false);
                    CursorManager.Instance.SetCursorLockMode(CursorLockMode.Locked);
                }
            }
            
            _input.SwitchActionMap(InputReader.ActionMapName.Player);
            _radialSelection.EnableRadialSelection(false);
        }
    }
}