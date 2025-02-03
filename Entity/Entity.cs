using UnityEngine;

public class Entity : MonoBehaviour {
    [field: SerializeField] public EntityType EntityType { get; private set; }
    EntityManager _entityManager;
    void Start() {
        _entityManager = EntityManager.Instance;
        
        if (_entityManager == null) {
            Debug.LogError("EntityManager is not set in the inspector", transform);
            return;
        }
        
        EntityManager.Instance.RegisterEntity(this);
    }

    // TODO: If disabling scenes, revisit this. Might need to unregister OnDisable or smt..
    void OnDestroy() {
        if (_entityManager == null) {
            Debug.Log("EntityManager already destroyed");
        }

        _entityManager.UnregisterEntity(this);
    }
}
public enum EntityType {
    Player,
    Enemy,
    NPC
}
