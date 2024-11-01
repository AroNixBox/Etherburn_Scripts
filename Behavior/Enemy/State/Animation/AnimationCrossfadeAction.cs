using Behavior.Enemy.State.Animation;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Animation Crossfade", story: "Trigger [AnimationState] crossfade in [AnimationController]", category: "Action", id: "553a02823f79e7799504018b57b3c18a")]
public partial class AnimationCrossfadeAction : Action {
    [SerializeReference] public BlackboardVariable<AnimationStates> AnimationState;
    [SerializeReference] public BlackboardVariable<AnimationController> AnimationController;

    protected override Status OnStart() {
        var animationDetails = AnimationsParams.GetAnimationDetails(AnimationState.Value);
        AnimationController.Value.CrossfadeToState(animationDetails);
        
        return Status.Success;
    }
    protected override Status OnUpdate() {
        return Status.Running;
    }
}

