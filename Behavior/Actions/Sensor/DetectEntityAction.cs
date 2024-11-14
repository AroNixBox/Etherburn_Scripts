using System;
using System.Collections.Generic;
using Extensions;
using Sensor;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Vision Cone Entity Detection", 
    story: "[Agent] detects [Target] with Tag [TargetType]", 
    category: "Action/Sensor", 
    id: "6190e8044eef6c4451131ef5897d760d")]
public partial class DetectEntityAction : Action {
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [Tooltip("Out Value that gets assigned with the detected target.")] 
    [SerializeReference] public BlackboardVariable<EnemyBodyParts> BodyParts;
    [SerializeReference] public BlackboardVariable<int> MaxTargets = new (10);
    [SerializeReference] public BlackboardVariable<float> DetectionRadius = new (5f);
    [Tooltip("Put to 360 to detect all around the agent.")] 
    [SerializeReference] public BlackboardVariable<float> VisionConeAngle = new (45f);
    [SerializeReference] public BlackboardVariable<EntityType> TargetType = new (EntityType.Player);

    NavMeshAgent _agent;
    VisionTargetQuery<Entity> _entityVisionTargetQuery;

    protected override Status OnStart() {
        if(ReferenceEquals(Agent.Value, null) || ReferenceEquals(Target.Value, null) || ReferenceEquals(BodyParts.Value, null)) {
            LogFailure("No Agent, Target or BodyParts assigned.");
            return Status.Failure;
        }
        
        _agent ??= Agent.Value.GetComponent<NavMeshAgent>();
        _entityVisionTargetQuery ??= new VisionTargetQuery<Entity>(
            BodyParts.Value.head, 
            BodyParts.Value.rayCheckOrigins, 
            MaxTargets.Value, 
            DetectionRadius.Value, 
            VisionConeAngle.Value
        );        
        return Status.Running;
    }

    protected override Status OnUpdate() {
        var targets = _entityVisionTargetQuery.GetAllTargetsInVisionConeSorted();
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

