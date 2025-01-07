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
[NodeDescription(name: "Range Entity Detection", story: "[Agent] detects [TargetType] in Range", category: "Action", id: "687ac00191772deb9c507553831b070d")]
public partial class DetectEntityInRangeAction : Action {
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> DetectionRadius = new (10f);
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<EntityType> TargetType = new (EntityType.Player);
    [SerializeReference] public BlackboardVariable<bool> ShowDebug;

    NavMeshAgent _agent;
    VisionTargetQuery<Entity> _entityVisionTargetQuery;
    List<Entity> _associatedPlayerEntities;

    protected override Status OnStart() {
        var missingType = MissingType();
        if(missingType != null) {
            LogFailure($"Missing Type: {missingType}");
            return Status.Failure;
        }
        
        // Get our Player
        var entityManager = EntityManager.Instance;
        if (entityManager == null) {
            Debug.LogError("Entity Manager ist not in the Scene!");
            return Status.Failure;
        }
            
        _associatedPlayerEntities ??= entityManager.GetEntitiesOfType(EntityType.Player);
        
        _agent ??= Agent.Value.GetComponent<NavMeshAgent>();
        _entityVisionTargetQuery ??= new VisionTargetQuery<Entity>.Builder()
                .SetHead(Agent.Value.transform)
                .SetDetectionRadius(DetectionRadius.Value)
                .SetDebug(ShowDebug.Value)
                .Build<Entity>();
        
        
        Application.quitting += () => _entityVisionTargetQuery.Dispose();
        return Status.Running;
    }

    Type MissingType() {
        if(ReferenceEquals(Agent.Value, null)) { return typeof(GameObject); }

        return null;
    }

    protected override Status OnUpdate() {
        var targetsSorted = _entityVisionTargetQuery.GetAllTargetsInRangeWithOutLineOfSightSorted(_associatedPlayerEntities);
        if(targetsSorted.Count == 0) {
            return Status.Failure;
        }
        
        Target.Value = targetsSorted.First().gameObject;
        return Status.Success;
    }

    protected override void OnEnd() { }
}

