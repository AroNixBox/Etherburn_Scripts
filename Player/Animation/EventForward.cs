using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Animation {
    public class EventForward : MonoBehaviour{
        [SerializeField, Required] Mover mover;
        [SerializeField, Required] References references;
        
        Animator _animator;

        void Awake() {
            _animator = GetComponent<Animator>();
        }

        // Fast Forward of the OnAnimatorMove Event, because it can be only read by a component
        // attached to the same GameObject as the Animator
        /* @Explanation
         * Rotation is not taken into account, because the Animator is responsible for the rotation
         */
        void OnAnimatorMove() {
            if (mover != null) {
                var deltaPosition = _animator.deltaPosition;
                var deltaRotation = _animator.deltaRotation;
                mover.AnimatorMove(deltaPosition, deltaRotation);
            }
        }
        
        // Helper Method for Blend Trees
        bool IsCurrentPerformedAnimation(AnimationClip currentClip) {
            var currentAnimatorClipInfo = _animator.GetCurrentAnimatorClipInfo(0);
            float highestWeight = 0f;
            AnimationClip highestWeightClip = null;
            
            // Find the clip with the highest weight
            foreach (var clipInfo in currentAnimatorClipInfo) {
                if (clipInfo.weight > highestWeight) {
                    highestWeight = clipInfo.weight;
                    highestWeightClip = clipInfo.clip;
                }
            }
            
            return highestWeightClip != null 
                   && currentClip == highestWeightClip;
        }
        
        void OnFootstep(AnimationEvent evt) {
            if (IsCurrentPerformedAnimation(evt.animatorClipInfo.clip)) { 
                references.OnFootstepPerformed?.Invoke();
            }
        }
        
        void OnLand(AnimationEvent evt) => references.OnLandPerformed?.Invoke();
        void OnDodge(AnimationEvent evt) {
            if (IsCurrentPerformedAnimation(evt.animatorClipInfo.clip)) { 
                references.OnDodgeStarted?.Invoke();
            }
        }
        
        // Called when each of this Animation ends
        void DodgeEnd(AnimationEvent evt) {
            if (IsCurrentPerformedAnimation(evt.animatorClipInfo.clip)) { 
                references.DodgeEnded = true;
            }
        }
        void LandEnd(AnimationEvent evt) => references.LandEnded = true;
        void ExecutionEnd(AnimationEvent evt) => references.ExecutionEnded = true;
        void AttackEnd(AnimationEvent evt) => references.AttackEnded = true;
        void GetHitEnd(AnimationEvent evt) { 
            if (IsCurrentPerformedAnimation(evt.animatorClipInfo.clip)) { 
                references.GetHitEnded = true;
            }
        }
        void ChangeWeaponEnd(AnimationEvent evt) => references.ChangeWeaponEnded = true;
        
        // In Animation Trigger Events
        void SpawnParticle(AnimationEvent evt) => references.SpawnParticles.Invoke();
        void EnableHitDetection(AnimationEvent evt) => references.EnableHitDetection.Invoke();
        void DisableHitDetection(AnimationEvent evt) => references.DisableHitDetection.Invoke();
        void MaterializeWeapon(AnimationEvent evt) => references.OnMaterializeWeapon.Invoke();
        void DissolveWeapon(AnimationEvent evt) => references.OnDissolveWeapon.Invoke();
        
        // Specific to the Warp Animation
        void OnWarpStart(AnimationEvent evt) => references.InAnimationWarpFrames = true;
        void OnWarpEnd(AnimationEvent evt) => references.InAnimationWarpFrames = false;
    }
}