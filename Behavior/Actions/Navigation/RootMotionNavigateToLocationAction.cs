using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Root Motion Navigate to Location", story: "[Agent] root-moves to [Location]", category: "Action", id: "f95b5de3fa3e49cd85c521afc0c1464c")]
public partial class RootMotionNavigateToLocationAction : Action
{
    [SerializeReference] public BlackboardVariable<NavMeshAgent> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> Location;

    [SerializeReference] public BlackboardVariable<bool> ApplyRotation;
    [SerializeReference] public BlackboardVariable<float> RotationSpeed = new (5.0f);

    protected override Status OnStart() {
        if (ReferenceEquals(Agent?.Value, null) || ReferenceEquals(Location, null)) {
            Debug.LogError("Agent or Target is missing.");
            return Status.Failure;
        }
        
        if ((Agent.Value.transform.position - Location.Value).magnitude <= Agent.Value.stoppingDistance) {
            return Status.Success;
        }
        
        Agent.Value.SetDestination(Location.Value);
        
        return Status.Running;
    }

    protected override Status OnUpdate() {
        if (Agent.Value.remainingDistance <= Agent.Value.stoppingDistance) {
            return Status.Success;
        }

        if (ApplyRotation) {
            RotateTowardsTargetLocation();
        }
        
        return Status.Running;
    }

    protected override void OnEnd() {
        if(ReferenceEquals(Agent?.Value, null)) { return; }
        // Force LookAt Target
        Agent.Value.transform.LookAt(Agent.Value.steeringTarget);
        if(Agent.Value.isOnNavMesh) {
            Agent.Value.ResetPath();
        }
    }
    
    void RotateTowardsTargetLocation() {
        Vector3 direction = (Agent.Value.steeringTarget - Agent.Value.transform.position).normalized;
        if (direction != Vector3.zero) {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Agent.Value.transform.rotation = Quaternion.Slerp(Agent.Value.transform.rotation, targetRotation, Time.deltaTime * 2.5f);
        }
    }
}
