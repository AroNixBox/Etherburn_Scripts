using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Death Cleanup", story: "[Animator] detatches from [Self] and Destruct", category: "Action", id: "b1beea965995b446234087d98938e5aa")]
public partial class DeathCleanupAction : Action
{
    [SerializeReference] public BlackboardVariable<Animator> Animator;
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    protected override Status OnStart() {
        SeperateAnimatorFromSelf();
        DestructAllComponentsNextToAnimator();
        UnityEngine.Object.Destroy(Self.Value);
        
        return Status.Success;
    }

    void SeperateAnimatorFromSelf() {
        Animator.Value.transform.SetParent(null);
    }

    void DestructAllComponentsNextToAnimator() {
        var componentsOnAnimatorGo = Animator.Value.GetComponents<Component>();
        foreach (var component in componentsOnAnimatorGo) {
            if(component is Animator) { continue; }
            if(component is Transform) { continue; }
            if(component is Enemy.EnemyEventForward) { continue; }
            
            UnityEngine.Object.Destroy(component);
        }
    }

    protected override Status OnUpdate() {
        return Status.Success;
    }

    protected override void OnEnd() { }
}

