using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Smoothly Look At",
    description: "Smoothly rotates the Transform to look at the Target.",
    story: "[Transform] smoothly looks at [Target]",
    category: "Action/Transform",
    id: "new_unique_id_here")]
public partial class SmoothlyLookAtAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Transform;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<bool> SignalOnComplete;
    [SerializeReference] public BlackboardVariable<bool> LimitToYAxis;
    [SerializeReference] public BlackboardVariable<float> RotationSpeed = new(5f);

    protected override Status OnStart()
    {
        var missingType = MissingType();
        if (missingType != null) {
            Debug.LogError($"Missing required BlackboardVariable of type {missingType}.");
            return Status.Failure;
        }

        return Status.Running;
    }

    Type MissingType() {
        if(ReferenceEquals(Transform.Value, null)) { return typeof(Transform); }
        if(ReferenceEquals(Target.Value, null)) { return typeof(Transform); }
        
        return null;
    }

    protected override Status OnUpdate() {
        if (ProcessSmoothLookAt()) { 
            return SignalOnComplete.Value ? Status.Success : Status.Running;
        }
        return Status.Running;
    }

    bool ProcessSmoothLookAt() {
        Vector3 targetPosition = Target.Value.position;

        if (LimitToYAxis.Value)
        {
            targetPosition.y = Transform.Value.position.y;
        }

        Vector3 direction = (targetPosition - Transform.Value.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Transform.Value.rotation = Quaternion.Slerp(Transform.Value.rotation, targetRotation, Time.deltaTime * RotationSpeed.Value);

            // Check if the rotation is close enough to the target rotation
            if (Quaternion.Angle(Transform.Value.rotation, targetRotation) < 0.1f)
            {
                Transform.Value.rotation = targetRotation;
                return true;
            }
        }
        return false;
    }
}