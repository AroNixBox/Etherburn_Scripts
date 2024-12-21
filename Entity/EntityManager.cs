using System.Collections.Generic;
using System.Threading.Tasks;
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
    public async Task WaitTillInitialized() {
        // Wait until there is at least one entity in the scene and that entity is a player
        while (_entities.Count == 0 || !_entities.Exists(entity => entity.EntityType == EntityType.Player)) {
            await Task.Yield();
        }
    }
    public List<Entity> GetEntitiesOfType(EntityType entityType) {
        return _entities.FindAll(entity => entity.EntityType == entityType);
    }
}
