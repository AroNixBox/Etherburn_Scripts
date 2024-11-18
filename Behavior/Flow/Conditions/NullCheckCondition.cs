using System;
using Unity.Behavior;
using UnityEngine;

namespace Behavior.Flow.Conditions {
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "Entity Null Check", story: "[Entity] [NullComparison] null", category: "Variable Conditions", id: "28a70606683fa1e36a9a36db4e19cfc5")]
    public partial class NullCheckCondition : Condition {
        [SerializeReference] public BlackboardVariable<GameObject> Entity;
        [SerializeReference] public BlackboardVariable<Comparison> NullComparison;

        public override bool IsTrue() {
            return NullComparison.Value switch {
                Comparison.Is => Entity.Value == null,
                Comparison.IsNot => Entity.Value != null,
                _ => false
            };
        }

        public override void OnStart() { }

        public override void OnEnd() { }
    }
}