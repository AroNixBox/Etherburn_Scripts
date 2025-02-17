using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Get Current NavMesh Position or Rotation", story: "Get [Data] as [DataToStore] from [Self]", category: "Action", id: "b83ab0c1838ef827360ccf752616794d")]
public partial class GetCurrentNavMeshPositionAction : Action
{
    public enum DataType {
        Position,
        Rotation
    }

    [SerializeReference] public BlackboardVariable<Vector3> Data;
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<DataType> DataToStore;

    protected override Status OnStart() {
        var missingType = MissingType();
        if (missingType != null) {
            Debug.LogError($"Missing type: {missingType}");
            return Status.Failure;
        }

        switch (DataToStore.Value) {
            case DataType.Position:
                Data.Value = Self.Value.transform.position;
                break;
            case DataType.Rotation:
                Data.Value = Self.Value.transform.rotation.eulerAngles;
                break;
            default:
                Debug.LogError("Unsupported data type");
                return Status.Failure;
        }

        return Status.Running;
    }

    Type MissingType() => ReferenceEquals(Self.Value, null) ? typeof(GameObject) : null; // If all checks passed, no type is missing

    protected override Status OnUpdate() {
        return Status.Success;
    }

    protected override void OnEnd() {
    }
}