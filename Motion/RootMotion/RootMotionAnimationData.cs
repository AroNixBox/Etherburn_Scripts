using Sirenix.OdinInspector;
using UnityEngine;

namespace Motion.RootMotion {
    [CreateAssetMenu(fileName = "Root Motion Animation Data", menuName = "Animation Helper/Root Motion Animation Data")]
    public class RootMotionAnimationDataSO : ScriptableObject {
        [InfoBox("Specify the frame range to apply the root motion warp")]
        [Required] public AnimationClip clip;
        
        // TODO: Currently this is only used for enemies. Even tho we also use this Asset for the Player
        // Missing time ;)
        public bool distanceIndependent;
        [ShowIf("@distanceIndependent")]
        [Tooltip("Selecting an AnimationClip if DistanceIndependent is not based on the distance to the target, but rather on the probability of selection")]
        [Range(1, 100)] public uint selectionProbability;
        [ShowIf("@distanceIndependent")]
        [Tooltip("The maximum amount how often this animation can be executed")]
        public uint executionAmount;
        
        [ShowIf("@!distanceIndependent")]
        [Tooltip("If the Attack has an impact radius, the attack end will be an area instead of a point")]
        public bool hasAttackRadius;
        [ShowIf("@hasAttackRadius && !distanceIndependent")]
        [Tooltip("The radius of the attack")]
        public float attackRadius;
        
        [BoxGroup("Precompute Root Motion")]
        public GameObject targetObject;
        
        [InfoBox("Dont write the values by hand, use the button to precompute the values", InfoMessageType.Warning)]
        [BoxGroup("Precompute Root Motion")]
        public Vector3 totalRootMotion;
        
        [Button("Perform Precompute", ButtonSizes.Large, ButtonStyle.CompactBox)]
        [GUIColor(0.4f, 0.8f, 1.0f)]
        [BoxGroup("Precompute Root Motion")]
        public virtual void PrecomputeRootMotion() {
            if(clip == null) {
                Debug.LogError("No clip assigned to warp animation");
                return;
            }
            if(targetObject == null) {
                Debug.LogError("No animator assigned to warped object");
                return;
            }

            int totalFrames = Mathf.FloorToInt(clip.length * clip.frameRate);
            // Compute total motion from 0 to the last frame, because fuck rotation
            totalRootMotion = targetObject.transform.
                InverseTransformDirection(CalculateMotionBetweenFrames(0, totalFrames - 1, clip.frameRate));
        }
        
        // Helper method to compute motion between two frames
        protected Vector3 CalculateMotionBetweenFrames(int startFrame, int endFrame, float frameRate) {
            Vector3 totalMotion = Vector3.zero;
            for (int i = startFrame; i < endFrame; i++) {
                float startTime = i / frameRate;
                float endTime = (i + 1) / frameRate;
                Vector3 deltaPosition = GetRootMotionDeltaAtTime(startTime, endTime);
                totalMotion += deltaPosition;
            }
            return totalMotion;
        }
        
        Vector3 GetRootMotionDeltaAtTime(float startTime, float endTime) {
            var animated = targetObject.gameObject;
            var initialPosition = animated.transform.position;
            var initialRotation = animated.transform.rotation;
        
            // Logic for sampling animation
            clip.SampleAnimation(animated, startTime);
            Vector3 startPos = animated.transform.position;

            clip.SampleAnimation(animated, endTime);
            Vector3 endPos = animated.transform.position;
        
            // Reset the position and rotation
            animated.transform.position = initialPosition;
            animated.transform.rotation = initialRotation;

            return endPos - startPos;
        }
    }
}