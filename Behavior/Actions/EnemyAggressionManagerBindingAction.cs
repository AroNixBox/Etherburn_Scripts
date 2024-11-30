using System;
using Enemy.Positioning;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "EnemyAggressionManager Binding", story: "[self] [bindingState] to EnemyAggressionManager", category: "Action/Managed", id: "0a7bbd810f2549b8bd873d121c464034")]
public partial class EnemyAggressionManagerBindingAction : Action {
    [SerializeReference] public BlackboardVariable<GameObject> self;
    [SerializeReference] public BlackboardVariable<BindingState> bindingState;
    [SerializeReference] public BlackboardVariable<OptimalPositionChanged> OptimalPositionChangedChannel;
    protected override Status OnStart() {
        if(ReferenceEquals(self, null)) {
            return Status.Failure;
        }
        
        switch (bindingState.Value) {
            case BindingState.Register:
                EnemyAggressionManager.Instance.RegisterEnemy(self.Value, OptimalPositionChangedChannel.Value);
                break;
            
            case BindingState.Unregister:
                EnemyAggressionManager.Instance.UnregisterEnemy(self.Value);
                break;

            default:
                Debug.LogError("Unknown BindingState");
                return Status.Failure;
        }
        
        return Status.Success;
    }

    protected override Status OnUpdate() {
        return Status.Success;
    }

    protected override void OnEnd() { }
    public enum BindingState {
        Register,
        Unregister
    }
}

