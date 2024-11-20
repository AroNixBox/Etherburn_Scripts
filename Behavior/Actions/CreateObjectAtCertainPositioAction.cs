using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Create Object at Certain Positio", story: "Create [Object] at Certain [Position]", category: "Action", id: "31a031e8e98359398d67882b34db3eb7")]
public partial class CreateObjectAtCertainPositioAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Object;
    [SerializeReference] public BlackboardVariable<Vector3> Position;

    protected override Status OnStart()
    {
        UnityEngine.Object.Instantiate(Object.Value, Position.Value, Quaternion.identity);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

