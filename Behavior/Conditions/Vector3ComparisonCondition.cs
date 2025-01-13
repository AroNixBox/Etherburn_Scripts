using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Vector3 Comparison", story: "[Vector] is [Operator] [ComparisonVector] within [Threshold]", category: "Vector Conditions", id: "158b134f613ae25d1d7edc7dc02fe78c")]
public partial class Vector3ComparisonCondition : Condition {
    [SerializeReference] public BlackboardVariable<Vector3> Vector;
    [SerializeReference] public BlackboardVariable<Vector3> ComparisonVector;
    [Comparison(comparisonType: ComparisonType.BlackboardVariables, variable: "Vector", comparisonValue: "ComparisonVector")]
    [SerializeReference] public BlackboardVariable<ConditionOperator> Operator;
    [SerializeReference] public BlackboardVariable<float> Threshold;

    public override bool IsTrue() {
        var result = Operator.Value switch {
            ConditionOperator.Equal => AreVectorsEqual(Vector.Value, ComparisonVector.Value, Threshold.Value),
            ConditionOperator.NotEqual => !AreVectorsEqual(Vector.Value, ComparisonVector.Value, Threshold.Value),
            _ => false
        };

        return result;

        bool AreVectorsEqual(Vector3 v1, Vector3 v2, float threshold) {
            return Mathf.Abs(v1.x - v2.x) <= threshold &&
                   Mathf.Abs(v1.y - v2.y) <= threshold &&
                   Mathf.Abs(v1.z - v2.z) <= threshold;
        }
    }

    public override void OnStart() {
    }

    public override void OnEnd() {
    }
}