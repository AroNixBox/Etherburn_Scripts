using Effects.VFX;
using Motion.RootMotion;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player.Animation.MotionWarp {
    [CreateAssetMenu(fileName = "Warp Animation", menuName = "Animation Helper/Warp Animation")]
    public class WarpAnimation : RootMotionAnimationDataSO {
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
        public Vector3 totalRootMotionUntilWarpStartFrame;
        [BoxGroup("Precompute Root Motion")]
        public Vector3 totalRootMotionBetweenWarpStartAndEndFrame;
        
        [FormerlySerializedAs("particleInstance")] [BoxGroup("Polish")]
        public EffectInstance effectInstance;

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
        public override void PrecomputeRootMotion() {
            if(clip == null) {
                Debug.LogError("No clip assigned to warp animation");
                return;
            }
            if(targetObject == null) {
                Debug.LogError("No animator assigned to warped object");
                return;
            }
        
            // Compute total motion from 0 to startFrame and transform it to local space, because fuck rotation
            totalRootMotionUntilWarpStartFrame = targetObject.transform.
                InverseTransformDirection(CalculateMotionBetweenFrames(0, startFrame, clip.frameRate));

            // Compute total motion between startFrame and endFrame, because fuck rotation
            totalRootMotionBetweenWarpStartAndEndFrame = targetObject.transform.
                InverseTransformDirection(CalculateMotionBetweenFrames(startFrame, endFrame, clip.frameRate));
            
            base.PrecomputeRootMotion();
        }
    }
}