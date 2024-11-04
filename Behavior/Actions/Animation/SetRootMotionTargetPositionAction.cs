using System;
using Behavior.Enemy.State.Animation;
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
        
        // TODO: Maybe we need to seperate the selecting right rmData and setting the position into seperate nodes
        // TODO: The problem is, that if we will not find any data, this node will fail and the enemy is stuck.
        
        Vector3 selfPosition = Self.Value.transform.position;
        // TODO: Change Destination + Namespace
        Player.Animation.RootMotionAnimationDataSO bestRootMotionData = null;
        float bestDistance = float.MaxValue;
        Vector3 rmWorldRootMotion = Vector3.zero;
        // Get the Root Motion that would fit the distance to the target
        foreach (var rmData in RootMotionDataWrapper.Value.RootMotionData) {
            rmWorldRootMotion = Self.Value.transform.TransformDirection(rmData.totalRootMotion);
            // Ignore Y axis for comparison, since it is not affecting Root Motion anyways.
            rmWorldRootMotion.y = 0;
            // check distance to target
            var distanceToTarget = Vector3.Distance(Target.Value.transform.position, Self.Value.transform.position + rmWorldRootMotion);
            
            // TODO: We shall not use our Y axis for the sampleposition, since it is not affecting the root motion anyways.
            // TODO: Not sure if this is already handled, test it and maybe change it here
            // Check if the Distance is reachable on the navmesh
            if (!NavMesh.SamplePosition(selfPosition + rmWorldRootMotion, out NavMeshHit hit, 0.1f, NavMesh.AllAreas)) {
                continue;
            }
            CreateVisualMarker("RootMotionTarget", selfPosition + rmWorldRootMotion);
            CreateVisualMarker("NavmeshPosition", hit.position);
            
            if (distanceToTarget < bestDistance) {
                bestDistance = distanceToTarget;
                bestRootMotionData = rmData;
            }
        }
        
        if (bestRootMotionData == null) {
            Debug.LogError("No fitting Root Motion Data found.");
            return Status.Failure;
        }
        
        
        RootMotionEndPosition.Value = selfPosition + rmWorldRootMotion;
        
        var oldAnimationClip = AnimationController.Value.GetInitialAttackClip(CurrentAnimationState.Value);
        
        AnimationController.Value.ReplaceClipFromOverrideController(oldAnimationClip, bestRootMotionData.clip);
        return Status.Success;
    }

    protected override Status OnUpdate() {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }

    void CreateVisualMarker(string name, Vector3 position) {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = name;
        marker.transform.position = position;
        marker.transform.localScale = Vector3.one * 0.1f; //
    }
}