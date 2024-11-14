using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Collider Active State", story: "Set [Collider] [Active]", category: "Action/Collision", id: "c9e82f8506b0edb29e0e3f35e663f9da")]
public partial class SetColliderAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Collider;
    [SerializeReference] public BlackboardVariable<bool> Active;

    protected override Status OnStart()
    {
        if(ReferenceEquals(Collider.Value, null)) {
            LogFailure("No Collider assigned.");
            return Status.Failure;
        }
        
        if(!Collider.Value.TryGetComponent(out Collider collider)) {
            LogFailure("No Collider component found on: " + Collider.Value.name);
            return Status.Failure;
        }
        
        collider.enabled = Active.Value;
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

