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
    [SerializeReference] public BlackboardVariable<bool> ApplyRotation;
    [SerializeReference] public BlackboardVariable<float> RotationSpeed = new (5.0f);
    
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
        if (HasTargetMoved()) {
            _lastTargetPosition = Target.Value.transform.position;
            _colliderAdjustedTargetPosition = GetPositionColliderAdjusted();
            Agent.Value.SetDestination(_colliderAdjustedTargetPosition);
        }

        if (ApplyRotation) {
            RotateTowardsTargetLocation();
        }
        
        return Agent.Value.remainingDistance <= Agent.Value.stoppingDistance 
            ? Status.Success 
            : Status.Running;
    }
    bool HasTargetMoved() {
        return !Mathf.Approximately(_lastTargetPosition.x, Target.Value.transform.position.x) 
               || !Mathf.Approximately(_lastTargetPosition.y, Target.Value.transform.position.y) 
               || !Mathf.Approximately(_lastTargetPosition.z, Target.Value.transform.position.z);
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
    
    void RotateTowardsTargetLocation() {
        Vector3 direction = (Agent.Value.steeringTarget - Agent.Value.transform.position).normalized;
        if (direction != Vector3.zero) {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Agent.Value.transform.rotation = Quaternion.Slerp(Agent.Value.transform.rotation, targetRotation, Time.deltaTime * 2.5f);
        }
    }

    protected override void OnEnd() {
        if(ReferenceEquals(Agent?.Value, null)) { return; }
        
        if(Agent.Value.isOnNavMesh) {
            Agent.Value.ResetPath();
        }
    }
}

