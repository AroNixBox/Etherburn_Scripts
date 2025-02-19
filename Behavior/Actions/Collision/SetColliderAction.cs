using System;
using System.Collections.Generic;
using System.Linq;
using Sensor;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Weapons Active State", story: "Set [Weapons] [Active]", category: "Action/Collision", id: "c9e82f8506b0edb29e0e3f35e663f9da")]
public partial class SetColliderAction : Action
{
    [SerializeReference] public BlackboardVariable<List<GameObject>> Weapons;
    [SerializeReference] public BlackboardVariable<bool> Active;

    protected override Status OnStart() {
        var missingType = MissingType();
        if(missingType != null) {
            Debug.LogError($"{missingType} is missing.");
            return Status.Failure;
        }

        var weapons = Weapons.Value
            .Select(weapon => weapon.GetComponent<DamageDealingObject>())
            .Where(damageDealingObject => damageDealingObject != null)
            .ToList();

        if (weapons.Count != Weapons.Value.Count) {
            Debug.LogError("One or more weapons do not have a DamageDealingObject component.");
            return Status.Failure;
        }

        foreach (var weapon in weapons) {
            weapon.CastForObjects(Active.Value);
        }
        
        return Status.Success;
    }
    
    Type MissingType() {
        if(ReferenceEquals(Weapons.Value, null) || Weapons.Value.Count == 0) { return typeof(GameObject); }
        return null;
    }

    protected override Status OnUpdate() {
        return Status.Success;
    }

    protected override void OnEnd() { }
}

