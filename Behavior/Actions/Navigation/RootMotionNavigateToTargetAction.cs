using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RootMotion Navigate to Target", story: "[Agent] root-moves to [Target]", category: "Action", id: "9e97d39273b3f1279f4549738afb9780")]
public partial class RootMotionNavigateToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<NavMeshAgent> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    
    Vector3 _lastTargetPosition;
    Vector3 _colliderAdjustedTargetPosition;

    protected override Status OnStart() {
        if (ReferenceEquals(Agent?.Value, null) || ReferenceEquals(Target, null)) {
            Debug.LogError("Agent or Target is missing.");
            return Status.Failure;
        }
        
        Agent.Value.SetDestination(Target.Value.transform.position);
        
        return Status.Running;
    }

    protected override Status OnUpdate() {
        // Check if the target position has changed.
        bool boolUpdateTargetPosition = !Mathf.Approximately(_lastTargetPosition.x, Target.Value.transform.position.x) 
                                        || !Mathf.Approximately(_lastTargetPosition.y, Target.Value.transform.position.y) 
                                        || !Mathf.Approximately(_lastTargetPosition.z, Target.Value.transform.position.z);
        if (boolUpdateTargetPosition) {
            _lastTargetPosition = Target.Value.transform.position;
            _colliderAdjustedTargetPosition = GetPositionColliderAdjusted();
            Agent.Value.SetDestination(_colliderAdjustedTargetPosition);
        }
        
        return Agent.Value.remainingDistance <= Agent.Value.stoppingDistance 
            ? Status.Success 
            : Status.Running;
    }
    
    /// <returns>Get the position of the target adjusted to the collider.</returns>
    Vector3 GetPositionColliderAdjusted() {
        Collider targetCollider = Target.Value.GetComponentInChildren<Collider>();
        if (targetCollider != null)
        {
            return targetCollider.ClosestPoint(Agent.Value.transform.position);
        }
        return Target.Value.transform.position;
    }

    protected override void OnEnd() {
        if(ReferenceEquals(Agent?.Value, null)) { return; }
        
        if(Agent.Value.isOnNavMesh) {
            Agent.Value.ResetPath();
        }
    }
}

