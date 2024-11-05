using System;
using Sensor;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DetectTarget", 
    story: "[Agent] detects [Target]", 
    category: "Action", 
    id: "6190e8044eef6c4451131ef5897d760d")]
public partial class DetectTargetAction : Action {
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    NavMeshAgent _agent;
    TargetDetectionSensor _sensor;

    protected override Status OnStart() {
        _agent ??= Agent.Value.GetComponent<NavMeshAgent>();
        _sensor ??= Agent.Value.GetComponent<TargetDetectionSensor>();
        
        return Status.Running;
    }

    protected override Status OnUpdate() {
        var target = _sensor.GetNearestTargetInVisionCone();
        if(target == null) return Status.Running;
        
        Target.Value = target.gameObject;
        Debug.Log($"Detected target: {target.name}");
        return Status.Success;
    }

    protected override void OnEnd() { }
}

