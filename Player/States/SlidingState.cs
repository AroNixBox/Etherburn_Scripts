using Extensions.FSM;

namespace Player.States {
    public class SlidingState : IState {
        readonly Mover _mover;
        readonly Animation.AnimationController _animatorController;
        public SlidingState(References references) {
            _mover = references.mover;
            _animatorController = references.animationController;
        }
        public void OnEnter() {
            _animatorController.EnableRootMotion(false);
            // Set Falling Animation
            PlayAnimation();
        }

        void PlayAnimation() {
            _animatorController.ChangeAnimationState(Animation.AnimationParameters.Fall, 
                Animation.AnimationParameters.GetAnimationDuration(Animation.AnimationParameters.Fall), 
                0);
        }

        public void Tick() { }

        public void FixedTick() {
            // TODO: Maybe face the direction we fall or smt?
            _mover.HandleSliding();
        }

        public void OnExit() {
            _animatorController.EnableRootMotion(true);
        }
    }
}