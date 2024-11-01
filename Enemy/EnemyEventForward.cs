using UnityEngine;

namespace Enemy {
    [RequireComponent(typeof(Animator))]
    public class EnemyEventForward : MonoBehaviour {
        [SerializeField] EnemyMover enemyMover;
        Animator _animator;

        void Awake() {
            _animator = GetComponent<Animator>();
            
            // TODO: Dont do this here!
            _animator.applyRootMotion = true;
        }

        void OnAnimatorMove() {
            enemyMover.AnimatorMove(_animator.rootPosition);
        }
    }
}
