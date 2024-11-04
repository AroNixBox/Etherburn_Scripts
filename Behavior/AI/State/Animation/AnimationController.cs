using System.Collections.Generic;
using UnityEngine;

namespace Behavior.Enemy.State.Animation {
    public class AnimationController : MonoBehaviour {
        [SerializeField] Animator animator;
        [SerializeField] AnimationClip firstInitialAttackClip;
        [SerializeField] AnimationClip secondInitialAttackClip;
        
        int _currentStateHash;
        public void CrossfadeToState(AnimationsParams.AnimationDetails stateDetails) {
            if (_currentStateHash == stateDetails.StateName) return;
            _currentStateHash = stateDetails.StateName;
            animator.CrossFade(_currentStateHash, stateDetails.BlendDuration);
        }
        public AnimationClip GetInitialAttackClip(AnimationStates animationState) {
            switch (animationState) {
                case AnimationStates.AttackA:
                    return firstInitialAttackClip;
                case AnimationStates.AttackB:
                    return secondInitialAttackClip;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(animationState), animationState, null);
                    return null;
            }
        }
        public void ReplaceClipFromOverrideController(AnimationClip oldClip, AnimationClip newClip) {
            var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            
            if(overrideController == null) {
                Debug.LogError("Animator does not have an override controller.");
                return;
            }
            
            // Get the current overrides
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);

            // Find and replace the specific override
            for (int i = 0; i < overrides.Count; i++) {
                if (overrides[i].Key == oldClip) {
                    overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, newClip);
                    break;
                }
            }

            // Apply the modified overrides
            overrideController.ApplyOverrides(overrides);
        }
    }
}
