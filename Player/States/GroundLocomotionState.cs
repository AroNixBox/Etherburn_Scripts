using Extensions.FSM;
using Interfaces.Attribute;
using UnityEngine;

namespace Player.States {
    public class GroundLocomotionState : IState {
        #region References
        
        readonly References _references;
        readonly Animation.AnimationController _animationController;
        readonly Mover _mover;
        readonly IEnergy _stamina;

        #endregion

        #region Values

        // Initial values
        readonly float _runSpeed;
        readonly float _walkSpeed;
        readonly float _speedLerpRate;
        readonly float _directionLerpRate;
        readonly float _runStaminaCostPerSecond;

        readonly float _minPlayerSpeedWhereCameraRotates;
        
        const float Epsilon = 0.01f; // Small value to check if we are close to 0
        
        // Dynamic values
        float _currentSpeed;
        Vector2 _currentVelocity;

        #endregion
        public GroundLocomotionState(References references) {
            // References
            _references = references;
            _animationController = references.animationController;
            _mover = references.mover;
            _stamina = references.StaminaAttribute;
            
            // Initial values
            _runSpeed = references.runSpeedInAnimator;
            _walkSpeed = references.walkSpeedInAnimator;
            _speedLerpRate = references.speedLerpRate;
            _directionLerpRate = references.directionLerpRate;
            _minPlayerSpeedWhereCameraRotates = references.minPlayerSpeedWhereCameraRotatesModel;
            _runStaminaCostPerSecond = references.runStaminaCostPerSecond;
        }
        
        public void OnEnter() {
            ReadAnimatorValues();
            PlayAnimation();
            
            // Physics
            _mover.SetGravity(true);
        }
        void ReadAnimatorValues() {
            // Read from animator:
            _currentSpeed = _animationController.GetAnimatorFloat(Animation.AnimationParameters.Speed);
            _currentVelocity = new Vector2(_animationController.GetAnimatorFloat(Animation.AnimationParameters.VelocityX), 
                _animationController.GetAnimatorFloat(Animation.AnimationParameters.VelocityZ));
        }
        void PlayAnimation() {
            _animationController.ChangeAnimationState(Animation.AnimationParameters.GroundLocomotion, 
                Animation.AnimationParameters.GetAnimationDuration(Animation.AnimationParameters.GroundLocomotion), 
                0);
        }

        public void Tick() {
            bool isRunning = _references.RunKeyPressed && _stamina.HasEnough(_runStaminaCostPerSecond * Time.deltaTime);
            bool hasMoveInput = Mathf.Abs(_references.MovementInput.x) >= Epsilon || Mathf.Abs(_references.MovementInput.y) >= Epsilon;

            UpdateMovementValues(hasMoveInput, isRunning);
            AdjustValuesNearZero();
            UpdateAnimator();
            
            // Allow the player to rotate the model in the direction of the camera if over a certain speed
            _mover.CanApplyModelRotationInCameraForward = _currentSpeed > _minPlayerSpeedWhereCameraRotates;
        }
        void UpdateMovementValues(bool hasMoveInput, bool isRunning) {
            // Calculate the target speed based on the input, if there is no input, return 0
            var targetSpeed = hasMoveInput ? isRunning ? _runSpeed : _walkSpeed : 0f;
            var targetVelocity = hasMoveInput ? _references.MovementInput.normalized : Vector2.zero;

            // Lerp the speed and velocity to get a smooth animation blends
            _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * _speedLerpRate);
            _currentVelocity = Vector2.Lerp(_currentVelocity, targetVelocity, Time.deltaTime * _directionLerpRate);

            // Consume stamina if running
            if (isRunning) { _stamina.Decrease(_runStaminaCostPerSecond * Time.deltaTime); }
        }
        void AdjustValuesNearZero() {
            // After Lerp, check if we are close to 0, if so, set to 0
            if (Mathf.Abs(_currentSpeed) < Epsilon) { _currentSpeed = 0f; }
            // Even smaller threshold for velocity, because we have small blending values ({Epsilon * Epsilon})
            if (_currentVelocity.sqrMagnitude < Epsilon * Epsilon) { _currentVelocity = Vector2.zero; }
        }
        void UpdateAnimator() {
            // Feed it into the Animator
            _animationController.UpdateAnimatorSpeed(_currentSpeed);
            _animationController.UpdateAnimatorVelocity(_currentVelocity);
        }
        public void FixedTick() { }
        public void OnExit() {
            _mover.SetGravity(false);
            
            // Every State needs to independently set the CanApplyRotation!
            _mover.CanApplyModelRotationInCameraForward = false;
        }
    }
}