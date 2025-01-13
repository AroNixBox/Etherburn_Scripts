using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Smoothly Rotate to Rotation", story: "[Transform] smoothly rotates to [TargetRotation]", category: "Action", id: "f37c4e5a1a963357c28ed46d183cb0ee")]
public partial class SmoothlyRotateToRotationAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Transform;
    [SerializeReference] public BlackboardVariable<Vector3> TargetRotation;
    [SerializeReference] public BlackboardVariable<bool> SignalOnComplete;
    [SerializeReference] public BlackboardVariable<bool> LimitToYAxis;
    [SerializeReference] public BlackboardVariable<float> RotationSpeed = new(5f);

    protected override Status OnStart() {
        var missingType = MissingType();
        if (missingType != null) {
            Debug.LogError($"Missing required BlackboardVariable of type {missingType}.");
            return Status.Failure;
        }

        return Status.Running;
    }

    Type MissingType() => ReferenceEquals(Transform.Value, null) ? typeof(Transform) : null;

    protected override Status OnUpdate() {
        if (ProcessSmoothLookAt()) { 
            return SignalOnComplete.Value ? Status.Success : Status.Running;
        }
        return Status.Running;
    }

    bool ProcessSmoothLookAt() {
        Vector3 targetEulerAngles = TargetRotation.Value;

        if (LimitToYAxis.Value) {
            targetEulerAngles.x = 0;
            targetEulerAngles.z = 0;
        }

        Quaternion targetQuaternion = Quaternion.Euler(targetEulerAngles);
        Transform.Value.rotation = Quaternion.Slerp(Transform.Value.rotation, targetQuaternion, Time.deltaTime * RotationSpeed.Value);

        // Check if the rotation is close enough to the target rotation
        if (Quaternion.Angle(Transform.Value.rotation, targetQuaternion) < 0.1f)
        {
            Transform.Value.rotation = targetQuaternion;
            return true;
        }

        return false;
    }
}