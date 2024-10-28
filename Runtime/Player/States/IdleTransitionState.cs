using Extensions.FSM;
using UnityEngine;

namespace Player.States {
    /* @ Explanation
     * This state is used as an empty transition state, so if something needs to give the player time
     * Usecase 1: Transition from Unequip to Equip we need to assign a new AnimatorOverrideController
     * If we do this in the unequip animation, two animation events for exiting the Unequip State may be called
     */
    public class IdleTransitionState : IState {
        readonly Animation.AnimationController _animationController;
        readonly Timer _timer;
        readonly int _stateToTransitionTo;
        readonly int _transitionLayer;
        readonly float _transitionTime;
        
        // Exit Condition
        bool _isTransitionTimeOver;
        public IdleTransitionState(References references, float idleTime, int stateToTransitionTo, float transitionTime, int transitionLayer) {
            _animationController = references.animationController;
            _stateToTransitionTo = stateToTransitionTo;
            _transitionTime = transitionTime;
            _transitionLayer = transitionLayer;
            
            _timer = new CountdownTimer(idleTime);
            _timer.OnTimerStop += OnTimerStop;
        }

        void OnTimerStop() => _isTransitionTimeOver = true;

        public void OnEnter() {
            _timer.Start();
            
            _animationController.ChangeAnimationState(_stateToTransitionTo, _transitionTime, _transitionLayer);
        }
        
        public bool IsTransitionTimeOver() => _isTransitionTimeOver;

        public void Tick() {
            _timer.Tick(Time.deltaTime);
        }

        public void FixedTick() { }

        public void OnExit() {
            _isTransitionTimeOver = false;
        }
    }
}