using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Wait for end of Animation", story: "Wait for end of Animation on [Animator]", category: "Action/Animation", id: "6c7d980a47ea6b5ce81e4e037edd6555")]
public partial class WaitForEndOfAnimationAction : Action {
    [SerializeReference] public BlackboardVariable<Animator> Animator;
    [SerializeReference] public BlackboardVariable<int> LayerIndex;
    CountdownTimer _countdownTimer;
    protected override Status OnStart() {
        if(ReferenceEquals(Animator.Value, null)) {
            LogFailure("No Animator assigned.");
            return Status.Failure;
        }
        
        var waitTime = GetCurrentOrNextClipLength(LayerIndex.Value);        
        Debug.Log($"Waiting for {waitTime} seconds.");
        _countdownTimer = new CountdownTimer(waitTime);
        _countdownTimer.Start();
        return Status.Running;
    }

    float GetCurrentOrNextClipLength(int layerIndex) {
        if (Animator.Value.IsInTransition(layerIndex)) {
            // If Blendtree, returns average length of all clips in the blendtree
            var nextStateInfo = Animator.Value.GetNextAnimatorStateInfo(layerIndex);
            return nextStateInfo.length;
        }

        var currentClipInfo = Animator.Value.GetCurrentAnimatorClipInfo(layerIndex);
        return currentClipInfo.Length > 0 ? currentClipInfo[0].clip.length : 0f;
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

