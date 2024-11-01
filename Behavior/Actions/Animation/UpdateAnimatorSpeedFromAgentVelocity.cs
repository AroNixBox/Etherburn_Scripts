using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

// If using this class, make sure to Reset the Agent Path when reaching the destination, since this class depends on it.
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Update Animator Speed from Agent Velocity", story: "Sets the [Animator] BlendSpeed [Speed] with param [AnimatorSpeedParam] based on [Agent] velocity", category: "Action", id: "2c1bb4cb3a7ae46bf8f28c20070c0e2d")]
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
        SlowDownToStop();
    
        if (_velocity.magnitude < 0.01f) {
            ResetVelocity();
            return;
        }
    
        // Update Animator with Current Velocity
        Animator.Value.SetFloat(
            AnimatorSpeedParam.Value, 
            _velocity.magnitude, 
            VelocitySmoothing, 
            Time.deltaTime
        );
    }

    void SlowDownToStop() {
        _velocity = Vector3.Lerp(_velocity, Vector3.zero, 5f * Time.deltaTime);
    }

    void ResetVelocity() {
        _velocity = Vector3.zero;
        Animator.Value.SetFloat(AnimatorSpeedParam.Value, 0);
    }

    void UpdateAnimationSpeed() {
        Vector3 worldDeltaPosition = Agent.Value.desiredVelocity;
        _smoothDeltaPosition = Vector3.SmoothDamp(
            _smoothDeltaPosition,
            worldDeltaPosition,
            ref _velocity,
            VelocitySmoothing
        );
    
        var speed = _smoothDeltaPosition.magnitude;
        Animator.Value.SetFloat(AnimatorSpeedParam.Value, speed, .5f, Time.deltaTime);
    }

    protected override void OnEnd() {
        ResetVelocity();
    }
}