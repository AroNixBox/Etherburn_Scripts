using Behavior.Enemy.State.Animation;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Animation Crossfade", story: "Trigger [AnimationState] crossfade in [AnimationController]", category: "Action", id: "553a02823f79e7799504018b57b3c18a")]
public partial class AnimationCrossfadeAction : Action {
    [SerializeReference] public BlackboardVariable<NPCAnimationStates> AnimationState;
    [SerializeReference] public BlackboardVariable<AnimationController> AnimationController;
    
    CountdownTimer _countdownTimer;

    protected override Status OnStart() {
        var missingType = MissingType();
        if(missingType != null) {
            Debug.LogError($"{missingType} is missing.");
            return Status.Failure;
        }
        
        var animationDetails = AnimationsParams.GetAnimationDetails(AnimationState.Value);
        AnimationController.Value.CrossfadeToState(animationDetails);
        
        _countdownTimer ??= new CountdownTimer(animationDetails.BlendDuration);
        return Status.Running;
    }
    
    Type MissingType() {
        return ReferenceEquals(AnimationController.Value, null) ? typeof(AnimationController) : null; // If all checks passed, no type is missing
    }
    protected override Status OnUpdate() {
        if (_countdownTimer.IsFinished) {
            return Status.Success;
        }
        _countdownTimer.Tick(Time.deltaTime);
        return Status.Running;
    }
}

