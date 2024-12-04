using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Update Animator Speed from Agent Velocity", 
                story: "Sets the [Animator] BlendSpeed [Speed] with param [AnimatorSpeedParam] based on [Agent] velocity", 
                category: "Action/Animation", 
                id: "2c1bb4cb3a7ae46bf8f28c20070c0e2d")]
public partial class UpdateAnimatorSpeedFromAgentVelocity : Action {
    [SerializeReference] public BlackboardVariable<NavMeshAgent> Agent;
    [SerializeReference] public BlackboardVariable<Animator> Animator;
    [SerializeReference] public BlackboardVariable<float> Speed;
    [SerializeReference] public BlackboardVariable<float> VelocitySmoothing = new (0.1f);
    [SerializeReference] public BlackboardVariable<string> AnimatorSpeedParam;
    [SerializeReference] public BlackboardVariable<MoveDirection> MovementDirection;
    [SerializeReference] public BlackboardVariable<bool> ResetOnEnd = new (true);
    
    Vector3 _smoothDeltaPosition;
    Vector3 _velocity;

    protected override Status OnStart() {
        if (ReferenceEquals(Agent.Value, null) || ReferenceEquals(Animator.Value, null)) {
            LogFailure("No Agent or Animator assigned.");
            return Status.Failure;
        }
        if(string.IsNullOrEmpty(AnimatorSpeedParam.Value)) {
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
        
        // TODO: Slow down before reaching the target

        _smoothDeltaPosition = Vector3.SmoothDamp(
            _smoothDeltaPosition,
            worldDeltaPosition,
            ref _velocity,
            VelocitySmoothing
        );

        var speed = _smoothDeltaPosition.magnitude;
        if (MovementDirection.Value == MoveDirection.Backward) {
            speed = -speed;
        }
        
        Animator.Value.SetFloat(AnimatorSpeedParam.Value, speed, 0.5f, Time.deltaTime);
    }

    protected override void OnEnd() {
        if(ResetOnEnd.Value) {
            _velocity = Vector3.zero;
            Animator.Value.SetFloat(AnimatorSpeedParam.Value, 0);
        }
    }
    
}