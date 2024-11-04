using System;
using Player.Animation;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Root Motion Target Position", story: "Set [RootMotionEndPosition] position from [RootMotionDataWrapper] based on [Self] position", category: "Action", id: "2824f299514320b8fb4d4556125a2faf")]
public partial class SetRootMotionTargetPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> RootMotionEndPosition;
    [SerializeReference] public BlackboardVariable<RootMotionDataWrapper> RootMotionDataWrapper;
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    protected override Status OnStart()
    {
        if (ReferenceEquals(RootMotionEndPosition, null) || ReferenceEquals(RootMotionDataWrapper, null) || ReferenceEquals(Self, null)) {
            Debug.LogError("RootMotionEndPosition, RootMotionDataWrapper or Self is missing.");
            return Status.Failure;
        }

        Vector3 selfPosition = Self.Value.transform.position;
        RootMotionAnimationDataSO bestRootMotionData = null;
        float bestDistance = float.MaxValue;
        // Get the Root Motion that would fit the distance to the target
        foreach (var rmData in RootMotionDataWrapper.Value.RootMotionData) {
            var rmWorldRootMotion = Self.Value.transform.TransformDirection(rmData.totalRootMotion);
            rmWorldRootMotion.y = 0;
            if(Target.Value == null) {
                Debug.LogError("No Target Assigned, but trying to find a fitting Attack.");
                return Status.Failure;
            }
            // check distance to target
            var distanceToTarget = Vector3.Distance(Target.Value.transform.position, selfPosition + rmWorldRootMotion);
            if (distanceToTarget < bestDistance) {
                bestDistance = distanceToTarget;
                bestRootMotionData = rmData;
            }
        }
        
        if (bestRootMotionData == null) {
            Debug.LogError("No Root Motion Data found.");
            return Status.Failure;
        }

        // Convert totalRootMotion to world space
        Vector3 worldRootMotion = Self.Value.transform.TransformDirection(bestRootMotionData.totalRootMotion);
        Debug.Log("Chose: " + bestRootMotionData.name + " with distance: " + bestDistance);
        // TODO: Take the BestMotionData Animation Clip
        // Ensure each component is correctly added
        RootMotionEndPosition.Value = selfPosition + worldRootMotion;

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