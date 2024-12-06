using Extensions;
using Extensions.FSM;
using Interfaces.Attribute;
using Player.Animation;
using UnityEngine;

namespace Player.States {
    /* @ Exit Condition
     * Every Dodge Animation needs to fire the DodgeEnd Event
     */
    public class DodgingState : IState {
        // References
        readonly References _references;
        readonly AnimationController _animationController;
        readonly Mover _mover;
        readonly IEnergy _stamina;
        readonly CapsuleCollider _collider;

        readonly Vector2[] _dodgeDirections =  {
            new (0, 1),       // Dodge Front
            new (-0.7f, 0.7f), // Dodge Front Left
            new (0.7f, 0.7f),  // Dodge Front Right
            new (-1, 0),     // Dodge Left
            new (1, 0),      // Dodge Right
            new (0, -1),     // Dodge Back
            new (-0.7f, -0.7f), // Dodge Back Left
            new (0.7f, -0.7f)  // Dodge Back Right
        };
        
        // Values
        readonly float _dodgeStaminaCost;
        
        public DodgingState(References references) {
            // References
            _references = references;
            _animationController = references.animationController;
            _mover = references.mover;
            _stamina = references.StaminaAttribute;
            _collider = references.collider;
            
            // Timers
            
            // Values
            _dodgeStaminaCost = references.dodgeStaminaCost;
        }
        public void OnEnter() {
            // Physics
            _mover.SetGravity(true);
            
            PlayAnimation();

            _stamina.Decrease(_dodgeStaminaCost);
            ReduceColliderSize();
        }
        
        void ReduceColliderSize() {
            _collider.height /= 2;
            _collider.center = new Vector3(_collider.center.x, _collider.height / 2, _collider.center.z);
        }
        
        void ResetColliderSize() {
            _collider.height *= 2;
            _collider.center = new Vector3(_collider.center.x, _collider.height / 2, _collider.center.z);
        }

        void PlayAnimation() {
            var dodgeDirection = GetDodgeDirection();
            var bestMatchBlendTreeAnimation = Vector2Extensions.GetClosestDirectionVectorToDirection(dodgeDirection, _dodgeDirections);
            _animationController.UpdateAnimatorVelocity(bestMatchBlendTreeAnimation);
            _animationController.ChangeAnimationState(AnimationParameters.Dodge, 
                AnimationParameters.GetAnimationDuration(AnimationParameters.Dodge), 
                0);
        }
        Vector2 GetDodgeDirection() {
            Vector2 input = _references.MovementInput.normalized;

            // By default, dodge backwards if no input is given
            if (input == Vector2.zero) {
                return _dodgeDirections[5]; // Dodge Back
            }

            return input;
        }


        public void Tick() { }

        public void FixedTick() { }

        // Reset the Flag that tells the dodge has ended
        public void OnExit() {
            // Reset State Leave Flag
            _references.DodgeEnded = false;
            
            // Physics
            _mover.SetGravity(false);
            
            ResetColliderSize();
        }
    }
}