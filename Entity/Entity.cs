using System;
using UnityEngine;

public class Entity : MonoBehaviour {
    [field: SerializeField] public EntityType EntityType { get; private set; }

    void Start() {
        EntityManager.Instance.RegisterEntity(this);
    }

    // TODO: If disabling scenes, revisit this. Might need to unregister OnDisable or smt..
    void OnDestroy() {
        EntityManager.Instance.UnregisterEntity(this);
    }
}
public enum EntityType {
    Player,
    Enemy,
    NPC
}
