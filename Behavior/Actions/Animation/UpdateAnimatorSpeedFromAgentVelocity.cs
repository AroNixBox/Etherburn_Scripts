using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Update Animator Speed from Agent Velocity", 
                story: "Sets the [Animator] BlendSpeed [Speed] with param [AnimatorSpeedParam] based on [Agent] velocity", 
                category: "Action", 
                id: "2c1bb4cb3a7ae46bf8f28c20070c0e2d")]
public partial class UpdateAnimatorSpeedFromAgentVelocity : Action {
    [SerializeReference] public BlackboardVariable<NavMeshAgent> Agent;
    [SerializeReference] public BlackboardVariable<Animator> Animator;
    [SerializeReference] public BlackboardVariable<float> Speed;
    [SerializeReference] public BlackboardVariable<float> VelocitySmoothing = new (0.1f);
    [SerializeReference] public BlackboardVariable<string> AnimatorSpeedParam;
    
    Vector3 _smoothDeltaPosition;
    Vector3 _velocity;

    protected override Status OnStart() {
        if (Agent.Value == null || Animator.Value == null) {
            LogFailure("No Agent or Animator assigned.");
            return Status.Failure;
        }
        if(string.IsNullOrEmpty(AnimatorSpeedParam.Value) || Speed.Value == 0) {
            LogFailure("No Animator Speed Param or Speed assigned.");
            return Status.Failure;
        }
        
        Agent.Value.speed = Speed.Value;
        return Status.Running;
    }

    protected override Status OnUpdate() {
        if (!Agent.Value.hasPath) {
            HandleNoPath();
            return Status.Running;
        }
        UpdateAnimationSpeed();
    
        return Status.Running;
    }

    void HandleNoPath() {
        _velocity = Vector3.zero;
        Animator.Value.SetFloat(AnimatorSpeedParam.Value, 0);
    }

    void UpdateAnimationSpeed() {
        Vector3 worldDeltaPosition = Agent.Value.desiredVelocity;
        float distanceToTarget = Agent.Value.remainingDistance;
        float slowdownDistance = Agent.Value.stoppingDistance * 4f;
        
        // TODO: Handle properly slowing down, currently its based on a hardcoded value.
        // Slow down in the last part of the path
        if (distanceToTarget <= slowdownDistance) {
            // Ensure we reach the target, even tho target could be very close to us and we are starting to run.
            float normalizedDistance = Mathf.Clamp01(distanceToTarget / slowdownDistance);
            // Lerp between half speed and full speed (Because Idle = 0, Walk is Half and Run is Full)
            float targetSpeed = Mathf.Lerp(Agent.Value.speed / 2f, Agent.Value.speed, normalizedDistance);
        
            worldDeltaPosition = worldDeltaPosition.normalized * targetSpeed;
        }

        _smoothDeltaPosition = Vector3.SmoothDamp(
            _smoothDeltaPosition,
            worldDeltaPosition,
            ref _velocity,
            VelocitySmoothing
        );

        var speed = _smoothDeltaPosition.magnitude;
        Animator.Value.SetFloat(AnimatorSpeedParam.Value, speed, 0.5f, Time.deltaTime);
    }

    protected override void OnEnd() {
        _velocity = Vector3.zero;
        Animator.Value.SetFloat(AnimatorSpeedParam.Value, 0);
    }
}