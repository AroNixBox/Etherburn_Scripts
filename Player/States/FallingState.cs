using Extensions.FSM;
using UnityEngine;

namespace Player.States {
    public class FallingState : IState {
        readonly Animation.AnimationController _animatorController;
        readonly Mover _mover;
        
        Vector2 _moveInput;
        
        public FallingState(References references) {
            _animatorController = references.animationController;
            _mover = references.mover;
        }
        public void OnEnter() {
            _animatorController.EnableRootMotion(false);
            // Set Falling Animation
            PlayAnimation();
        }

        void PlayAnimation() {
            // TODO: Set Falling Animation Velocity in Animator to match the current velocity
            _animatorController.ChangeAnimationState(Animation.AnimationParameters.Fall, 
                Animation.AnimationParameters.GetAnimationDuration(Animation.AnimationParameters.Fall), 
                0);
        }

        public void Tick() { }

        public void FixedTick() {
            _mover.HandleFalling();
        }

        public void OnExit() {
            _animatorController.EnableRootMotion(true);
        }
    }
}