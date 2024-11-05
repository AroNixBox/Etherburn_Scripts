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
    
    
    Vector3 _lastTargetPosition;
    Vector3 _colliderAdjustedTargetPosition;

    protected override Status OnStart() {
        if (ReferenceEquals(Agent?.Value, null) || ReferenceEquals(Location, null)) {
            Debug.LogError("Agent or Target is missing.");
            return Status.Failure;
        }
        
        if ((Agent.Value.transform.position - Location.Value).magnitude
            <= Agent.Value.stoppingDistance) {
            return Status.Success;
        }
        
        Agent.Value.SetDestination(Location.Value);
        
        return Status.Running;
    }

    protected override Status OnUpdate() {
        return Agent.Value.remainingDistance <= Agent.Value.stoppingDistance 
            ? Status.Success 
            : Status.Running;
    }
    protected override void OnEnd() {
        if(ReferenceEquals(Agent?.Value, null)) { return; }
        
        if(Agent.Value.isOnNavMesh) {
            Agent.Value.ResetPath();
        }
    }
}

