using System;
using Drawing;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RootMotion Navigate to Target", story: "[Agent] root-moves to [Target]", category: "Action/Navigation/RootMotion", id: "9e97d39273b3f1279f4549738afb9780")]
public partial class RootMotionNavigateToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<NavMeshAgent> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<MoveDirection> MovementDirection;
    [SerializeReference] public BlackboardVariable<bool> SignalOnArrival = new (true);
    
    Vector3 _lastTargetPosition;
    Vector3 _colliderAdjustedTargetPosition;

    protected override Status OnStart() {
        var missingType = MissingType();
        if(missingType != null) {
            Debug.LogError($"{missingType} is missing.");
            return Status.Failure;
        }
        
        Agent.Value.SetDestination(Target.Value.transform.position);

        if (SignalOnArrival.Value) {
            // Already at the destination
            if ((Agent.Value.transform.position - Target.Value.transform.position).magnitude <= Agent.Value.stoppingDistance) {
                return Status.Success;
            }
        }
        
        return Status.Running;
    }
    
    Type MissingType() {
        if(ReferenceEquals(Agent.Value, null)) { return typeof(NavMeshAgent); }
        if(ReferenceEquals(Target.Value, null)) { return typeof(GameObject); }
        
        return null;
    }

    protected override Status OnUpdate() {
        // Check if the target position has changed.
        if (HasTargetMoved()) {
            _lastTargetPosition = Target.Value.transform.position;
            _colliderAdjustedTargetPosition = GetPositionColliderAdjusted();
            Agent.Value.SetDestination(_colliderAdjustedTargetPosition);
        }

        RotateTowardsTargetLocation();

        
        return SignalOnArrival.Value && Agent.Value.remainingDistance <= Agent.Value.stoppingDistance
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
        direction.y = 0;
        if (direction == Vector3.zero) { return; }
        if(MovementDirection.Value == MoveDirection.Backward) {
            direction = -direction;
        }
        // Smoothly look at the target
        float desiredRotationTimeFor360 = 60f / Agent.Value.angularSpeed;
        float slerpSpeed = 1f / desiredRotationTimeFor360;
        Agent.Value.transform.rotation = Quaternion.Slerp(
            Agent.Value.transform.rotation, 
            Quaternion.LookRotation(direction), 
            Time.deltaTime * slerpSpeed);
        
#if UNITY_EDITOR
        DrawArrowToSteerTarget(direction);
#endif
    }

#if UNITY_EDITOR
    void DrawArrowToSteerTarget(Vector3 direction) {
        Vector3 heightPoint = Vector3.up * 2f;
        Vector3 startPoint = Agent.Value.transform.position + heightPoint; // Starts above the head
        Vector3 endPoint = Agent.Value.transform.position + heightPoint + direction * (Agent.Value.steeringTarget - Agent.Value.transform.position).magnitude;            // Draw an arrow from the agent to the target with Aline Asset
        using (Draw.WithColor(Color.white)) {
            Draw.Arrow(startPoint, endPoint);
            Draw.WireBox(endPoint, Vector3.one * 0.1f);
        }
    }
#endif
    
    protected override void OnEnd() {
        if(ReferenceEquals(Agent?.Value, null)) { return; }
        if(Agent.Value.isOnNavMesh) {
            Agent.Value.ResetPath();
        }
    }
}

