using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RootMotion Patrol between Waypoints", story: "[Agent] root-patrols between [Waypoints]", category: "Action", id: "898860062991204839cd797780ebca0b")]
public partial class RootMotionPatrolBetweenWaypointsAction : Action
{
    [SerializeReference] public BlackboardVariable<NavMeshAgent> Agent;
    [SerializeReference] public BlackboardVariable<List<GameObject>> Waypoints;
    [SerializeReference] public BlackboardVariable<float> WaitTimeAtPoint = new(2f);

    CountdownTimer _waitTimer;
    int _currentPointIndex;
    
    protected override Status OnStart() {
        if (Agent.Value == null || Waypoints.Value == null || Waypoints.Value.Count == 0) {
            LogFailure("No Agent assigned or patrol points are missing.");
            return Status.Failure;
        }

        _currentPointIndex = 0;
        _waitTimer ??= new CountdownTimer(WaitTimeAtPoint.Value);
        _waitTimer.OnTimerStop += MoveToNextPoint;
        MoveToNextPoint();

        return Status.Running;
    }

    protected override Status OnUpdate() {
    if (_waitTimer.IsRunning) {
        // Handle wait time at each point
        _waitTimer.Tick(Time.deltaTime);
        return Status.Running;
    }
    
    if (Agent.Value.remainingDistance <= Agent.Value.stoppingDistance) {
        Agent.Value.ResetPath();
        
        // Start waiting until the wait time is over
        _waitTimer.Start();
    } else {
        // Smoothly rotate towards the target
        Vector3 direction = (Agent.Value.steeringTarget - Agent.Value.transform.position).normalized;
        if (direction != Vector3.zero) {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Agent.Value.transform.rotation = Quaternion.Slerp(Agent.Value.transform.rotation, targetRotation, Time.deltaTime * 2.5f);
        }
    }

    return Status.Running;
}

    void MoveToNextPoint() {
        // Set the next patrol point as the agent's destination
        Agent.Value.SetDestination(Waypoints.Value[_currentPointIndex].transform.position);

        // Cycle through patrol points
        _currentPointIndex = (_currentPointIndex + 1) % Waypoints.Value.Count;
    }

    protected override void OnEnd() {
        _waitTimer.OnTimerStop -= MoveToNextPoint;
        _waitTimer.Stop();
    }
}

