using Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player.Animation.MotionWarp {
    [System.Serializable]
    public class WarpData {
        [SerializeField] WarpAnimation[] finisherWarpAnimation;
        [HideLabel] public AttributeData attributeData;
        public WarpAnimation GetFinisherWarpAnimation(int index) => finisherWarpAnimation[index];
        public int WarpCount => finisherWarpAnimation.Length;
    }
}