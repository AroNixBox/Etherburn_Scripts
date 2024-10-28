using UnityEngine;

namespace Behavior.Enemy.State.Animation {
    public class AnimationController : MonoBehaviour {
        [SerializeField] Animator animator;
        int _currentStateHash;
        public void CrossfadeToState(AnimationsParams.AnimationDetails stateDetails) {
            if (_currentStateHash == stateDetails.StateName) return;
            _currentStateHash = stateDetails.StateName;
            animator.CrossFade(_currentStateHash, stateDetails.BlendDuration);
        }
    }
}
