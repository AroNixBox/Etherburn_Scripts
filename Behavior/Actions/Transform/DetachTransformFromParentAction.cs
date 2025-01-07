using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Detach Transform from Parent", story: "Detach [Transform] from Parent", category: "Action", id: "8715ed1962d3fe0cf13aee079b90ef2d")]
public partial class DetachTransformFromParentAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Transform;

    protected override Status OnStart() {
        var missingType = MissingType();
        if (missingType != null) {
            Debug.LogError($"Missing Type: {missingType}");
            return Status.Failure;
        }
        
        Transform.Value.SetParent(null);
        return Status.Success;
    }
    
    Type MissingType() {
        if(ReferenceEquals(Transform.Value, null)) { return typeof(Transform); }
        
        return null;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd() { }
}

