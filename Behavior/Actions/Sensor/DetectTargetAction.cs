using System;
using System.Linq;
using Sensor;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DetectTarget", 
    story: "[Agent] detects [Target] with Tag [TargetType]", 
    category: "Action", 
    id: "6190e8044eef6c4451131ef5897d760d")]
public partial class DetectTargetAction : Action {
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<EntityType> TargetType = new (EntityType.Player);

    NavMeshAgent _agent;
    EntityDetectionSensor _sensor;

    protected override Status OnStart() {
        _agent ??= Agent.Value.GetComponent<NavMeshAgent>();
        _sensor ??= Agent.Value.GetComponent<EntityDetectionSensor>();
        
        return Status.Running;
    }

    protected override Status OnUpdate() {
        var targets = _sensor.GetAllTargetsInVisionConeSorted();
        if (targets.Count == 0) {
            return Status.Running;
        }
        foreach (var target in targets) {
            if (target.EntityType == TargetType.Value) {
                Target.Value = target.gameObject;
                return Status.Success;
            }
        }
        return Status.Running;
    }

    protected override void OnEnd() { }
}

