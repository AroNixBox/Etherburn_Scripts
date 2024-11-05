using System.Collections.Generic;
using Motion;
using Motion.RootMotion;
using Player.Animation;
using UnityEngine;

public class RootMotionDataWrapper : MonoBehaviour {
    [field: SerializeField] public List<RootMotionAnimationDataSO> RootMotionData { get; private set; }
}
