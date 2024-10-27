using System;
using Extensions.FSM;
using Interfaces.Attribute;
using Player.Animation;
using Player.Audio;
using UnityEngine;

namespace Player.States {
    /* @ Exit Condition
     * No Exit Condition, is an End State
     */
    public class DieState : IState {
        // References
        readonly References _references;
        readonly AnimationController _animationController;
        readonly Mover _mover;
        readonly Transform _modelRoot;
        readonly PlayerSounds _playerSounds;
        Action _discardStateMachine;
        
        readonly IHealth _health;
        
        readonly Vector2[] _fallDirections = {
            new (0, 1),      // GetHit Front
            new (0, -1)     // GetHit Back
        };
        
        public DieState(References references, Action discardStateMachine) {
            // References
            _references = references;
            _animationController = references.animationController;
            _mover = references.mover;
            _health = references.HealthAttribute;
            _modelRoot = references.modelRoot;
            _playerSounds = references.playerSounds;
            
            _discardStateMachine = discardStateMachine;
        }
        public void OnEnter() {
            // Physics
            _mover.SetGravity(true);
            
            PlayAnimation();
            
            int randomSoundIndex = UnityEngine.Random.Range(0, _playerSounds.hurtSounds.Length);
            _references.weapon2DSource.PlayOneShot(_playerSounds.hurtSounds[randomSoundIndex]);
            
            _discardStateMachine.Invoke();
        }

        void PlayAnimation() {
            var hitDirection = Extensions.Vector2Extensions.GetLocalDirectionToPoint(_modelRoot, _health.HitPosition);
            var bestMatchBlendTreeAnimation = Extensions.Vector2Extensions.GetClosestDirectionVectorToDirection(hitDirection, _fallDirections);
            _animationController.UpdateAnimatorHitDirection(bestMatchBlendTreeAnimation);
            _animationController.ChangeAnimationState(AnimationParameters.Die, 
                AnimationParameters.GetAnimationDuration(AnimationParameters.Die), 
                0);
        }

        public void Tick() { }

        public void FixedTick() { }

        // Reset the Flag that tells the dodge has ended
        public void OnExit() {
            // Reset the Entry Flag
            _health.HasDied = false;
            
            // Physics
            _mover.SetGravity(false);
        }
    }
}