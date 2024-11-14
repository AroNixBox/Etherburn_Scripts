using Sirenix.OdinInspector;
using UnityEngine;

namespace Enemy {
    [RequireComponent(typeof(Animator))]
    public class EnemyEventForward : MonoBehaviour {
        [SerializeField, Required] EnemyMover enemyMover;
        [SerializeField, Required] EnemyWeaponController weaponController;
        Animator _animator;

        void Awake() {
            _animator = GetComponent<Animator>();
            
            // TODO: Dont do this here!
            _animator.applyRootMotion = true;
        }

        void OnAnimatorMove() {
            enemyMover.AnimatorMove(_animator.rootPosition);
        }
        
        // Animation Events
        void EnableHitDetection(AnimationEvent evt) {
            weaponController.SetWeaponColliderState(true);
        }
        
        void DisableHitDetection(AnimationEvent evt) {
            weaponController.SetWeaponColliderState(false);
        }
    }
}
