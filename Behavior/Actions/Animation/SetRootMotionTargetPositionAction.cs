using System;
using Behavior.Enemy.State.Animation;
using Motion.RootMotion;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Root Motion Target Position", story: "Set [RootMotionEndPosition] position from [RootMotionDataWrapper] in [CurrentAnimationState] based on [Self] position", category: "Action", id: "2824f299514320b8fb4d4556125a2faf")]
public partial class SetRootMotionTargetPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> RootMotionEndPosition;
    [SerializeReference] public BlackboardVariable<RootMotionDataWrapper> RootMotionDataWrapper;
    [SerializeReference] public BlackboardVariable<AnimationController> AnimationController;
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<AnimationStates> CurrentAnimationState;
    [SerializeReference] public BlackboardVariable<float> MinAttackDistance;

    protected override Status OnStart()
{
    if (ReferenceEquals(RootMotionEndPosition, null) 
        || ReferenceEquals(RootMotionDataWrapper, null)
        || ReferenceEquals(Self, null)
        || ReferenceEquals(Target, null)
        || ReferenceEquals(AnimationController, null)) {
        Debug.LogError("RootMotionEndPosition, RootMotionDataWrapper, Target, AnimatorController or Self is missing.");
        return Status.Failure;
    }
    
    Vector3 selfPosition = Self.Value.transform.position;
    RootMotionAnimationDataSO bestRootMotionData = null;
    float bestDistance = float.MaxValue;

    foreach (var rmData in RootMotionDataWrapper.Value.RootMotionData) {
        var rmWorldRootMotion = Self.Value.transform.TransformDirection(rmData.totalRootMotion);
        rmWorldRootMotion.y = 0;

        var distanceToTarget = (Target.Value.transform.position - selfPosition).magnitude;

        if (!NavMesh.SamplePosition(selfPosition + rmWorldRootMotion, out NavMeshHit hit, 0.1f, NavMesh.AllAreas)) {
            continue;
        }

        if (distanceToTarget < bestDistance) {
            bestDistance = distanceToTarget;
            bestRootMotionData = rmData;
        }
    }

    if (bestRootMotionData == null) {
        Debug.LogError("No fitting Root Motion Data found.");
        return Status.Failure;
    }

    RootMotionEndPosition.Value = selfPosition + Self.Value.transform.TransformDirection(bestRootMotionData.totalRootMotion);
    GameObject debugObject = new GameObject("DebugRootMotionEndPosition");
    debugObject.transform.position = RootMotionEndPosition.Value;

    var oldAnimationClip = AnimationController.Value.GetInitialAttackClip(CurrentAnimationState.Value);
    AnimationController.Value.ReplaceClipFromOverrideController(oldAnimationClip, bestRootMotionData.clip);
    return Status.Success;
}

    protected override Status OnUpdate() {
        return Status.Success;
    }

    protected override void OnEnd() { }
}