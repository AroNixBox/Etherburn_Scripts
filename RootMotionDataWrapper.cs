using System;
using System.Collections.Generic;
using Motion.RootMotion;
using UnityEngine;

public class RootMotionDataWrapper : MonoBehaviour {
    [SerializeField] List<RootMotionAnimationDataSO> rootMotionAttackData;
    [SerializeField] List<RootMotionAnimationDataSO> rootMotionHurtData;
    public List<RootMotionAnimationDataSO> GetRootMotionData(RootMotionType rootMotionType) {
        return rootMotionType switch {
            RootMotionType.Attack => rootMotionAttackData,
            RootMotionType.Hurt => rootMotionHurtData,
            _ => throw new Exception("RootMotionType not found")
        };
    }
    public enum RootMotionType {
        Attack,
        Hurt
    }
}
