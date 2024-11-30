using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Inform EnemyAgressionManager about AttackState", story: "[Self] informs EnemyAggressionManager that Attack [attackState]", category: "Action/Managed", id: "248c5e1c9b7b7cb8c3be734f509accb4")]
public partial class InformEnemyAgressionManagerAboutAttackStateAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<AttackState> attackState;

    protected override Status OnStart()
    {
        switch (attackState.Value) {
            case AttackState.Started:
                // TODO: Inform Manager that the attack has started
                break;
            case AttackState.Ended:
                // TODO: Inform Manager that the attack has ended
                break;
            default:
                Debug.LogError("Unknown BindingState");
                return Status.Failure;
        }
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
    public enum AttackState
    {
        Started,
        Ended
    }
}

