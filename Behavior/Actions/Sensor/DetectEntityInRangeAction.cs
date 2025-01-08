using System;
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
    Entity _associatedPlayerEntity;
    
    TargetEntitiesUnregisteredChannel _targetEntityUnregisteredBbv;
    const string EntityUnregisteredChannelName = "TargetEntitiesUnregisteredChannel";

    protected override Status OnStart() {
        var missingType = MissingType();
        if (missingType != null) {
            LogFailure($"Missing Type: {missingType}");
            return Status.Failure;
        }

        var entityManager = EntityManager.Instance;
        if (entityManager == null) {
            Debug.LogError("Entity Manager is not in the Scene!");
            return Status.Failure;
        }

        TargetEntitiesUnregisteredChannel targetEntitiesUnregisteredChannel = null;
        _associatedPlayerEntity ??= entityManager.GetEntityOfType(TargetType.Value, out targetEntitiesUnregisteredChannel);

        if (_associatedPlayerEntity == null) {
            Debug.LogError($"{TargetType.Value} Entity not found");
            return Status.Failure;
        }

        if (targetEntitiesUnregisteredChannel != null && _targetEntityUnregisteredBbv == null) {
            if (!Agent.Value.TryGetComponent(out BehaviorGraphAgent behaviorGraphAgent)) {
                Debug.LogError("BehaviorGraphAgent is not Attached to the Agent GameObject");
                return Status.Failure;
            }

            if (!behaviorGraphAgent.BlackboardReference.GetVariableValue(EntityUnregisteredChannelName, out TargetEntitiesUnregisteredChannel assignedEntityUnregisteredChannel)) {
                Debug.LogError($"Blackboard variable: {EntityUnregisteredChannelName} could not be set, the variable name is incorrect or the variable does not exist in the blackboard");
                return Status.Failure;
            }

            if (assignedEntityUnregisteredChannel.name != targetEntitiesUnregisteredChannel.name) {
                if (!behaviorGraphAgent.BlackboardReference.SetVariableValue(EntityUnregisteredChannelName, targetEntitiesUnregisteredChannel)) {
                    Debug.LogError($"Blackboard variable: {EntityUnregisteredChannelName} could not be set, the variable name is incorrect or the variable does not exist in the blackboard");
                    return Status.Failure;
                }
            }
        }

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
        return ReferenceEquals(Agent.Value, null) ? typeof(GameObject) : null;
    }

    protected override Status OnUpdate() {
        var targetsSorted = _entityVisionTargetQuery.GetTargetInRangeWithOutVisionCone(_associatedPlayerEntity);
        
        Target.Value = targetsSorted.gameObject;
        return Status.Success;
    }

    protected override void OnEnd() { }
}

