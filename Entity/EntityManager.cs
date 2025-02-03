using System.Collections.Generic;
using System.Threading.Tasks;
using Extensions;
using UnityEngine;

public class EntityManager : Singleton<EntityManager> {
    readonly List<Entity> _entities = new ();
    readonly Dictionary<EntityType, TargetEntitiesUnregisteredChannel> _onEntityUnregisteredChannels = new ();
    
    public void RegisterEntity(Entity entity) {
        _entities.Add(entity);
        
        // Add a channel for the entity type if it does not exist
        if(_onEntityUnregisteredChannels.ContainsKey(entity.EntityType)) { return; }
        var channel = ScriptableObject.CreateInstance<TargetEntitiesUnregisteredChannel>();
        channel.name = entity.EntityType + "_unregisteredChannel";
        _onEntityUnregisteredChannels.Add(entity.EntityType, channel);
    }
    
    public void UnregisterEntity(Entity entity) {
        _entities.Remove(entity);
        
        // Handle if there is no more entities of that type
        if (_entities.FindAll(e => e.EntityType == entity.EntityType).Count != 0) { return; }
        var entityUnregisteredChannel = _onEntityUnregisteredChannels[entity.EntityType];
            
        if (entityUnregisteredChannel == null) {
            Debug.LogError($"No channel found for {entity.EntityType}");
            return;
        }
            
        entityUnregisteredChannel.SendEventMessage();
    }
    public async Task WaitTillInitialized() {
        // Wait until there is at least one entity in the scene and that entity is a player
        while (_entities.Count == 0 || !_entities.Exists(entity => entity.EntityType == EntityType.Player)) {
            await Task.Yield();
        }
    }
    public List<Entity> GetEntitiesOfType(EntityType entityType, out TargetEntitiesUnregisteredChannel targetEntitiesUnregisteredChannel) {
        var entitiesOfType = _entities.FindAll(entity => entity.EntityType == entityType);
        
        // No Entities of that type found
        if(entitiesOfType.Count == 0) {
            targetEntitiesUnregisteredChannel = null; // Return null for the channel
            return new List<Entity>(); // Return an empty list
        }
        
        // No channel found for the entity type, but at least one entity of that type exists
        if(_onEntityUnregisteredChannels[entityType] == null) {
            Debug.LogError($"No channel found for {entityType}, not returning any entity");
            targetEntitiesUnregisteredChannel = null;
            return null;
        }
        
        // Return the channel for the entity type
        targetEntitiesUnregisteredChannel = _onEntityUnregisteredChannels[entityType];
        
        return entitiesOfType; // Return the entities of that type
    }
    
    public Entity GetEntityOfType(EntityType entityType, out TargetEntitiesUnregisteredChannel targetEntitiesUnregisteredChannel) {
        var entitiesOfType = _entities.FindAll(entity => entity.EntityType == entityType);
        if (entitiesOfType.Count > 1) {
            Debug.LogError($"Multiple entities of type {entityType} found, asking for one though.");
        }
        
        // At least one entity of that type found and no channel found for the entity type
        if(_onEntityUnregisteredChannels[entityType] == null) {
            Debug.LogError($"No channel found for {entityType}, not returning any entity");
            targetEntitiesUnregisteredChannel = null;
            return null;
        }
        
        targetEntitiesUnregisteredChannel = _onEntityUnregisteredChannels[entityType]; // Return the channel for the entity type
        return System.Linq.Enumerable.FirstOrDefault(entitiesOfType); // Return the first entity of that type
    }
}
