using System;
using Behavior.Conditions;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is Transform on NavMesh", story: "[Transform] [Condition] reachable by [Agent] on NavMesh", category: "Conditions", id: "73e3bcaed5856e50f1d708b039920105")]
public partial class IsTransformOnNavMeshCondition : Condition
{
    [SerializeReference] public BlackboardVariable<Transform> Transform;
    [SerializeReference] public BlackboardVariable<NavMeshAgent> Agent;
    [SerializeReference] public BlackboardVariable<Comparison> Condition;

    public override bool IsTrue() {
        if (!NavMesh.SamplePosition(Transform.Value.position, out var hit, 0.1f, NavMesh.AllAreas)) {
            return Condition.Value == Comparison.IsNot;
        }
    
        NavMeshPath path = new NavMeshPath();
        bool isPathComplete = Agent.Value.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete;
    
        return Condition.Value == Comparison.Is ? isPathComplete : !isPathComplete;
    }
    public override void OnStart() { }

    public override void OnEnd() { }
}
