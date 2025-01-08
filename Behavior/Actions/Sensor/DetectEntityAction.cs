using System;
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
    Entity _associatedPlayerEntity;
    
    TargetEntitiesUnregisteredChannel _targetEntityUnregisteredBbv;
    const string EntityUnregisteredChannelName = "TargetEntitiesUnregisteredChannel";

    protected override Status OnStart() {
        var missingType = MissingType();
        if (missingType != null) {
            Debug.LogError($"Missing Type: {missingType}");
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
        return ReferenceEquals(BodyParts.Value, null) ? typeof(EnemyBodyParts) : null;
    }

    protected override Status OnUpdate() {
        var target = _entityVisionTargetQuery.GetTargetInRangeAndVisionCone(_associatedPlayerEntity);
        
        if (target == null) {
            return Status.Failure;
        }

        Target.Value = target.gameObject;
        return Status.Success;
    }

    protected override void OnEnd() { }
}

