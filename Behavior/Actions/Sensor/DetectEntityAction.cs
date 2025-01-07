using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
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
    [SerializeReference] public BlackboardVariable<float> DetectionRadius = new (5f);
    [Tooltip("Put to 360 to detect all around the agent.")] 
    [SerializeReference] public BlackboardVariable<float> VisionConeAngle = new (45f);
    [SerializeReference] public BlackboardVariable<EntityType> TargetType = new (EntityType.Player);
    [SerializeReference] public BlackboardVariable<bool> ShowDebug;

    NavMeshAgent _agent;
    VisionTargetQuery<Entity> _entityVisionTargetQuery;
    List<Entity> _associatedPlayerEntities;

    protected override Status OnStart() {
        var missingType = MissingType();
        if (missingType != null) {
            Debug.LogError($"Missing Type: {missingType}");
            return Status.Failure;
        }
        
        // Get our Player
        var entityManager = EntityManager.Instance;
        if (entityManager == null) {
            Debug.LogError("Entity Manager ist not in the Scene!");
            return Status.Failure;
        }
            
        _associatedPlayerEntities ??= entityManager.GetEntitiesOfType(TargetType.Value);
        
        _agent ??= Agent.Value.GetComponent<NavMeshAgent>();
        _entityVisionTargetQuery ??= new VisionTargetQuery<Entity>.Builder()
                .SetHead(BodyParts.Value.head)
                .SetRayCheckOrigins(BodyParts.Value.rayCheckOrigins)
                .SetDetectionRadius(DetectionRadius.Value)
                .SetVisionConeAngle(VisionConeAngle.Value)
                .SetDebug(ShowDebug.Value)
                .Build<Entity>();
        
        
        Application.quitting += () => _entityVisionTargetQuery.Dispose();
        return Status.Running;
    }

    Type MissingType() {
        if(ReferenceEquals(Agent.Value, null)) { return typeof(GameObject); }
        if(ReferenceEquals(BodyParts.Value, null)) { return typeof(EnemyBodyParts); }
        
        return null;
    }

    protected override Status OnUpdate() {
        var targets = _entityVisionTargetQuery.GetAllTargetsInVisionConeSorted(_associatedPlayerEntities);
        if(targets.Count == 0) {
            return Status.Failure;
        }

        Target.Value = targets.First().gameObject;
        return Status.Success;
    }

    protected override void OnEnd() { }
}

