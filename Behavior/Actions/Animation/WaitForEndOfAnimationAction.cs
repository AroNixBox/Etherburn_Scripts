using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitForEndOfAnimation", story: "Wait for end of Animation on [Animator]", category: "Action", id: "6c7d980a47ea6b5ce81e4e037edd6555")]
public partial class WaitForEndOfAnimationAction : Action {
    [SerializeReference] public BlackboardVariable<Animator> Animator;
    
    CountdownTimer _countdownTimer;
    protected override Status OnStart() {
        var stateInfo = Animator.Value.GetCurrentAnimatorStateInfo(0);
        var waitTime = stateInfo.length;        
        _countdownTimer = new CountdownTimer(waitTime);
        _countdownTimer.Start();

        return Status.Running;
    }

    protected override Status OnUpdate() {
        _countdownTimer.Tick(Time.deltaTime);
        if (_countdownTimer.IsFinished) {
            return Status.Success;
        }
        return Status.Running;
    }

    protected override void OnEnd() { }
}

