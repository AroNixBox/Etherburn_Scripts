using Extensions;
using Extensions.FSM;
using Interfaces.Attribute;
using Player.Animation;
using Player.Audio;
using UnityEngine;

namespace Player.States {
    /* @ Exit Condition
     * Every GetHit Animation needs to fire the GetHitEnd Event
     */
    public class GetHitState : IState {
        // References
        readonly References _references;
        readonly AnimationController _animationController;
        readonly Mover _mover;
        readonly Transform _modelRoot;
        readonly PlayerSounds _playerSounds;
        
        readonly IHealth _health;
        
        readonly Vector2[] _blendTreeDirections = {
            new (0, 1),      // GetHit Front
            new (-0.7f, 0.7f), // GetHit Front Left
            new (0.7f, 0.7f),  // GetHit Front Right
            new (-1, 0),    // GetHit Left
            new (1, 0),     // GetHit Right
            new (0, -1)     // GetHit Back
        };
        public GetHitState(References references) {
            // References
            _references = references;
            _animationController = references.animationController;
            _mover = references.mover;
            _health = references.HealthAttribute;
            _modelRoot = references.modelRoot;
            _playerSounds = references.playerSounds;
        }
        public void OnEnter() {
            // Physics
            _mover.SetGravity(true);
            
            PlayAnimation();
            
            int randomSoundIndex = Random.Range(0, _playerSounds.hurtSounds.Length);
            _references.weapon2DSource.PlayOneShot(_playerSounds.hurtSounds[randomSoundIndex]);
        }

        void PlayAnimation() {
            var hitDirection = Vector2Extensions.GetLocalDirectionToPoint(_modelRoot, _health.HitPosition);
            var bestMatchBlendTreeAnimation = Vector2Extensions.GetClosestDirectionVectorToDirection(hitDirection, _blendTreeDirections);
            _animationController.UpdateAnimatorHitDirection(bestMatchBlendTreeAnimation);
            _animationController.ChangeAnimationState(AnimationParameters.GetHit, 
                AnimationParameters.GetAnimationDuration(AnimationParameters.GetHit), 
                0);
        }
        public void Tick() { }

        public void FixedTick() { }

        // Reset the Flag that tells the dodge has ended
        public void OnExit() {
            // Reset State Leave Flag
            _references.GetHitEnded = false;
            
            // Reset the Entry Flag
            _health.HasTakenDamage = false;
            
            // Physics
            _mover.SetGravity(false);
        }
    }
}