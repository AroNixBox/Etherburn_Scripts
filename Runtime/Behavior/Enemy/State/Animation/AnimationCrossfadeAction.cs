using Behavior.Enemy.State.Animation;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Animation Crossfade", story: "Trigger [AnimationState] crossfade in [AnimationController] and waits for blend [WaitForCompletion]", category: "Action", id: "553a02823f79e7799504018b57b3c18a")]
public partial class AnimationCrossfadeAction : Action {
    [SerializeReference] public BlackboardVariable<AnimationStates> AnimationState;
    [SerializeReference] public BlackboardVariable<AnimationController> AnimationController;
    [Tooltip("If true, the action waits for the blend to finish before returning success.")]
    [SerializeReference] public BlackboardVariable<bool> WaitForCompletion;
    CountdownTimer _timer;

    protected override Status OnStart() {
        var animationDetails = AnimationsParams.GetAnimationDetails(AnimationState.Value);
        var blendDuration = animationDetails.BlendDuration;
        _timer = new CountdownTimer(blendDuration);
        _timer.Start();
        
        AnimationController.Value.CrossfadeToState(animationDetails);
        return Status.Running;
    }

    protected override Status OnUpdate() {
        if (_timer.IsFinished) { // If timer wasnt startet, IsFinished will return true
            return Status.Success;
        } 
        _timer.Tick(Time.deltaTime);
        return Status.Running;
    }

    protected override void OnEnd() {
        _timer = null;
    }
}

