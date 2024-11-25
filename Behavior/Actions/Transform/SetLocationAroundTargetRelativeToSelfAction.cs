using System;
using Enemy;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Location around Target relative to Self", story: "Set [DistanceLocationProvider] around [Target] relative to [Self]", category: "Action/Transform", id: "12b7d4807c5bc0a2141929090402fd5b")]
public partial class SetLocationAroundTargetRelativeToSelfAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> DistanceLocationProvider;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    // Should be the Max Distance Attack Distane (Hard Reset)
    [SerializeReference] public BlackboardVariable<float> DistanceToTarget = new (2f);
    Target _targetProvider;

    protected override Status OnStart() {
        if(ReferenceEquals(Target?.Value, null)) {
            Debug.LogError("Target is missing.");
            return Status.Failure;
        }
        if(ReferenceEquals(Self?.Value, null)) {
            Debug.LogError("Self is missing.");
            return Status.Failure;
        }
        
        if(_targetProvider == null) {
            var newLocationProvider = new GameObject("LocationProvider " + Self.Value.name);
            _targetProvider = new Target(newLocationProvider.transform, Self.Value.transform, Target.Value.transform, DistanceToTarget.Value);
        }
        
        _targetProvider.RotateAroundParent();
        // Actually we dont need the LookAt
        _targetProvider.LookAtParent();
        
        DistanceLocationProvider.Value = _targetProvider.GetTransform();
        
        return Status.Success;
    }

    protected override Status OnUpdate() {
        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

