using Attribute;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Attribute Threshold", story: "[Attribute] is [Operator] than [Threshold]", category: "Conditions", id: "90c74c3aa019e9b24ed1f3e7b3cb59fe")]
public partial class AttributeThresholdCondition : Condition {
    [SerializeReference] public BlackboardVariable<float> Attribute;
    [SerializeReference] public BlackboardVariable<float> Threshold;
    [Comparison(comparisonType: ComparisonType.All)]
    [SerializeReference] public BlackboardVariable<ConditionOperator> Operator;

    public override bool IsTrue() {
        return Operator.Value switch {
            ConditionOperator.Greater => Attribute.Value > Threshold.Value,
            ConditionOperator.GreaterOrEqual => Attribute.Value >= Threshold.Value,
            ConditionOperator.LowerOrEqual => Attribute.Value <= Threshold.Value,
            ConditionOperator.Lower => Attribute.Value < Threshold.Value,
            ConditionOperator.Equal => Mathf.Approximately(Attribute.Value, Threshold.Value),
            ConditionOperator.NotEqual => !Mathf.Approximately(Attribute.Value, Threshold.Value),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override void OnStart() { }

    public override void OnEnd() { }
}
