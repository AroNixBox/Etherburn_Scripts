using System;
using System.Collections.Generic;
using UnityEngine;

namespace Behavior.Enemy.State.Animation {
    public class AnimationController : MonoBehaviour {
        [SerializeField] Animator animator;
        [SerializeField] AnimationClip[] emptyAttackClips;
        [SerializeField] AnimationClip[] emptyHurtClips;
        
        readonly Dictionary<NPCAnimationStates, AnimationClip> _animationClipMap = new ();
        int _currentStateHash;

        void Awake() {
            if (emptyAttackClips.Length > 0) {
                _animationClipMap[NPCAnimationStates.AttackA] = emptyAttackClips[0];
                _animationClipMap[NPCAnimationStates.AttackB] = emptyAttackClips[1];
            } else {
                Debug.LogError("emptyAttackClips array is empty.");
            }

            if (emptyHurtClips.Length > 0) {
                _animationClipMap[NPCAnimationStates.HurtA] = emptyHurtClips[0];
                _animationClipMap[NPCAnimationStates.HurtB] = emptyHurtClips[1];
            } else {
                Debug.LogError("emptyHurtClips array is empty.");
            }
        }
        
        public void CrossfadeToState(AnimationsParams.AnimationDetails stateDetails) {
            if (_currentStateHash == stateDetails.StateName) return;
            _currentStateHash = stateDetails.StateName;
            animator.CrossFade(_currentStateHash, stateDetails.BlendDuration);
        }
        public AnimationClip GetInitialAttackClip(NPCAnimationStates npcAnimationState) {
            if (_animationClipMap.TryGetValue(npcAnimationState, out var clip)) {
                return clip;
            }
            throw new ArgumentOutOfRangeException(nameof(npcAnimationState), npcAnimationState, "The requested NPCAnimationState is not supported.");
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
