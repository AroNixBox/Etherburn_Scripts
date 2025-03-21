using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Target to Position", story: "Set [Target] to [Position]", category: "Action/Transform", id: "233dc3cde0a56a5b32f9dc547b23af8b")]
public partial class SetTargetToPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<Vector3> Position;
    GameObject _actualTarget;
    protected override Status OnStart() {
        var uniqueId = "Target " + Guid.NewGuid();
        if (_actualTarget == null) {
            _actualTarget = new GameObject(uniqueId);
            
            if(_actualTarget.scene != GameObject.scene) {
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(_actualTarget, GameObject.scene);
            }
        }
        
        
        Target.Value = _actualTarget;
        Target.Value.transform.position = Position.Value;
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

