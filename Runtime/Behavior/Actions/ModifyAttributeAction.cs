using Attributes;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ModifyAttribute", story: "[Modify] [Attribute] by [Amount]", category: "Action", id: "3e532d4c081d9aa6f610e5211bb2dbc5")]
public partial class ModifyAttributeAction : Action
{
    [SerializeReference] public BlackboardVariable<EnergyBase> Attribute;
    [SerializeReference] public BlackboardVariable<float> Amount;
    [SerializeReference] public BlackboardVariable<ModifyType> Modify;

    protected override Status OnStart() {
        switch (Modify.Value) {
            case ModifyType.Increase:
                Attribute.Value.Increase(Amount.Value);
                return Status.Success;
            case ModifyType.Decrease:
                Attribute.Value.Decrease(Amount.Value);
                return Status.Success;
            default:
                return Status.Failure;
        }
    }

    protected override Status OnUpdate() {
        return Status.Success;
    }

    protected override void OnEnd() { }
    
    public enum ModifyType {
        Increase,
        Decrease
    }
}

