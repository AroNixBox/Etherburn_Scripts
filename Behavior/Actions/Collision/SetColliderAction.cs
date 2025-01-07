using System;
using Sensor;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Weapon Active State", story: "Set [Weapon] [Active]", category: "Action/Collision", id: "c9e82f8506b0edb29e0e3f35e663f9da")]
public partial class SetColliderAction : Action
{
    [SerializeReference] public BlackboardVariable<DamageDealingObject> Weapon;
    [SerializeReference] public BlackboardVariable<bool> Active;

    protected override Status OnStart() {
        var missingType = MissingType();
        if(missingType != null) {
            Debug.LogError($"{missingType} is missing.");
            return Status.Failure;
        }
        
        Weapon.Value.CastForObjects(Active.Value);
        return Status.Success;
    }
    
    Type MissingType() {
        if(ReferenceEquals(Weapon.Value, null)) { return typeof(DamageDealingObject); }
        return null;
    }

    protected override Status OnUpdate() {
        return Status.Success;
    }

    protected override void OnEnd() { }
}

