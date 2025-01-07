using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Determine Move Direction", story: "Determine [moveDirection] based on [self] and [target]", category: "Action", id: "f64a13287e931482a1c91d53d8f62046")]
public partial class DetermineMoveDirectionAction : Action
{
    [Tooltip("Output variable to store the determined move direction")]
    [SerializeReference] public BlackboardVariable<GameObject> self;
    [SerializeReference] public BlackboardVariable<MoveDirection> moveDirection;
    [SerializeReference] public BlackboardVariable<GameObject> target;

    protected override Status OnStart() {
        var missingType = MissingType();
        if(missingType != null) {
            Debug.LogError($"{missingType} is missing.");
            return Status.Failure;
        }
        
        // Calculate the direction to the target
        var directionToTarget = target.Value.transform.position - self.Value.transform.position;
        directionToTarget.y = 0; // Ignore vertical difference

        // Calculate the forward direction of the self object
        var forward = self.Value.transform.forward;
        forward.y = 0; // Ensure consistency by ignoring vertical component
        forward.Normalize();

        // Determine if the target is in front or behind
        var dotProduct = Vector3.Dot(forward, directionToTarget.normalized);

        moveDirection.Value = dotProduct > 0 ? MoveDirection.Forward : MoveDirection.Backward;

        return Status.Success;
    }
    
    Type MissingType() {
        if(ReferenceEquals(self.Value, null)) { return typeof(GameObject); }
        if(ReferenceEquals(target.Value, null)) { return typeof(GameObject); }
        
        return null;
    }

    protected override Status OnUpdate() {
        return Status.Success;
    }

    protected override void OnEnd() { }
}

