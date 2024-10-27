using System;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Player.Animation.MotionWarp {
    public class RootMotionWarpingController  {
        /// <summary>The Current Warp Animation that was passed in on P</summary>
        WarpAnimation _warpAnimation;
        readonly References _references;
        /// <summary>Indicator if WarpAnimation is Currently Playing, Condition for OnAnimatorMove!</summary>
        public bool IsWarpTargetAssigned => _warpTarget != null;
        Transform _warpTarget; 
        /// <summary>60 Is currently max without glitching.</summary>
        const float MaxAngleToTarget = 60f;
        
        /// <summary> Indicator that stops all Motion including root Motion when true</summary>
        bool _hasReachedTarget;

        /// <summary>GameObject with the Animator Component on (Your Character Mesh Probably</summary>
        readonly Transform _animatedGameObject;
        /// <summary>The Target you Apply Rotation for your Character on.</summary>
        readonly Transform _rotationTarget;

        /// <param name="animatedGameObject">The GameObject where the Animator for the RootMotion Animation</param>
        /// <param name="rotationTarget">The GameObject that controls character rotation</param>
        /// <param name="references">Where the IsInWarpFrames Bool is Changed from the AnimationEvents</param>
        public RootMotionWarpingController(Transform animatedGameObject, Transform rotationTarget, References references) {
            _animatedGameObject = animatedGameObject;
            _references = references;
            _rotationTarget = rotationTarget;
        }
        /// <summary>Sets the Warp Conditions for OnAnimatorMove</summary>
        Vector3 _startPostion;

        public void SetWarpAnimationAndTarget(Transform newWarpTarget, WarpAnimation newWarpAnimation, Vector3 startPostion) {
            _warpAnimation = newWarpAnimation;
            _warpTarget = newWarpTarget;
            _startPostion = startPostion;
        }
        /// <summary> Call when we are done with the Animation.</summary>
        public void NullAllConditions() {
            _warpTarget = null;
            _hasReachedTarget = false;
            _warpAnimation = null;
        }

        #region Calculus
        ///<summary>Call OnAnimatorMove when WarpTarget != null and apply Position and Rotation that is Returned out of this tuple</summary>
        /// <returns>The Position and Rotation Adjust the Character needs get towards the warp target</returns>
        public (Vector3, Quaternion) GetTotalWarpedAndRootMotion(Vector3 deltaPosition, AnimatorStateInfo stateInfo) {
            // Do not proceed if we don't want to warp or have reached the target
            if (_hasReachedTarget /* Or the Animation has finished playing */) {
                // Dont Move, invisible wall.
                return (_animatedGameObject.position, _rotationTarget.rotation);
            }
        
            Vector3 directionToTarget = (_warpTarget.position - _animatedGameObject.position).normalized;
            Vector3 totalPositionAdjust = _animatedGameObject.position;
            Quaternion totalRotationAdjust = _rotationTarget.rotation;
        
            // Inside of warp frame range
            if (_references.InAnimationWarpFrames) {
                // Only warp if the moved direction is towards the target
                // Dont warp if we make a step back for example but still in the warp frame range
                if (Vector3.Dot(deltaPosition.normalized, directionToTarget) > 0) {
                    var warpedPosAndRot= GetWarpedPositionAndRotationTowardsTarget(_warpTarget.position, stateInfo, deltaPosition);
                    totalPositionAdjust = warpedPosAndRot.Item1;
                    totalRotationAdjust = warpedPosAndRot.Item2;
                } 
            }
            else {
                totalPositionAdjust += deltaPosition;
            }
            
            return (totalPositionAdjust, totalRotationAdjust);
        }
    
        (Vector3, Quaternion) GetWarpedPositionAndRotationTowardsTarget(Vector3 targetPosition, AnimatorStateInfo stateInfo, Vector3 deltaPosition) {
            Vector3 currentPosition = _animatedGameObject.position;
            Vector3 directionToTarget = (targetPosition - currentPosition).normalized;
            Vector3 directionToTargetFromStart = (targetPosition - _startPostion).normalized;
            
            Vector3 remainingDistance = targetPosition - currentPosition;

            // Calculate the remaining animation motion
            float normalizedTime = stateInfo.normalizedTime;
            Vector3 remainingAnimationMotion = _warpAnimation.totalRootMotionBetweenWarpStartAndEndFrame * (1f - normalizedTime);
            
            // Adjust the scaleFactor based on remaining distance and remaining animation motion
            float scaleFactor = 1f;
            if (remainingAnimationMotion.magnitude > 0) {
                // Standard scale factor based on remaining distance and animation motion
                scaleFactor = remainingDistance.magnitude / remainingAnimationMotion.magnitude;
                // Calculate a dynamic multiplier based on the total root motion and animation speed
                float animationSpeedFactor = _warpAnimation.totalRootMotion.z / _warpAnimation.totalRootMotionBetweenWarpStartAndEndFrame.z;
                // Dynamic multiplier based on remaining distance and animation speed factor
                float dynamicMultiplier = Mathf.Max(remainingDistance.magnitude / animationSpeedFactor, 1f);
                // Apply the dynamic multiplier to the scale factor
                scaleFactor *= dynamicMultiplier;
            }

            // Motion for this one frame
            Vector3 frameMotion = deltaPosition * scaleFactor;

            // If we are closer to the target with the frame motion itself
            if (frameMotion.magnitude > remainingDistance.magnitude) {
                float overshootFactor = frameMotion.magnitude / remainingDistance.magnitude;
                frameMotion = remainingDistance / overshootFactor;
            }

            // Transform the frame motion to local space
            Vector3 localFrameMotion = _animatedGameObject.InverseTransformDirection(frameMotion);
            // Ensure the local position is within bounds
            localFrameMotion.x = Mathf.Clamp(localFrameMotion.x, -1f, 1f);
            localFrameMotion.z = Mathf.Clamp(localFrameMotion.z, 0f, remainingDistance.magnitude);
            // Transform back to world space
            frameMotion = _animatedGameObject.TransformDirection(localFrameMotion);

            // Adjust the position based on the direction to the target
            Vector3 newPosition = _animatedGameObject.position + directionToTarget * frameMotion.magnitude + deltaPosition;
            
            // Check if we already overshot the target
            if((_startPostion - targetPosition).sqrMagnitude < (_startPostion - newPosition).sqrMagnitude) {
                newPosition = targetPosition;
                _hasReachedTarget = true;
            }
            
            // Set the Y position to match the target's Y position
            newPosition.y = targetPosition.y;
            Vector3 targetDirectionWithoutY = new Vector3(directionToTargetFromStart.x, 0, directionToTargetFromStart.z);
            Quaternion newRotation = Quaternion.LookRotation(targetDirectionWithoutY);

            return (newPosition, newRotation);
        }
        
        #endregion
        
        #region Warp Checks
        
        public bool IsWarpPossible(Transform warpTargetToCheck, WarpAnimation warpAnimation, float maxWarpRMMultiplier) {
            return IsFarEnoughAwayForMinimumWarp(warpTargetToCheck, warpAnimation) 
                   && IsWarpSufficient(warpTargetToCheck, warpAnimation, maxWarpRMMultiplier);
        }

        ///<summary> Checks if a warp to the target position is possible based on the Root Motion Animation until the warp start frame</summary>
        bool IsFarEnoughAwayForMinimumWarp(Transform warpTargetToCheck, WarpAnimation warpAnimation) {
            // Convert target position to local space
            Vector3 localTargetPosition = _animatedGameObject.InverseTransformPoint(warpTargetToCheck.position);
            Vector3 localCurrentPosition = Vector3.zero;
    
            // Calculate the remaining distance to the target in local space
            Vector3 remainingDistance = localTargetPosition - localCurrentPosition;

            // Get the forward distance to the target
            float forwardDistance = remainingDistance.z;
    
            // Calculate the total root motion from the start of the animation to the warp start frame
            Vector3 rootMotionUntilStart = warpAnimation.totalRootMotionUntilWarpStartFrame;

            // Check if the root motion magnitude is less than or equal to the forward distance
            // If true, the warp is possible; if false, the target is too far for the warp
            return rootMotionUntilStart.z <= forwardDistance;
        }   
        /// <summary>Less than max Warp Distance? Based on RootMotionMovement + Movement in Warp Frames * Multiplier</summary>
        bool IsWarpSufficient(Transform warpTargetToCheck, WarpAnimation warpAnimation, float maxWarpRMMultiplier) {
            Vector3 targetPosition = warpTargetToCheck.position;

            // Direction from the character to the target
            Vector3 directionToTarget = (targetPosition - _animatedGameObject.position).normalized;

            // Angle between both
            float angle = Vector3.Angle(_animatedGameObject.forward, directionToTarget);

            // Clamp angle is max 60 degrees, causes issues otherwise
            if (angle > MaxAngleToTarget) {
                return false;
            }

            // Project the target location onto the local z-axis of the character
            Vector3 localTargetPosition = _animatedGameObject.InverseTransformPoint(targetPosition);

            // RM Distance between start and end frame
            Vector3 rootMotionInWarpFrames = warpAnimation.totalRootMotionBetweenWarpStartAndEndFrame;
            Vector3 totalRootMotion = warpAnimation.totalRootMotion;

            // Only z
            float rootMotionDistanceInWarpFrames = rootMotionInWarpFrames.z;
            float totalRootMotionDistance = totalRootMotion.z;
            float rootMotionDistanceWithoutWarpFrames = totalRootMotionDistance - rootMotionDistanceInWarpFrames;

            // Local distance to check for difference in local space
            float distanceToTargetZ = Mathf.Abs(localTargetPosition.z);

            return rootMotionDistanceWithoutWarpFrames + rootMotionDistanceInWarpFrames * maxWarpRMMultiplier >= distanceToTargetZ;
        }
        
        #endregion
    }
}