using Extensions.FSM;

namespace Player.States {
    /* @ Exit Condition
     * Every Land Animation needs to fire the LandEnd Event
     */
    public class LandingState : IState {
        readonly References _references;
        readonly Animation.AnimationController _animatorController;
        
        public LandingState(References references) {
            _references = references;
            _animatorController = references.animationController;
        }
        public void OnEnter() {
            PlayAnimation();
        }

        void PlayAnimation() {
            _animatorController.ChangeAnimationState(Animation.AnimationParameters.Land, 
                Animation.AnimationParameters.GetAnimationDuration(Animation.AnimationParameters.UnEquip), 
                0);
        }

        public void Tick() { }

        public void FixedTick() { }

        public void OnExit() {
            // Reset Exit Condition
            _references.LandEnded = false;
        }
    }
}