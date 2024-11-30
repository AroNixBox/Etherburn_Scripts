using System;
using Drawing;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Root Motion Navigate to Location", story: "[Agent] root-moves to [Location]", category: "Action/Navigation/RootMotion", id: "f95b5de3fa3e49cd85c521afc0c1464c")]
public partial class RootMotionNavigateToLocationAction : Action
{
    [SerializeReference] public BlackboardVariable<NavMeshAgent> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> Location;
    [SerializeReference] public BlackboardVariable<bool> SignalOnArrival = new (true);
    [SerializeReference] public BlackboardVariable<MoveDirection> MovementDirection;

    protected override Status OnStart() {
        if (ReferenceEquals(Agent?.Value, null)) {
            Debug.LogError("Agent is missing.");
            return Status.Failure;
        }
        
        Agent.Value.SetDestination(Location.Value);

        if (SignalOnArrival.Value) {
            // Already at the destination
            if ((Agent.Value.transform.position - Location.Value).magnitude <= Agent.Value.stoppingDistance) {
                return Status.Success;
            }
        }
        
        return Status.Running;
    }

    protected override Status OnUpdate() {
        // If the Location has changed, update the destination
        if (Agent.Value.destination != Location.Value) {
            Debug.Log("Destination changed");
            Agent.Value.SetDestination(Location.Value);
        }
        
        RotateTowardsTargetLocation();
        
        return Agent.Value.remainingDistance <= Agent.Value.stoppingDistance && SignalOnArrival.Value
            ? Status.Success 
            : Status.Running;
    }

    protected override void OnEnd() {
        if(ReferenceEquals(Agent?.Value, null)) { return; }
        if(Agent.Value.isOnNavMesh) {
            Agent.Value.ResetPath();
        }
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
}
