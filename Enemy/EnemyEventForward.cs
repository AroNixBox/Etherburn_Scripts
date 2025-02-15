using Sensor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Enemy {
    [RequireComponent(typeof(Animator))]
    public class EnemyEventForward : MonoBehaviour {
        [SerializeField, Required] EnemyMover enemyMover;
        [SerializeField, Required] DamageDealingObject weapon;
        Animator _animator;

        void Awake() {
            _animator = GetComponent<Animator>();
            
            // TODO: Dont do this here!
            _animator.applyRootMotion = true;
        }
        
        // EnemyMover will be null when Enemy dies, all Components get destroyed except [EnemyEventForward.cs] and [Animator]
        void OnAnimatorMove() {
            if(enemyMover == null) { return; }
            
            enemyMover.AnimatorMove(_animator.rootPosition);
        }
        
        // Animation Events
        void EnableHitDetection(AnimationEvent evt) {
            if (IsInAnimationTransition(evt)) { return; }
            if(weapon == null) { return; } // [WeaponController.cs] will be null when Enemy dies, all Components get destroyed except [EnemyEventForward.cs] and [Animator]
            weapon.CastForObjects(true);
        }
        
        void DisableHitDetection(AnimationEvent evt) {
            if (IsInAnimationTransition(evt)) { return; }
            if(weapon == null) { return; } // [WeaponController.cs] will be null when Enemy dies, all Components get destroyed except [EnemyEventForward.cs] and [Animator]
            weapon.CastForObjects(false);
        }
        
        bool IsInAnimationTransition(AnimationEvent evt) {
            return evt.animatorClipInfo.weight <= 0.95f;
        }
    }
}
