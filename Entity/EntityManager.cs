using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour {
    readonly List<Entity> _entities = new ();
    public static EntityManager Instance { get; private set; }
    
    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    
    public void RegisterEntity(Entity entity) {
        _entities.Add(entity);
    }
    
    public void UnregisterEntity(Entity entity) {
        _entities.Remove(entity);
    }
    
    public List<Entity> GetEntitiesOfType(EntityType entityType) {
        return _entities.FindAll(entity => entity.EntityType == entityType);
    }
}
