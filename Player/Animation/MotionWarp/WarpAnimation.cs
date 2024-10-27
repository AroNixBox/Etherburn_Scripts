using Effects.VFX;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player.Animation.MotionWarp {
    [CreateAssetMenu(fileName = "Warp Animation", menuName = "Animation Helper/Warp Animation")]
    public class WarpAnimation : ScriptableObject {
        [InfoBox("Specify the frame range to apply the root motion warp")]
        [Required] 
        public AnimationClip clip;

        [HorizontalGroup("Frames", Width = 0.5f)]
        [LabelWidth(100)]
        [MinValue(0)]
        [MaxValue("@clip.frameRate * clip.length")]
        public int startFrame;

        [HorizontalGroup("Frames", Width = 0.5f)]
        [LabelWidth(100)]
        [MinValue(0)]
        [MaxValue("@clip.frameRate * clip.length")]
        public int endFrame;

        [BoxGroup("Precompute Root Motion")]
        public GameObject objectToWarp;
        [InfoBox("Dont write the values by hand, use the button to precompute the values", InfoMessageType.Warning)]
        [BoxGroup("Precompute Root Motion")]
        public Vector3 totalRootMotionUntilWarpStartFrame;
        [BoxGroup("Precompute Root Motion")]
        public Vector3 totalRootMotionBetweenWarpStartAndEndFrame;
        [BoxGroup("Precompute Root Motion")]
        public Vector3 totalRootMotion;
        
        [FormerlySerializedAs("particleInstance")] [BoxGroup("Polish")]
        public EffectInstance effectInstance;
        [BoxGroup("Polish")] [Tooltip("In Air Sway Sound")]

        void OnEnable() {
            if(clip == null) {
                Debug.LogError("No clip assigned to warp animation");
                return;
            }
            
            #if(UNITY_EDITOR)
            Extensions.AnimationEventUtility.AddOrReplaceAnimationEvent(clip, startFrame / clip.frameRate, "OnWarpStart");
            Extensions.AnimationEventUtility.AddOrReplaceAnimationEvent(clip, endFrame / clip.frameRate, "OnWarpEnd");
            #endif
        }

        [Button("Perform Precompute", ButtonSizes.Large, ButtonStyle.CompactBox)]
        [GUIColor(0.4f, 0.8f, 1.0f)]
        [BoxGroup("Precompute Root Motion")]
        public void PrecomputeRootMotion() {
            if(clip == null) {
                Debug.LogError("No clip assigned to warp animation");
                return;
            }
            if(objectToWarp == null) {
                Debug.LogError("No animator assigned to warped object");
                return;
            }
        
            float frameRate = clip.frameRate;
            int totalFrames = Mathf.FloorToInt(clip.length * frameRate);
            
            // Compute total motion from 0 to startFrame and transform it to local space, because fuck rotation
            totalRootMotionUntilWarpStartFrame = objectToWarp.transform.
                InverseTransformDirection(CalculateMotionBetweenFrames(0, startFrame, frameRate));

            // Compute total motion between startFrame and endFrame, because fuck rotation
            totalRootMotionBetweenWarpStartAndEndFrame = objectToWarp.transform.
                InverseTransformDirection(CalculateMotionBetweenFrames(startFrame, endFrame, frameRate));

            // Compute total motion from 0 to the last frame, because fuck rotation
            totalRootMotion = objectToWarp.transform.
                InverseTransformDirection(CalculateMotionBetweenFrames(0, totalFrames - 1, frameRate));
        }

        // Helper method to compute motion between two frames
        Vector3 CalculateMotionBetweenFrames(int startFrame, int endFrame, float frameRate) {
            Vector3 totalMotion = Vector3.zero;
            for (int i = startFrame; i < endFrame; i++) {
                float startTime = i / frameRate;
                float endTime = (i + 1) / frameRate;
                Vector3 deltaPosition = GetRootMotionDeltaAtTime(startTime, endTime);
                totalMotion += deltaPosition;
            }
            return totalMotion;
        }

        // Existing GetRootMotionDeltaAtTime method should remain the same
        Vector3 GetRootMotionDeltaAtTime(float startTime, float endTime) {
            var animated = objectToWarp.gameObject;
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