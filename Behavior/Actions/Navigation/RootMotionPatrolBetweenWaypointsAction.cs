using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RootMotion Patrol between Waypoints", story: "[Agent] root-patrols between [Waypoints]", category: "Action/Navigation/RootMotion", id: "898860062991204839cd797780ebca0b")]
public partial class RootMotionPatrolBetweenWaypointsAction : Action
{
    [SerializeReference] public BlackboardVariable<NavMeshAgent> Agent;
    [SerializeReference] public BlackboardVariable<List<GameObject>> Waypoints;
    [SerializeReference] public BlackboardVariable<float> WaitTimeAtPoint = new(2f);
    
    [SerializeReference] public BlackboardVariable<bool> ApplyRotation;
    [SerializeReference] public BlackboardVariable<float> RotationSpeed = new (5.0f);

    CountdownTimer _waitTimer;
    int _currentPointIndex;
    
    protected override Status OnStart() {
        var missingType = MissingType();
        if(missingType != null) {
            Debug.LogError($"{missingType} is missing.");
            return Status.Failure;
        }
        
        if (Waypoints.Value.Count == 0 || Waypoints.Value.Any(waypoint => waypoint == null)) {
            LogFailure("Too few waypoints or one of the waypoints is null.");
            return Status.Failure;
        }

        _currentPointIndex = 0;
        _waitTimer ??= new CountdownTimer(WaitTimeAtPoint.Value);
        _waitTimer.OnTimerStop += MoveToNextPoint;
        MoveToNextPoint();

        return Status.Running;
    }
    Type MissingType() {
        if(ReferenceEquals(Agent.Value, null)) { return typeof(NavMeshAgent); }
        if(ReferenceEquals(Waypoints.Value, null)) { return typeof(List<GameObject>); }
        
        return null;
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
            if (ApplyRotation) {
                // Smoothly rotate towards the target
                RotateTowardsTargetLocation();
            }
        }

        return Status.Running;
    }

    void RotateTowardsTargetLocation() {
        Vector3 direction = (Agent.Value.steeringTarget - Agent.Value.transform.position).normalized;
        if (direction != Vector3.zero) {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Agent.Value.transform.rotation = Quaternion.Slerp(Agent.Value.transform.rotation, targetRotation, Time.deltaTime * 2.5f);
        }
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

